using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Services.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public class CommonRefDBRepositoryTest
    {
        private readonly Mock<ICommonRefDBServices> _commonRefDBServices;
        private readonly Mock<ILogger<CommonRefDBRepository>> _logger;
        private readonly CommonRefDBRepository _repo;
        public CommonRefDBRepositoryTest()
        {
            _logger = new Mock<ILogger<CommonRefDBRepository>>();
            _commonRefDBServices = new Mock<ICommonRefDBServices>();
            _repo = new CommonRefDBRepository(_commonRefDBServices.Object);

        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CommonRefDBRepository(null));
        }

        [Theory]
        [InlineData("XX")]
        public async Task GetPartnerIdSuccess(string partnerCode)
        {
            var partnerId = "0000";
            _commonRefDBServices.Setup(x => x.GetPartnerDetailsByPartnerCode("XX")).ReturnsAsync(partnerId);
            var result = await _repo.GetPartnerId(partnerCode);
            Assert.Equal("0000", result);
        }
    }
}
