using Clawrenceks.HttpCachingHandler.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Clawrenceks.HttpCachingHandler.UnitTests.Extensions
{
    public class HttpReponseMessageExtensionsTests
    {
        [Fact]
        public void IsPrivatelyCachable_ReturnsFalse_GivenHttpResponse_WithNo_CacheControlHeader()
        {
            //Arrange
            var response = new HttpResponseMessage();

            //Assert
            Assert.False(response.IsPrivatelyCachable());
        }

        [Fact]
        public void IsPrivatelyCachable_ReturnsFalse_GivenHttpResponse_WithCacheControlHeader_ContainingNoStoreDirective()
        {
            //Arrange
            var response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };

            //Assert
            Assert.False(response.IsPrivatelyCachable());
        }

        [Fact]
        public void IsPrivatelyCachable_ReturnsFalse_GivenHttpResponse_WithCacheControlHeader_ContainingNoMaxAgeDirective()
        {
            //Arrange
            var response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue();

            //Assert
            Assert.False(response.IsPrivatelyCachable());
        }

        [Fact]
        public void IsPrivatelyCachable_ReturnsFalse_GivenHttpResponse_WithCacheControlHeader_ContainingMaxAge_ThatIsLessThanOne()
        {
            //Arrange
            var response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(0.5) };

            //Assert
            Assert.False(response.IsPrivatelyCachable());
        }

        [Fact]
        public void IsPrivatelyCachable_ReturnsFalse_GivenHttpResponse_WithResponseCode_ThatIsNot200_Or304()
        {
            //Arrange
            var response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(60) };
            response.StatusCode = HttpStatusCode.BadRequest;

            //Assert
            Assert.False(response.IsPrivatelyCachable());
        }

        [Fact]
        public void IsPrivatelyCachableReturnsTrue_GivenHttpResponse_ThatIsPrivatelyCachable_WithHttpStatusCode_OfOk()
        {
            //Arrange
            var response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(60) };
            response.StatusCode = HttpStatusCode.OK;

            //Assert
            Assert.True(response.IsPrivatelyCachable());
        }

        [Fact]
        public void IsPrivatelyCachableReturnsTrue_GivenHttpResponse_ThatIsPrivatelyCachable_WithHttpStatusCode_OfNotModified()
        {
            //Arrange
            var response = new HttpResponseMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromSeconds(60) };
            response.StatusCode = HttpStatusCode.NotModified;

            //Assert
            Assert.True(response.IsPrivatelyCachable());
        }
    }
}
