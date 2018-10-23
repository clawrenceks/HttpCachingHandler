using Clawrenceks.HttpCachingHandler.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class IntegrationTestResponseCache : IResponseCache
    {
        public IntegrationTestResponseCache()
        {
            var cacheLocationBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cacheId = Guid.NewGuid();
            var cacheLocation = Path.Combine(cacheLocationBase, cacheId.ToString());

            while (Directory.Exists(cacheLocation))
            {
                cacheId = Guid.NewGuid();
                cacheLocation = Path.Combine(cacheLocationBase, cacheId.ToString());
            }

            CacheLocation = Directory.CreateDirectory(cacheLocation).FullName;
        }

        public string CacheLocation { get; private set; }
        private bool CacheLocationExists => Directory.Exists(CacheLocation);

        public void Add(string key, string data, TimeSpan expireIn, string eTag = null)
        {
            key = UrlEncodeString(key);

            if (string.IsNullOrWhiteSpace(eTag))
            {
                eTag = null;
            }

            var expiryDate = DateTime.Now.Add(expireIn);

            var cachedResponse = new CachedResponse(data, expiryDate, eTag);
            var serizlizedReponse = JsonConvert.SerializeObject(cachedResponse);
            string formattedResponse = JToken.Parse(serizlizedReponse).ToString(Formatting.Indented);

            var cachedItemPath = Path.Combine(CacheLocation, key);

            try
            {
                File.WriteAllBytes(cachedItemPath, Encoding.ASCII.GetBytes(formattedResponse));
            }
            catch (Exception ex)
            {
                throw new CacheAccessException("An error occured when accessing the cache. More detail can be found in the inner exception.", ex);
            }
        }

        public void DeleteCache()
        {
            if (Directory.Exists(CacheLocation))
            {
                Directory.Delete(CacheLocation, true);
            }
        }

        public void EmptyAll()
        {
            if (Directory.Exists(CacheLocation))
            {
                Directory.Delete(CacheLocation, true);
                Directory.CreateDirectory(CacheLocation);
            }
        }

        public void EmptyExpired()
        {
            if (CacheLocationExists)
            {
                try
                {
                    var cachedItems = Directory.EnumerateFiles(CacheLocation, "*", SearchOption.AllDirectories);

                    foreach (var item in cachedItems)
                    {
                        var cachedItemContent = File.ReadAllText(item);
                        var cachedReponse = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);

                        if (cachedReponse.IsExpired)
                        {
                            File.Delete(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new CacheAccessException("An error occured when accessing the cache. More detail can be found in the inner exception.", ex);
                }
            }         
        }

        public bool Exists(string key)
        {
            key = UrlEncodeString(key);
            var cachedItemPath = Path.Combine(CacheLocation, key);
            return File.Exists(cachedItemPath);
        }

        public string Get(string key)
        {
            key = UrlEncodeString(key);
            if (!Exists(key))
            {
                throw new InvalidOperationException($"No item with {nameof(key)} {key} exists in the cache.");
            }

            var cachedResponse = LoadCachedResponse(key);
            return cachedResponse.Content;
        }

        public string GetETag(string key)
        {
            key = UrlEncodeString(key);
            if (!Exists(key))
            {
                throw new InvalidOperationException($"No item with {nameof(key)} {key} exists in the cache.");
            }

            var cachedReponse = LoadCachedResponse(key);
            return cachedReponse.Etag;
        }

        public DateTime? GetExpiration(string key)
        {
            key = UrlEncodeString(key);
            if (!Exists(key))
            {
                throw new InvalidOperationException($"No item with {nameof(key)} {key} exists in the cache.");
            }

            var cachedReponse = LoadCachedResponse(key);
            return cachedReponse.ExpiryDate;
        }

        public bool IsExpired(string key)
        {
            key = UrlEncodeString(key);
            if (!Exists(key))
            {
                throw new InvalidOperationException($"No item with {nameof(key)} {key} exists in the cache.");
            }

            var cachedReponse = LoadCachedResponse(key);
            return cachedReponse.IsExpired;
        }

        private CachedResponse LoadCachedResponse(string key)
        {
            var cachedItemContent = File.ReadAllText(Path.Combine(CacheLocation, key));
            var cachedReponse = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            return cachedReponse;
        }

        private string UrlEncodeString(string value)
        {
            //The value is first decoded here, to handle the scenario where 
            //a fully or partially encoded Url might have already been passed in as the key.
            var decodedUrl = WebUtility.UrlDecode(value);
            return WebUtility.UrlEncode(decodedUrl);
        }
    }
}
