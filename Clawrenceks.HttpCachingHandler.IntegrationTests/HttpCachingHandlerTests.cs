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

        [Fact]
        public async Task temp()
        {
            //Arrange
            var httpClient = new HttpClient(new HttpCachingHandler(_fixture.ResponseCache));
            var requestMessage1 = new HttpRequestMessage(HttpMethod.Get, "https://vignette.wikia.nocookie.net/googology/images/f/f3/Test.jpeg/revision/latest?cb=20180121032443");
            requestMessage1.Headers.CacheControl = new CacheControlHeaderValue();

            var requestMessage2 = new HttpRequestMessage(HttpMethod.Get, "https://vignette.wikia.nocookie.net/googology/images/f/f3/Test.jpeg/revision/latest?cb=20180121032443");
            requestMessage1.Headers.CacheControl = new CacheControlHeaderValue();

            //Act
            var result1 = await httpClient.SendAsync(requestMessage1);
            var result1Content = await result1.Content.ReadAsStreamAsync();

            var result2 = await httpClient.SendAsync(requestMessage2);
            var result2Content = await result2.Content.ReadAsStreamAsync();

            //Assert
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var expectedStream = File.OpenRead(Path.Combine(applicationDirectory, "TestData", "Test.jpg"));

            var fileStream1 = File.Create("C:\\Projects\\Repos\\Clawrenceks.HttpCachingHandler\\Clawrenceks.HttpCachingHandler.IntegrationTests\\bin\\Debug\\netcoreapp2.1\\TestData\\f1.jpg");
            expectedStream.Seek(0, SeekOrigin.Begin);
            expectedStream.CopyTo(fileStream1);
            fileStream1.Close();

            var fileStream2 = File.Create("C:\\Projects\\Repos\\Clawrenceks.HttpCachingHandler\\Clawrenceks.HttpCachingHandler.IntegrationTests\\bin\\Debug\\netcoreapp2.1\\TestData\\f2.jpg");
            result1Content.Seek(0, SeekOrigin.Begin);
            result1Content.CopyTo(fileStream2);
            fileStream2.Close();

            var fileStream3 = File.Create("C:\\Projects\\Repos\\Clawrenceks.HttpCachingHandler\\Clawrenceks.HttpCachingHandler.IntegrationTests\\bin\\Debug\\netcoreapp2.1\\TestData\\f3.jpg");
            result2Content.Seek(0, SeekOrigin.Begin);
            result2Content.CopyTo(fileStream3);
            fileStream3.Close();

            Assert.Equal(expectedStream.Length, result1Content.Length);
            expectedStream.Close();
        }
    }
}