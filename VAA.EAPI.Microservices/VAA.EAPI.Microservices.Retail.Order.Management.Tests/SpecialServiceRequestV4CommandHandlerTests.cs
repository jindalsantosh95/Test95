using Moq;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class SpecialServiceRequestV4CommandHandlerTests
    {
        private readonly Mock<ISpecialServiceRequestV4> _specialServiceRequestV4;
        private readonly SpecialServiceRequestV4CommandHandler _handler;
        public SpecialServiceRequestV4CommandHandlerTests()
        {
            _specialServiceRequestV4 = new Mock<ISpecialServiceRequestV4>();
            _handler = new SpecialServiceRequestV4CommandHandler(_specialServiceRequestV4.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }
        [Fact]
        public async Task Handler_ShouldReturnSuccessResponseWhenNoExceptionOccurs()
        {
            //Arrange
            var channelEntityData = TestDataProvider.GetTestData<Channel>("ChannelInfo");
            var request = new SpecialServiceRequestV4Command()
            {
                specialServiceRequestInfo = new SpecialServiceRequestInfo(),
                ChannelInfo = channelEntityData
            };
            var response = new AddResponseType() { Pnr = new PnrType() { SpecialServiceRequest = new SpecialServiceRequest1[1] } };
            _specialServiceRequestV4.Setup(r => r.AddSSRsRequest(It.IsAny<AddRequestType>())).Returns(response);
            //Act
            var result = await _handler.Handle(request, new System.Threading.CancellationToken());
            //Assert
            Assert.Single(result.Pnr.SpecialServiceRequest);
        }
    }
}
