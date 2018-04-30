using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Clawrenceks.HttpCachingHandler.UnitTests
{
    public class FakeDelegatingHttpHandler : DelegatingHandler
    {
        private HttpResponseMessage _httpResponseToReturn;

        public FakeDelegatingHttpHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendAsyncCalled = true;
            PreviousRequestHeaders = request.Headers;

            if (_httpResponseToReturn != null)
            {
                _httpResponseToReturn.RequestMessage = request;
            }

            return Task.FromResult(HttpResponseToReturn);
        }

        public bool SendAsyncCalled { get; private set; }

        public HttpRequestHeaders PreviousRequestHeaders { get; private set; }

        public HttpResponseMessage HttpResponseToReturn
        {
            get => _httpResponseToReturn;
            set
            {
                _httpResponseToReturn = value;
            }
        }
    }
}
