using System;

namespace Clawrenceks.HttpCachingHandler.Abstractions
{
    public interface IResponseCache
    {
        void Add(string key, string data, TimeSpan expireIn, string eTag = null);
        void EmptyAll();
        void EmptyExpired();
        bool Exists(string key);
        string Get(string key);
        string GetETag(string key);
        bool IsExpired(string key);
        DateTime? GetExpiration(string key);
    }
}
