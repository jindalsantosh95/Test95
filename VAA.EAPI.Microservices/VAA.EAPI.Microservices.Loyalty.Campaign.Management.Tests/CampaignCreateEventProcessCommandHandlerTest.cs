using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignCreateEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CampaignCreateEventProcessCommandHandler>> _mockLogger;
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly CampaignCreateEventProcessCommandHandler _handler;
        public CampaignCreateEventProcessCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignCreateEventProcessCommandHandler>>();
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new CampaignCreateEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCommonRefDBRepository.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateEventProcessCommandHandler(_mockMediator.Object, null, _mockCommonRefDBRepository.Object));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateEventProcessCommandHandler(null, _mockLogger.Object, _mockCommonRefDBRepository.Object));
        }
        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignCreateEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public void HandlerSuccessWithNoError()
        {
            var campaignCreateData = TestDataProvider.GetTestData<CampaignCreateEventProcessCommand>("CampaignManagementMS\\CampaignCreateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockCommonRefDBRepository.Setup(x => x.CreateCampaign("WELS44Test", "07/03/2022",
                                                                   "07/03/2023",
                                                                   "Unit Test",
                                                                   "Test_Desc_one",
                                                                   "Link",
                                                                   true,
                                                                   "Active",
                                                                   "Straight to Gold",
                                                                   "testuser")).ReturnsAsync("Success");
            var response = _handler.Handle(campaignCreateData, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
            Assert.Null(response.Result.Value.Errors);
        }

        [Fact]
        public void HandlerSuccessWithError()
        {
            var campaignCreateData = TestDataProvider.GetTestData<CampaignCreateEventProcessCommand>("CampaignManagementMS\\CampaignCreateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockCommonRefDBRepository.Setup(x => x.CreateCampaign("WELS44Test", "07/03/2022",
                                                                   "07/03/2023",
                                                                   "Unit Test",
                                                                   "Test_Desc_one",
                                                                   "Link",
                                                                   true,
                                                                   "Active",
                                                                   "Straight to Gold", "testuser")).ReturnsAsync("Faliure");
            var response = _handler.Handle(campaignCreateData, System.Threading.CancellationToken.None);
            Assert.False(response.Result.IsSuccess);
            Assert.True(response.Result.IsFailure);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var campaignCreateData = TestDataProvider.GetTestData<CampaignCreateEventProcessCommand>("CampaignManagementMS\\CampaignCreateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            _mockCommonRefDBRepository.Setup(x => x.CreateCampaign("WELS44Test", "07/03/2022",
                                                                   "07/03/2023",
                                                                   "Unit Test",
                                                                   "Test_Desc_one",
                                                                   "Link",
                                                                   true,
                                                                   "Active",
                                                                   "Straight to Gold", "testuser")).ReturnsAsync("Faliure");
            var response = _handler.Handle(campaignCreateData, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsFailure);
        }
    }
}
