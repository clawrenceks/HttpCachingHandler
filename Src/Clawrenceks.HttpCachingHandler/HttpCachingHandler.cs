using Clawrenceks.HttpCachingHandler.Abstractions;
using Clawrenceks.HttpCachingHandler.Extensions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Clawrenceks.HttpCachingHandler
{
    public class HttpCachingHandler : DelegatingHandler
    {
        private readonly IResponseCache _cache;

        public HttpCachingHandler(IResponseCache cache)
            : this(cache, new HttpClientHandler())
        {
        }

        public HttpCachingHandler(IResponseCache cache, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _cache = cache;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (BypassPrivateCache(request))
            {
                var response = await base.SendAsync(request, cancellationToken);
                response = await ProcessResponseCaching(response);
                return response;
            }

            var requestUrl = request.RequestUri.AbsoluteUri;

            if (_cache.IsExpired(requestUrl))
            {
                var eTag = _cache.GetETag(requestUrl);
                if (eTag != null)
                {
                    request.Headers.IfNoneMatch.TryParseAdd(eTag);
                }

                var response = await base.SendAsync(request, cancellationToken);
                response = await ProcessResponseCaching(response);
                
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    response.StatusCode = HttpStatusCode.OK;
                }

                return response;
            }

            var cachedResponse = _cache.Get(requestUrl);

            var bytes = Convert.FromBase64String(cachedResponse);


            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                //Content = new StringContent(cachedResponse)
                Content = new StreamContent(new MemoryStream(bytes))
            };

            return httpResponse;           
        }

        private async Task<HttpResponseMessage> ProcessResponseCaching(HttpResponseMessage response)
        {
            if (!ResponseIsCachable(response))
                return response;
            
            var requestUrl = response.RequestMessage.RequestUri.AbsoluteUri;

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                var cachedReponse = _cache.Get(requestUrl);

                if (cachedReponse != null)
                {
                    var base64DecodedContent = Convert.FromBase64String(cachedReponse);
                    var stream = new MemoryStream(base64DecodedContent);

                    response.Content = new StreamContent(stream);
                }
            }

            var memoryStream = new MemoryStream();
            var responseStream = await response.Content.ReadAsStreamAsync();
            responseStream.CopyTo(memoryStream);

            var bytes = memoryStream.ToArray();
            var base64EncodedContent = Convert.ToBase64String(bytes);

            var maxAgeHeader = response.Headers.CacheControl.MaxAge.Value.TotalSeconds;

            var eTag = ParseReponseEtag(response);

            _cache.Add(requestUrl,
                base64EncodedContent,
                TimeSpan.FromSeconds(maxAgeHeader),
                eTag);

            memoryStream.Position = 0;
            response.Content = new StreamContent(memoryStream);
            responseStream.Dispose();
            return response;
        }

        private string ParseReponseEtag(HttpResponseMessage response)
        {
            if (response?.Headers?.ETag == null)
            {
                return null;
            }

            return response.Headers.ETag.Tag;
        }

        private bool BypassPrivateCache(HttpRequestMessage request)
        {
            if (_cache == null)
                return true;

            if (request.ShouldBypassPrivateCache())
                return true;

            if (!_cache.Exists(request.RequestUri.AbsoluteUri))
                return true;

            return false;
        }

        private bool ResponseIsCachable(HttpResponseMessage response)
        {
            if (_cache == null)
                return false;

            if (!response.IsPrivatelyCachable())
                return false;

            return true;
        }
    }
}