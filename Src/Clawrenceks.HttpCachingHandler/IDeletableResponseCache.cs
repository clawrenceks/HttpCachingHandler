using Clawrenceks.HttpCachingHandler.Abstractions;

namespace Clawrenceks.HttpCachingHandler
{
    public interface IDeletableResponseCache : IResponseCache
    {
        void DeleteCache();
    }
}
