
using Moq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class UpdateVredCustomerDetailsControllerTest
    {
        private readonly Mock<IServiceResponseQuery> _serviceResponseQueryMock;

        public UpdateVredCustomerDetailsControllerTest()
        {
            VRedCommon.CreateTestEnvironmentSettings();
            _serviceResponseQueryMock = new Mock<IServiceResponseQuery>(MockBehavior.Strict);

        }

        private UpdateVredCustomerDetailsResponseQueryHandler CreateHandler(
            IServiceResponseQuery? service = null)
        {
            return new UpdateVredCustomerDetailsResponseQueryHandler(
                service ?? _serviceResponseQueryMock.Object);
        }


        [Fact]
        public async Task Handle_Success_ReturnsExpectedResponse()
        {

            var request = new UpdateVredCustomerDetailsResponseQuery
            {
                CorrelationId = "xxxx",
                Type = "VRED_UpdateCustomerDetails",
                
            };
            var payload= Json.Serialize(request);
            var serviceResponse = new ServiceResponse
            {
                Status = ServiceResponseProcessStatus.Completed,
                JsonResponseData = payload
            };
            _serviceResponseQueryMock
                .Setup(s => s.Handle(
                    It.IsAny<ServiceResponseQueryRequest>(),
                    It.IsAny<CancellationToken>()))
                .Callback((ServiceResponseQueryRequest actualReq, CancellationToken _) =>
                {

                    Assert.Equal(request.Id, actualReq.Id);
                    Assert.Equal(request.Type, actualReq.EventType);

                })
                .ReturnsAsync(serviceResponse);

            var handler = CreateHandler();
            Response<UpdateVredCustomerDetailsQueryResponse> result = await handler.Handle(request, CancellationToken.None);
            Assert.NotNull(result);
            Assert.Equal(Status.OK, result.Status);


        }

        [Fact]
        public async Task Handle_ServiceThrows_ReturnsFailureResponse()
        {

            var request = new UpdateVredCustomerDetailsResponseQuery
            {
                CorrelationId = "xxxx",              
                Type = "VRED_UpdateCustomerDetails",
              
            };

            var handler = CreateHandler();
            await Assert.ThrowsAsync<Moq.MockException>(() =>
                handler.Handle(request, CancellationToken.None));
        }
    }
}