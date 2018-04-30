﻿using Clawrenceks.HttpCachingHandler.Extensions;
using System;
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
                await ProcessResponseCaching(response);
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

                return await base.SendAsync(request, cancellationToken);
            }

            var cachedResponse = _cache.Get(requestUrl);
            var decodedCachedResponse = Convert.FromBase64String(cachedResponse);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(decodedCachedResponse)
            };

            return httpResponse;           
        }

        private async Task ProcessResponseCaching(HttpResponseMessage response)
        {
            if (!ResponseIsCachable(response))
                return;
            
            var requestUrl = response.RequestMessage.RequestUri.AbsoluteUri;

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                var cachedReponse = _cache.Get(requestUrl);

                if (cachedReponse != null)
                {
                    response.Content = new StringContent(cachedReponse);
                }
            }

            var responseContent = await response.Content.ReadAsByteArrayAsync();
            var base64Content = Convert.ToBase64String(responseContent);

            var maxAgeHeader = response.Headers.CacheControl.MaxAge.Value.TotalSeconds;

            var eTag = ParseReponseEtag(response);

            _cache.Add(requestUrl,
                base64Content,
                TimeSpan.FromSeconds(maxAgeHeader),
                eTag);
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