using Moq;
using System;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class CachedPriceClassData
    {
        private readonly Mock<IAzureBlobService> _blobService;

        public CachedPriceClassData()
        {
            _blobService = new Mock<IAzureBlobService>();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenBlobServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CachedPriceClass(null));
        }

    }
}
