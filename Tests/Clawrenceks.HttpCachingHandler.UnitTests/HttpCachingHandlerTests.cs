using Clawrenceks.HttpCachingHandler.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Clawrenceks.HttpCachingHandler.UnitTests
{
    public class HttpCachingHandlerTests
    {
        private Mock<IResponseCache> _mockResponseCache;

        public HttpCachingHandlerTests()
        {
            _mockResponseCache = new Mock<IResponseCache>();
        }

        [Fact]
        public void ParameterlessCtor_SetsInnerHandler_ToHttpClientHandler()
        {
            //Arrange
            var sut = new HttpCachingHandler(_mockResponseCache.Object);

            //Assert
            Assert.IsType<HttpClientHandler>(sut.InnerHandler);
        }

        [Fact]
        public void Ctor_SetsInnerHandler_ToPassedHttpHandler()
        {
            //Arrange
            var sut = new HttpCachingHandler(_mockResponseCache.Object, new FakeDelegatingHttpHandler());

            //Assert
            Assert.IsType<FakeDelegatingHttpHandler>(sut.InnerHandler);
        }

        [Fact]
        public async Task SendAsync_ExecutesInnerHandler_WhenResponseCache_IsNull()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(null, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.True(testHandler.SendAsyncCalled);
        }

        [Fact]
        public async Task SendAsync_ExecutesInnerHandler_WhenRequest_Contains_NoCache_CacheControlHeader()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true };
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.True(testHandler.SendAsyncCalled);
        }

        [Fact]
        public async Task SendAsync_ExecutesInnerHandler_WhenRequestIsNotAGetRequest()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.True(testHandler.SendAsyncCalled);
        }

        [Fact]
        public async Task SendAsync_ExecutesInnerHandler_GivenGetRequest_WhenCacheContainsNoResponseForRequest()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.True(testHandler.SendAsyncCalled);
        }

        [Fact]
        public async Task SendAsync_ExecutesInnerHandler_GivenGetRequest_WhenCacheContainsStaleContent()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            testHandler.HttpResponseToReturn = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.True(testHandler.SendAsyncCalled);
        }

        [Fact]
        public async Task SendAsync_AddsIfNonMatchedHeader_ToRequest_WhenCacheContainsStaleContent_AndAnEtagIsPresent()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.GetETag(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns("\"123\"");

            testHandler.HttpResponseToReturn = new HttpResponseMessage { StatusCode = HttpStatusCode.NotModified };


            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.Single(testHandler.PreviousRequestHeaders.IfNoneMatch);
            Assert.Equal("\"123\"", testHandler.PreviousRequestHeaders.IfNoneMatch.First().Tag);
        }

        [Fact]
        public async Task SendAsync_DoesNotAddIfNoneMatchedHeader_ToRequest_WhenCachedReponse_HasNoEtag()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.GetETag(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(() => null);

            testHandler.HttpResponseToReturn = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.Empty(testHandler.PreviousRequestHeaders.IfNoneMatch);
        }

        [Fact]
        public async Task SendAsync_DoesNotCallSendAsync_OnInnerHandler_WhenCacheHasFreshContent()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(false);

            var bytes = Encoding.UTF8.GetBytes("My cached response content");
            var cacheContent = Convert.ToBase64String(bytes);

            _mockResponseCache.Setup(c => c.Get(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(cacheContent);

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.False(testHandler.SendAsyncCalled);
        }

        [Fact]
        public async Task SendAsync_ReturnsResult_WithStatusCode200_WhenCacheHasFreshContent()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(false);

            var bytes = Encoding.UTF8.GetBytes("My cached response content");
            var cacheContent = Convert.ToBase64String(bytes);

            _mockResponseCache.Setup(c => c.Get(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(cacheContent);

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task SendAsync_ReturnsResult_WithCorrectReponseContent_WhenCacheHasFreshContent()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(false);

            var bytes = Encoding.UTF8.GetBytes("My cached response content");
            var cacheContent = Convert.ToBase64String(bytes);

            _mockResponseCache.Setup(c => c.Get(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(cacheContent);

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.Equal("My cached response content", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SendAsync_DoesNotCallAdd_OnReponseCache_WhenHttpResponse_HasNoCacheControlHeader()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Never);
        }

        [Fact]
        public async Task SendAsync_DoesNotCallAdd_OnReponseCache_WhenHttpResponse_HasControlHeader_SetToNoCache()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Never);
        }

        [Fact]
        public async Task SendAsync_DoesNotCallAdd_OnReponseCache_WhenHttpResponse_HasControlHeader_SetToNoStore()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Never);
        }

        [Fact]
        public async Task SendAsync_DoesNotCallAdd_OnReponseCache_WhenHttpResponseCode_IsNot200()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Never);
        }

        [Fact]
        public async Task SendAsync_SetsResultContent_UsingCache_WhenReponseCode_Is304()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var cachedContent = "My Cached Response";
            var bytes = Encoding.UTF8.GetBytes(cachedContent);
            var base64EncodedCacheContent = Convert.ToBase64String(bytes);

            _mockResponseCache.Setup(c => c.Get(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(base64EncodedCacheContent);

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotModified
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.Equal("My Cached Response", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SendAsync_CallsAdd_OnResponseCache_WhenResult_CanBeCached()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Once);
        }

        [Fact]
        public async Task SendAsync_CallsAdd_OnResponseCache_WhenPreviouslyCachedResponseHasExpired_AndNewResponseHas_NoEtag()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Once);
        }

        [Fact]
        public async Task SendAsync_CallsAdd_OnResponseCache_WhenPreviouslyCachedResponseHasExpired_AndNewResponseHas_AnEtag()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };
            testResponse.Headers.ETag = new EntityTagHeaderValue("\"123456789\"");

            testHandler.HttpResponseToReturn = testResponse;

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SendAsync_PassesCorrectKey_ToAddOnResponseCache__WhenResult_CanBeCached()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.Is<string>(s => s == request.RequestUri.AbsoluteUri), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Once);
        }

        [Fact]
        public async Task SendAsync_Passes_ResponseContent_AsBase64String_ToCache_WhenResult_CanBeCached()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            var bytes = Encoding.UTF8.GetBytes("My Example Reponse");
            var expectedContent = System.Convert.ToBase64String(bytes);
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.Is<string>(s => s == expectedContent), It.IsAny<TimeSpan>(), null), Times.Once);
        }

        [Fact]
        public async Task SendAsync_PassesCorrectTimeSpan_ToCache_WhenResult_CanBeCached()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.Is<TimeSpan>(t => t.TotalSeconds == 60), null), Times.Once);
        }

        [Fact]
        public async Task SendAsync_PassesNullEtag_ToCache_WhenResult_HasNoEtagHeader_AndCanBeCached()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), null), Times.Once);
        }

        [Fact]
        public async Task SendAsync_PassesResponseEtag_ToCache_WhenResult_HasEtagHeader_AndCanBeCached()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("My Example Reponse")
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };
            testResponse.Headers.ETag = new EntityTagHeaderValue("\"123456789\"");

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            await sut.SendAsync(request, new CancellationToken());

            //Assert
            _mockResponseCache.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.Is<string>(s => s == "\"123456789\"")), Times.Once);
        }

        [Fact]
        public async Task SendAsync_ReturnsResponse_WithStatusCode200_WhenIfNoneMatchedRequest_ReturnsReponseCode_WithStatusCode304()
        {
            //Arrange
            var testHandler = new FakeDelegatingHttpHandler();
            var sut = new HttpCachingHandlerTestWrapper(_mockResponseCache.Object, testHandler);

            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("http://www.tempuri.org/myresource");

            _mockResponseCache.Setup(c => c.Get(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns("My Cached Response");

            _mockResponseCache.Setup(c => c.Exists(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.IsExpired(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns(true);

            _mockResponseCache.Setup(c => c.GetETag(It.Is<string>(s => s == request.RequestUri.AbsoluteUri)))
                .Returns("\"123\"");

            var testResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotModified
            };
            testResponse.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.FromSeconds(60) };

            testHandler.HttpResponseToReturn = testResponse;

            //Act
            var result = await sut.SendAsync(request, new CancellationToken());

            //Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}