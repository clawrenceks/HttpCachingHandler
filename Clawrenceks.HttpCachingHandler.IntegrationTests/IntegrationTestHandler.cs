using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class IntegrationTestHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fileToReturn = request.RequestUri.AbsolutePath.Split("/").LastOrDefault();
            var fileContent = await File.ReadAllBytesAsync(Path.Combine(applicationDirectory, "TestData", fileToReturn));

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(fileContent)),
                RequestMessage = request
            };

            httpResponse.Headers.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = new TimeSpan(1, 0, 0)
            };

            return httpResponse;
        }
    }
}
