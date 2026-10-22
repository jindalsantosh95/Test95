using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignUpdateEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CampaignUpdateEventProcessCommandHandler>> _mockLogger;
        private readonly Mock<ICommonRefDBRepository> _mockCommonRefDBRepository;
        private readonly CampaignUpdateEventProcessCommandHandler _handler;
        public CampaignUpdateEventProcessCommandHandlerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CampaignUpdateEventProcessCommandHandler>>();
            _mockCommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new CampaignUpdateEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCommonRefDBRepository.Object);
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUpdateEventProcessCommandHandler(_mockMediator.Object, null, _mockCommonRefDBRepository.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUpdateEventProcessCommandHandler(null, _mockLogger.Object, _mockCommonRefDBRepository.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CampaignUpdateEventProcessCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }
        [Fact]
        public void Handler_Success_With_No_Error()
        {
            var campaingnUpdareEventRequestCommand = TestDataProvider.GetTestData<CampaignUpdateEventProcessCommand>("CampaignManagementMS\\CampaignUpdateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockCommonRefDBRepository.Setup(x => x.UpdateCampaign("1-19GLQ3S", "WELS44171C",
                                                                   "07/03/2022",
                                                                   "07/03/2023",
                                                                   "Test",
                                                                   "Test_Desc_one",
                                                                   "Link",
                                                                   true,
                                                                   "xcvxc",
                                                                   "Test_type", "testuser")).ReturnsAsync("Success");
            var response = _handler.Handle(campaingnUpdareEventRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsSuccess);
            Assert.Null(response.Result.Value.Errors);

        }
        [Fact]
        public void Handler_Success_With_Error()
        {
            var campaingnUpdareEventRequestCommand = TestDataProvider.GetTestData<CampaignUpdateEventProcessCommand>("CampaignManagementMS\\CampaignUpdateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).ReturnsAsync(true);
            _mockCommonRefDBRepository.Setup(x => x.UpdateCampaign("1-19GLQ3S", "WELS44171C",
                                                                   "07/03/2022",
                                                                   "07/03/2023",
                                                                   "Test",
                                                                   "Test_Desc_one",
                                                                   "Link",
                                                                   true,
                                                                   "xcvxc",
                                                                   "Test_type", "testuser")).ReturnsAsync("Faliure");
            var response = _handler.Handle(campaingnUpdareEventRequestCommand, System.Threading.CancellationToken.None);
            Assert.False(response.Result.IsSuccess);
        }

        [Fact]
        public void HandlerFailureWithError()
        {
            var campaingnUpdareEventRequestCommand = TestDataProvider.GetTestData<CampaignUpdateEventProcessCommand>("CampaignManagementMS\\CampaignUpdateEventProcessCommand");
            _mockMediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), System.Threading.CancellationToken.None)).Throws(new Exception());
            _mockCommonRefDBRepository.Setup(x => x.UpdateCampaign("1-19GLQ3S", "WELS44171C",
                                                                   "07/03/2022",
                                                                   "07/03/2023",
                                                                   "Test",
                                                                   "Test_Desc_one",
                                                                   "Link",
                                                                   true,
                                                                   "xcvxc",
                                                                   "Test_type", "testuser")).ReturnsAsync("Success");
            var response = _handler.Handle(campaingnUpdareEventRequestCommand, System.Threading.CancellationToken.None);
            Assert.True(response.Result.IsFailure);
        }
    }
}
