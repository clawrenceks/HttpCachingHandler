using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class HttpCachingHandlerTests : IClassFixture<HttpCachingHandlerTestsFixture>
    {
        private readonly HttpCachingHandlerTestsFixture _fixture;

        public HttpCachingHandlerTests(HttpCachingHandlerTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_TextDocument_BypassingCache_AndReadingResult_AsString()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");
            requestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultString = await result.Content.ReadAsStringAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedString = await File.ReadAllTextAsync(Path.Combine(applicationDirectory, "TestData", "Test-Text-Document.txt"));
            Assert.Equal(expectedString, resultString);
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_TextDocument_NotBypassingCache_AndReadingResults_AsString()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultString = await result.Content.ReadAsStringAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedString = await File.ReadAllTextAsync(Path.Combine(applicationDirectory, "TestData", "Test-Text-Document.txt"));
            Assert.Equal(expectedString, resultString);
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_TextDocument_BypassingCache_AndReadingResult_AsBytes()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");
            requestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultBytes = await result.Content.ReadAsByteArrayAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedBytes = await File.ReadAllBytesAsync(Path.Combine(applicationDirectory, "TestData", "Test-Text-Document.txt"));
            Assert.Equal(expectedBytes, resultBytes);
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_TextDocument_NotBypassingCache_AndReadingResults_AsBytes()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultBytes = await result.Content.ReadAsByteArrayAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedBytes = await File.ReadAllBytesAsync(Path.Combine(applicationDirectory, "TestData", "Test-Text-Document.txt"));
            Assert.Equal(expectedBytes, resultBytes);
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_JpegDocument_BypassingCache_AndReadingResults_AsBytes()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-JPEG-file.jpg");
            requestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultBytes = await result.Content.ReadAsByteArrayAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedBytes = await File.ReadAllBytesAsync(Path.Combine(applicationDirectory, "TestData", "Test-Jpeg-file.jpg"));
            Assert.Equal(expectedBytes, resultBytes);
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_JpegDocument_NotBypassingCache_AndReadingResults_AsBytes()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-JPEG-file.jpg");

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultBytes = await result.Content.ReadAsByteArrayAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedBytes = await File.ReadAllBytesAsync(Path.Combine(applicationDirectory, "TestData", "Test-Jpeg-file.jpg"));
            Assert.Equal(expectedBytes, resultBytes);
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_TextDocument_BypassingCache_AndReadingResult_AsStream()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");
            requestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultStream = await result.Content.ReadAsStreamAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var expectedStream = File.OpenRead(Path.Combine(applicationDirectory, "TestData", "Test-Text-Document.txt")))
            {
                Assert.Equal(expectedStream.Length, resultStream.Length);
                Assert.Equal(0, expectedStream.Position);
            }
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_TextDocument_NotBypassingCache_AndReadingResults_AsStream()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultStream = await result.Content.ReadAsStreamAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var expectedStream = File.OpenRead(Path.Combine(applicationDirectory, "TestData", "Test-Text-Document.txt")))
            {
                Assert.Equal(expectedStream.Length, resultStream.Length);
                Assert.Equal(0, expectedStream.Position);
            }
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_JpegDocument_BypassingCache_AndReadingResults_AsStream()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-JPEG-file.jpg");
            requestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultStream = await result.Content.ReadAsStreamAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var expectedStream = File.OpenRead(Path.Combine(applicationDirectory, "TestData", "Test-Jpeg-file.jpg")))
            {
                Assert.Equal(expectedStream.Length, resultStream.Length);
                Assert.Equal(0, expectedStream.Position);
            }
        }

        [Fact]
        public async Task HttpResponseMessage_ContainsCorrect_ResponseBody_WhenRequesting_JpegDocument_NotBypassingCache_AndReadingResults_AsStream()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache, new IntegrationTestHandler()));
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-JPEG-file.jpg");

            //Act
            var result = await httpClient.SendAsync(requestMessage);
            var resultStream = await result.Content.ReadAsStreamAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (var expectedStream = File.OpenRead(Path.Combine(applicationDirectory, "TestData", "Test-Jpeg-file.jpg")))
            {
                Assert.Equal(expectedStream.Length, resultStream.Length);
                Assert.Equal(0, expectedStream.Position);
            }
        }
    }
}