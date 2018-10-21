using System;
using Xunit;

namespace Clawrenceks.HttpCachingHandler.IntegrationTests
{
    public class CachedResponseTests
    {
        [Fact]
        public void Content_ReturnsCorrectValue_FollowingObjectConstruction()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now);

            //Assert
            Assert.Equal("Test Content", sut.Content);
        }

        [Fact]
        public void ExpiryDate_ReturnsCorrectValue_FollowingObjectConstruction()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", new DateTime(2018, 1, 10, 16, 15, 32));

            //Assert
            Assert.Equal(new DateTime(2018, 1, 10, 16, 15, 32), sut.ExpiryDate);
        }

        [Fact]
        public void Etag_ReturnsNull_GivenNoValue_At_Construction()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now);

            //Assert
            Assert.Null(sut.Etag);
        }

        [Fact]
        public void Etag_ReturnsCorrectValue_GivenValue_At_Construction()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now, "my-test-etag");

            //Assert
            Assert.Equal("my-test-etag", sut.Etag);
        }

        [Fact]
        public void HasEtag_ReturnsFalse_WhenNoEtag_IsSet_DuringConstruction()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now);

            //Assert
            Assert.False(sut.HasEtag);
        }

        [Fact]
        public void HasEtag_ReturnsTrue_WhenEtag_IsSet_DuringConstruction()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now, "my-test-etag");

            //Assert
            Assert.True(sut.HasEtag);
        }

        [Fact]
        public void IsExpired_ReturnsFalse_WhenObject_IsConstructed_WithExpiryDate_ThatIs_InTheFuture()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now.AddHours(1));

            //Assert
            Assert.False(sut.IsExpired);
        }

        [Fact]
        public void IsExpired_ReturnsTrue_WhenObject_IsConstructed_WithExpiryDate_ThatIs_InThePast()
        {
            //Arrange
            var sut = new CachedResponse("Test Content", DateTime.Now.AddHours(-1));

            //Assert
            Assert.True(sut.IsExpired);
        }
    }
}
