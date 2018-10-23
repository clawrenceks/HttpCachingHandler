using System;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class CacheAccessException : Exception
    {
        public CacheAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
