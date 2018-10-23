using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class HttpCachingHandlerTestsFixture : IDisposable
    {
        public HttpCachingHandlerTestsFixture()
        {
            ResponseCache = new IntegrationTestResponseCache();

            var httpClient = new HttpClient(new HttpCachingHandler(ResponseCache, new IntegrationTestHandler()));

            var request1 = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-Text-Document.txt");
            request1.Headers.CacheControl = new CacheControlHeaderValue { NoStore = false };

            httpClient.SendAsync(request1);

            var request2 = new HttpRequestMessage(HttpMethod.Get, "http://integration-testing/Test-JPEG-file.jpg");
            request2.Headers.CacheControl = new CacheControlHeaderValue { NoStore = false };

            httpClient.SendAsync(request2);
        }
        public IntegrationTestResponseCache ResponseCache { get; private set; }

        public void Dispose()
        {
            ResponseCache.DeleteCache();
        }        
    }
}
