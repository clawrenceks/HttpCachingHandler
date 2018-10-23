using Newtonsoft.Json;
using System;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class CachedResponse
    {
        public CachedResponse(string content, DateTime expiryDate, string eTag = null)
        {
            Content = content;
            ExpiryDate = expiryDate;
            Etag = eTag;
        }

        public string Content { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public string Etag { get; private set; }

        [JsonIgnore]
        public bool HasEtag => !string.IsNullOrWhiteSpace(Etag);

        [JsonIgnore]
        public bool IsExpired => ExpiryDate < DateTime.Now;

    }
}
