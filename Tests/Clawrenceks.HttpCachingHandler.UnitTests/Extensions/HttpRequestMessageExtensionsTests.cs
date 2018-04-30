using System.Net.Http;
using System.Net.Http.Headers;
using Clawrenceks.HttpCachingHandler.Extensions;
using Xunit;

namespace Clawrenceks.HttpCachingHandler.UnitTests.Extensions
{
    public class HttpRequestMessageExtensionsTests
    {
        [Fact]
        public void ShouldBypassPrivateCache_ReturnsTrue_WhenRequestMessage_ContainsNoCacheControlHeaders()
        {
            //Arrange
            var request = new HttpRequestMessage();

            //Assert
            Assert.True(request.ShouldBypassPrivateCache());
        }

        [Fact]
        public void ShouldBypassPrivateCache_ReturnsTrue_WhenNoCacheHeader_IsPresent()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

            //Assert
            Assert.True(request.ShouldBypassPrivateCache());            
        }

        [Fact]
        public void ShouldBypassPrivateCache_ReturnsTrue_WhenHttpMethod_IsNotGet()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.CacheControl = new CacheControlHeaderValue();
            request.Method = HttpMethod.Delete;

            //Assert
            Assert.True(request.ShouldBypassPrivateCache());
        }

        [Fact]
        public void ShouldBypassPrivateCache_ReturnsFalse_WhenHttpMethod_IsGet()
        {
            //Arrange
            var request = new HttpRequestMessage();
            request.Headers.CacheControl = new CacheControlHeaderValue();
            request.Method = HttpMethod.Get;

            //Assert
            Assert.False(request.ShouldBypassPrivateCache());
        }

    }
}
