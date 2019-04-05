namespace Clawrenceks.HttpCachingHandler.Abstractions
{
    public interface IDeletableResponseCache : IResponseCache
    {
        void DeleteCache();
    }
}
