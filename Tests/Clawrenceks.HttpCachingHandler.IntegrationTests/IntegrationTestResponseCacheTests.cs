using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class IntegrationTestResponseCacheTests : IDisposable
    {
        private readonly IntegrationTestResponseCache _sut;

        public IntegrationTestResponseCacheTests()
        {
            _sut = new IntegrationTestResponseCache();
        }

        public void Dispose()
        {
            _sut.DeleteCache();
        }

        [Fact]
        public void DeleteCache_Removes_CacheDirectory()
        {
            //Arrange
            var sut = new IntegrationTestResponseCache();

            //Act
            sut.DeleteCache();

            //Assert
            Assert.False(Directory.Exists(sut.CacheLocation));
        }

        [Fact]
        public void Exists_ReturnsFalse_WhenNoItem_WithSpecifiedKey_ExistsIn_CacheLocation()
        {
            //Act
            var result = _sut.Exists("test-file-name");

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void Exists_ReturnsTrue_WhenItem_WithSpecifiedKey_ExistsIn_CacheLocation()
        {
            //Arrange
            var cacheItemLocation = Path.Combine(_sut.CacheLocation, "my-test-cache-item");
            var cachedItem = File.Create(cacheItemLocation);
            cachedItem.Close();

            //Act
            var result = _sut.Exists("my-test-cache-item");

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void Exists_ReturnsTrue_GivenUrlAsKey_WhenCacheContainsItem_WithEncodedUrl_AsFilename()
        {
            //Arrange
            var cacheItemLocation = Path.Combine(_sut.CacheLocation, WebUtility.UrlEncode("http://my-test-cache-item"));
            var cachedItem = File.Create(cacheItemLocation);
            cachedItem.Close();

            //Act
            var result = _sut.Exists("http://my-test-cache-item");

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void EmptyAll_ClearsCacheDirectory()
        {
            //Arrange
            for (int i = 0; i < 3; i++)
            {
                var file = File.Create(Path.Combine(_sut.CacheLocation, Guid.NewGuid().ToString()));
                file.Close();
            }

            //Act
            _sut.EmptyAll();

            //Assert
            Assert.Empty(Directory.EnumerateFileSystemEntries(_sut.CacheLocation));
        }

        [Fact]
        public void EmptyAll_DoesNotDelete_Cache()
        {
            //Act
            _sut.EmptyAll();

            //Assert
            Assert.True(Directory.Exists(_sut.CacheLocation));
        }

        [Fact]
        public void Add_CreateNewItem_WithCorrectFileName_InCacheDirectory()
        {
            //Act
            _sut.Add("my-new-cache-item", "my-cached-item-content", DateTime.Now.AddDays(10).TimeOfDay);

            //Assert
            Assert.True(File.Exists(Path.Combine(_sut.CacheLocation, "my-new-cache-item")));
        }

        [Fact]
        public async Task Add_AddsCorrectCacheContent_ToCachedItem()
        {
            //Act
            _sut.Add("my-cache-item", "my-cache-item-content", DateTime.Now.AddDays(10).TimeOfDay);

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Equal("my-cache-item-content", cachedItem.Content);
        }

        [Fact]
        public async Task Add_AddsCorrect_ExpiryDate_ToCachedItem()
        {
            //Act
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0));

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Equal(DateTime.Now.AddDays(10).Date, cachedItem.ExpiryDate.Date);
        }

        [Fact]
        public async Task Add_AddsCachedItem_WithCorrectEtag()
        {
            //Act
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0), "my-test-etag-value");

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Equal("my-test-etag-value", cachedItem.Etag);
        }

        [Fact]
        public async Task Add_AddsCachedItem_WithNullEtag_WhenGiven_NoEtag()
        {
            //Act
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0));

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Null(cachedItem.Etag);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Add_AddsCachedItem_WithNullEtag_GivenEtag_ThatIs_NullEmptyOrWhitespace(string etag)
        {
            //Act
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0), etag);

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Null(cachedItem.Etag);
        }

        [Fact]
        public void Add_ThrowsCacheAccessException_WhenAnExceptionOccurs_AddingItemToCache()
        {
            //Arrange
            Directory.Delete(_sut.CacheLocation, true);

            //Act & Assert
            var exception = Assert.Throws<CacheAccessException>(() => _sut.Add("my-cache-item-key", "my cache item content", DateTime.Now.AddHours(1).TimeOfDay));
            Assert.Contains("error occured when accessing the cache", exception.Message);
        }

        [Fact]
        public void Add_AddsInnerException_ToCacheAccessException_WhenExceptionIsThrown()
        {
            //Arrange
            Directory.Delete(_sut.CacheLocation, true);

            //Act & Assert
            var exception = Assert.Throws<CacheAccessException>(() => _sut.Add("my-cache-item-key", "my cache item content", DateTime.Now.AddHours(1).TimeOfDay));
            Assert.NotNull(exception.InnerException);
        }

        [Fact]
        public void Add_UrlEncodesKey_WhenAddingItem_ToCache()
        {
            //Act
            _sut.Add("http://my-url/my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0));

            //Assert
            Assert.True(File.Exists(Path.Combine(_sut.CacheLocation, WebUtility.UrlEncode("http://my-url/my-cache-item"))));
        }

        [Fact]
        public void Add_Overwrites_CacheContent_WhenAnItem_WithTheSameKey_AlreadyExists_InCache()
        {
            //Arrange
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0), "my-test-etag-value");

            //Act
            _sut.Add("my-cache-item", "my-updated-cache-content", new TimeSpan(20, 0, 0, 0), "my-new-etag-value");

            //Assert
            Assert.Equal("my-updated-cache-content", _sut.Get("my-cache-item"));
        }

        [Fact]
        public async Task Add_Overwrites_ExpiryDate_WhenAnItem_WithTheSameKey_AlreadyExists_InCache()
        {
            //Arrange
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0), "my-test-etag-value");

            //Act
            _sut.Add("my-cache-item", "my-updated-cache-content", new TimeSpan(20, 0, 0, 0), "my-new-etag-value");

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Equal(DateTime.Now.AddDays(20).Date, cachedItem.ExpiryDate.Date);
        }

        [Fact]
        public async Task Add_Overwrites_Etag_WhenAnItem_WithTheSameKey_AlreadyExists_InCache_AndNewEtag_IsNull()
        {
            //Arrange
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0), "my-test-etag-value");

            //Act
            _sut.Add("my-cache-item", "my-updated-cache-content", new TimeSpan(20, 0, 0, 0));

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Null(cachedItem.Etag);
        }

        [Fact]
        public async Task Add_Overwrites_Etag_WhenAnItem_WithTheSameKey_AlreadyExists_InCache_AndNewEtag_IsNotNull()
        {
            //Arrange
            _sut.Add("my-cache-item", "my-cache-item-content", new TimeSpan(10, 0, 0, 0), "my-test-etag-value");

            //Act
            _sut.Add("my-cache-item", "my-updated-cache-content", new TimeSpan(20, 0, 0, 0),"my-new-etag-value");

            //Assert
            var cachedItemContent = await File.ReadAllTextAsync(Path.Combine(_sut.CacheLocation, "my-cache-item"));
            var cachedItem = JsonConvert.DeserializeObject<CachedResponse>(cachedItemContent);
            Assert.Equal("my-new-etag-value", cachedItem.Etag);
        }

        [Fact]
        public void Get_ThrowsInvalidOperationException_GivenKey_WhichDoesNotExist_InCache()
        {
            //Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _sut.Get("non-existent-item"));
            Assert.Contains("No item with", exception.Message);
        }

        [Fact]
        public void Get_ReturnsCorrectResult_WhenItem_ExistsInCache()
        {
            //Arrange
            _sut.Add("my-test-cache-item", "my-test-cache-content", DateTime.Now.AddHours(2).TimeOfDay);

            //Act
            var result = _sut.Get("my-test-cache-item");

            //Assert
            Assert.Equal("my-test-cache-content", result);
        }

        [Fact]
        public void Get_ReturnsCorrectResult_GivenUrl_AsKey()
        {
            //Arrange
            _sut.Add("http://my-test-cache-item/my-item?query=test", "my-test-cache-content", DateTime.Now.AddHours(2).TimeOfDay);

            //Act
            var result = _sut.Get("http://my-test-cache-item/my-item?query=test");

            //Assert
            Assert.Equal("my-test-cache-content", result);
        }

        [Fact]
        private void GetEtag_Throws_InvalidOperationException_GivenKey_WhichDoesNot_ExistInCache()
        {
            //Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _sut.GetETag("non-existent-item"));
            Assert.Contains("No item with", exception.Message);
        }

        [Fact]
        private void GetEtag_ReturnsNull_GivenKey_OfItem_ThatHas_NoEtag()
        {
            //Arrange
            _sut.Add("my-new-cache-item", "my-cache-date", DateTime.Now.AddHours(1).TimeOfDay);

            //Act
            var result = _sut.GetETag("my-new-cache-item");

            //Assert
            Assert.Null(result);
        }

        [Fact]
        private void GetEtag_ReturnsCorrectEtag_GivenKey_OfItem_ThatHasAnEtag()
        {
            //Arrange
            _sut.Add("my-new-cache-item", "my-cache-date", DateTime.Now.AddHours(1).TimeOfDay, "my-test-etag");

            //Act
            var result = _sut.GetETag("my-new-cache-item");

            //Assert
            Assert.Equal("my-test-etag", result);
        }

        [Fact]
        private void GetEtag_ReturnsCorrectEtag_GivenUrl_AsKey()
        {
            //Arrange
            _sut.Add("http://www.my-item-to-cache", "my-cache-date", DateTime.Now.AddHours(1).TimeOfDay, "my-test-etag");

            //Act
            var result = _sut.GetETag("http://www.my-item-to-cache");

            //Assert
            Assert.Equal("my-test-etag", result);
        }

        [Fact]
        public void GetExpiration_Throws_InvalidOperationException_GivenKey_WhichDoesNot_ExistInCache()
        {
            //Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _sut.GetExpiration("non-existent-item"));
            Assert.Contains("No item with", exception.Message);
        }

        [Fact]
        public void GetExpiration_ReturnsCorrectDate_GivenKey_OfItem_WhichExists_InCache()
        {
            //Arrange
            var expiryDate = DateTime.Now.AddDays(15);

            var cachedResponse = new CachedResponse("my-test-cached-response", expiryDate);
            var serializedResponse = JsonConvert.SerializeObject(cachedResponse);
            var cachedItem = File.Create(Path.Combine(_sut.CacheLocation, "my-test-cached-item"));
            cachedItem.Write(Encoding.ASCII.GetBytes(serializedResponse));
            cachedItem.Close();

            //Act
            var result = _sut.GetExpiration("my-test-cached-item");

            //Assert
            Assert.Equal(result, expiryDate);
        }

        [Fact]
        public void GetExpiration_ReturnsCorrectDate_GivenUrl_AsKey()
        {
            //Arrange
            var expiryDate = DateTime.Now.AddDays(15);

            var cachedResponse = new CachedResponse("my-test-cached-response", expiryDate);
            var serializedResponse = JsonConvert.SerializeObject(cachedResponse);
            var cachedItem = File.Create(Path.Combine(_sut.CacheLocation, WebUtility.UrlEncode("my-test-cached-item")));
            cachedItem.Write(Encoding.ASCII.GetBytes(serializedResponse));
            cachedItem.Close();

            //Act
            var result = _sut.GetExpiration("my-test-cached-item");

            //Assert
            Assert.Equal(result, expiryDate);
        }

        [Fact]
        public void IsExpired_Throws_InvalidOperationException_GivenKey_WhichDoesNot_ExistInCache()
        {
            //Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _sut.IsExpired("non-existent-item"));
            Assert.Contains("No item with", exception.Message);
        }

        [Fact]
        public void IsExpired_ReturnsTrue_GivenKey_OfItemInCache_WhichHas_Expired()
        {
            //Arrange
            var expiry = DateTime.Now.AddDays(-2) - DateTime.Now;
            _sut.Add("my-new-cache-item", "my-cache-date", expiry);

            //Act
            var result = _sut.IsExpired("my-new-cache-item");

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void IsExpired_ReturnsTrue_GivenUrl_AsKey()
        {
            //Arrange
            var expiry = DateTime.Now.AddDays(-2) - DateTime.Now;
            _sut.Add("https://my-new-cache-item", "my-cache-date", expiry);

            //Act
            var result = _sut.IsExpired("https://my-new-cache-item");

            //Assert
            Assert.True(result);
        }

        [Fact]
        public void IsExpired_ReturnsFalse_GivenKey_OfItemInCache_WhichHasNot_Expired()
        {
            //Arrange
            _sut.Add("my-new-cache-item", "my-cache-date", DateTime.Now.AddHours(1).TimeOfDay);

            //Act
            var result = _sut.IsExpired("my-new-cache-item");

            //Assert
            Assert.False(result);
        }

        [Fact]
        public void EmptyExpired_RemovesExpiredEntries_FromCache()
        {
            //Arrange
            var expiredItemExpiry = DateTime.Now.AddDays(-2) - DateTime.Now;
            var nonExpiredItemExpiry = DateTime.Now.AddHours(2).TimeOfDay;

            _sut.Add("expired-item-one", "expired-item-content", expiredItemExpiry);
            _sut.Add("expired-item-two", "expired-item-content", expiredItemExpiry);
            _sut.Add("none-expired-item-one", "none-expired-item-content", nonExpiredItemExpiry);
            _sut.Add("none-expired-item-two", "none-expired-item-content", nonExpiredItemExpiry);

            //Act
            _sut.EmptyExpired();

            //Assert
            var filesInCache = Directory.EnumerateFiles(_sut.CacheLocation, "*", SearchOption.AllDirectories);

            Assert.Equal(2, filesInCache.Count());
        }

        [Fact]
        public void EmptyExpired_ThrowsCacheAccessException_WhenCacheItem_CannotBeDeleted()
        {
            //Arrange
            var expiryTime = DateTime.Now - DateTime.Now.AddHours(-5);
            _sut.Add("my-test-cache-item", "my-test=cache-data", expiryTime);
            var cacheItem = File.Open(Path.Combine(_sut.CacheLocation, "my-test-cache-item"), FileMode.Open);

            //Act & Assert
            var exception = Assert.Throws<CacheAccessException>(() => _sut.EmptyExpired());
            Assert.Contains("error occured when accessing the cache", exception.Message);
            cacheItem.Close();
        }

        [Fact]
        public void EmptyExpired_AddsInnerException_ToCacheAccessException_WhenException_IsThrown()
        {
            //Arrange
            var expiryTime = DateTime.Now - DateTime.Now.AddHours(-5);
            _sut.Add("my-test-cache-item", "my-test=cache-data", expiryTime);
            var cacheItem = File.Open(Path.Combine(_sut.CacheLocation, "my-test-cache-item"), FileMode.Open);

            //Act & Assert
            var exception = Assert.Throws<CacheAccessException>(() => _sut.EmptyExpired());
            Assert.NotNull(exception.InnerException);
            cacheItem.Close();
        }
    }
}
