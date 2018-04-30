using System.Net.Http;

namespace Clawrenceks.HttpCachingHandler.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static bool ShouldBypassPrivateCache(this HttpRequestMessage request)
        {
            if (request?.Headers?.CacheControl?.NoCache == true)
            return true;

            if (request?.Method != HttpMethod.Get)
                return true;

            return false;
        }
    }
}
