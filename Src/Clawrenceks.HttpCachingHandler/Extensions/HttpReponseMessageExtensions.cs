using System;
using System.Net;
using System.Net.Http;

namespace Clawrenceks.HttpCachingHandler.Extensions
{
    public static class HttpReponseMessageExtensions
    {
        public static bool IsPrivatelyCachable(this HttpResponseMessage response)
        {
            if (response?.Headers?.CacheControl == null)
                return false;

            if (response?.Headers?.CacheControl?.NoStore == true)
                return false;

            if (!response?.Headers?.CacheControl?.MaxAge.HasValue == true)
                return false;

            if (response.Headers.CacheControl.MaxAge.Value.TotalSeconds < 1)
                return false;

            if (response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.NotModified)
            {
                return false;
            }

            return true;
        }
    }
}
