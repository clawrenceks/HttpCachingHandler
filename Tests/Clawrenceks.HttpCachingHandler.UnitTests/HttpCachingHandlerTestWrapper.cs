using Clawrenceks.HttpCachingHandler.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Clawrenceks.HttpCachingHandler.UnitTests
{
    class HttpCachingHandlerTestWrapper : HttpCachingHandler
    {
        public HttpCachingHandlerTestWrapper(IResponseCache cache, HttpMessageHandler innerHandler)
            : base(cache, innerHandler)
        {
        }
        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}
