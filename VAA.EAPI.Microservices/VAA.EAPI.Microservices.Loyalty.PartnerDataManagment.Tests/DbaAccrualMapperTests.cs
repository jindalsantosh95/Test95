using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class DbaAccrualMapperTests
    {
        [Fact]
        public void Map_UsesOperatingAirlineForUniqueIdentification()
        {
            var request = new AutoAccrualFlownDetails
            {
                OperatingAirlineCode = "VS",
                OperatingAirAuthNumber = "000106337233331"
            };

            var accrualRequest = DbaAccrualMapper.Map(request);

            Assert.NotNull(accrualRequest.UniqueIdentification);
            Assert.Equal("VS000106337233331", accrualRequest.UniqueIdentification!.UniqueIdentificationNumber);
            Assert.Equal("VS", accrualRequest.UniqueIdentification.AirlineCode);
        }
    }
}
