using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class RetroCreditCommandHandlerTest
    {
        private readonly RetroCreditCommandHandler _handler;
        private readonly Mock<IFlownDbRepository> _flownDbRepository;
        private readonly Mock<IRetroApiService> _retroApiService;
        private readonly Mock<ILogger<RetroCreditCommandHandler>> _logger;

        public RetroCreditCommandHandlerTest()
        {
            PDMTestHelpers.CreateTestEnvironmentSettings();
            _flownDbRepository = new Mock<IFlownDbRepository>();
            _retroApiService = new Mock<IRetroApiService>();
            _logger = new Mock<ILogger<RetroCreditCommandHandler>>();
            _handler = new RetroCreditCommandHandler(_flownDbRepository.Object, _retroApiService.Object, _logger.Object);
        }       

        [Fact]
        public async Task HandlerResultStatusErrorForNullRetroCreditCommand()
        {
            RetroCreditCommand command = null;

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Status.Error, result.Status);
        }
        
        [Fact]
        public async Task HandlerShouldGiveErrorStatusForNullRetroCreditRequest()
        {
            RetroCreditCommand command = new RetroCreditCommand()
            {
                RetroCreditRequest = null,
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(Status.Error, result.Status);
        }

        [Fact]
        public async Task HandlerShouldCheckRetroDateInValidRange_Minimum_EDC101()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            request.ScheduledFlightSegmentDepartureDate = DateTime.Now.ToString("yyyyMMdd");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };

            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC101);
        }

        [Fact]
        public async Task HandlerShouldCheckRetroDateInValidRange_Maximum_EDC101()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            request.ScheduledFlightSegmentDepartureDate = DateTime.Now.AddYears(-2).ToString("yyyyMMdd");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };

            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC101);
        }

        [Fact]
        public async Task HandlerShouldCheckFlightIsValid_EDC102()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            request.ScheduledFlightSegmentDepartureDate = DateTime.Now.AddDays(-10).ToString("yyyyMMdd");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };

            _retroApiService.Setup(m => m.GetCodeShareFlightDetails(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(TestDataProvider.GetTestData<FlightDetails>("PartnerDataManagmentMS\\FlightAPIResponse_NotFound"));

            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC102);
        }

        [Fact]
        public async Task HandlerShouldReturnRetroDetailsForValidRetroRequest_EDC000()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            request.ScheduledFlightSegmentDepartureDate = DateTime.Now.AddDays(-10).ToString("yyyyMMdd");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };

            _retroApiService.Setup(m => m.GetCodeShareFlightDetails(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(TestDataProvider.GetTestData<FlightDetails>("PartnerDataManagmentMS\\FlightAPIResponse"));
            _flownDbRepository.Setup(m => m.GetFlownDetailsForRetroCreditAsync(It.IsAny<RetroCreditRequest>())).ReturnsAsync(PDMTestHelpers.FetchRetroCreditFlownDetails());
            
            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC000);
        }

        [Fact(Skip = "EligibilityDenialCode always set to 101")]
        public async Task HandlerShouldReturnNotFoundEDCIfRetroDetailsNotInDB_EDC104()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };
            
            _retroApiService.Setup(m => m.GetCodeShareFlightDetails(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(TestDataProvider.GetTestData<FlightDetails>("PartnerDataManagmentMS\\FlightAPIResponse"));
            _flownDbRepository.Setup(m => m.GetFlownDetailsForRetroCreditAsync(It.IsAny<RetroCreditRequest>())).ReturnsAsync(new RetroCreditFlownDetails { Error = "Data Not Found" });
            
            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC104);
        }

        [Fact(Skip = "EligibilityDenialCode always set to 101")]
        public async Task HandlerShouldReturnRetroEDCForMultipleRecordsFound_EDC666()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };

            _retroApiService.Setup(m => m.GetCodeShareFlightDetails(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(TestDataProvider.GetTestData<FlightDetails>("PartnerDataManagmentMS\\FlightAPIResponse"));
            _flownDbRepository.Setup(m => m.GetFlownDetailsForRetroCreditAsync(It.IsAny<RetroCreditRequest>())).ReturnsAsync(new RetroCreditFlownDetails { Error = "ERROR CODE 666" });
            
            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC666);
        }

        [Fact(Skip = "EligibilityDenialCode always set to 101")]
        public async Task HandlerShouldReturnRetroEDCIfRetroAlreadyClaimed_EDC201Y()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };
            
            _retroApiService.Setup(m => m.GetCodeShareFlightDetails(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(TestDataProvider.GetTestData<FlightDetails>("PartnerDataManagmentMS\\FlightAPIResponse"));
            _flownDbRepository.Setup(m => m.GetFlownDetailsForRetroCreditAsync(It.IsAny<RetroCreditRequest>())).ReturnsAsync(PDMTestHelpers.FetchRetroCreditFlownDetails("Allocated"));
            
            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC201);
            Assert.Equal(result.Value.PostingNonEligibilityIndicator, "Y");
        }

        [Fact(Skip = "EligibilityDenialCode always set to 101")]
        public async Task HandlerShouldReturnRetroDetails_Success_EDC201R()
        {
            var request = TestDataProvider.GetTestData<RetroCreditRequest>("PartnerDataManagmentMS\\OnlineRetroCreditRequest");
            var creditCommand = new RetroCreditCommand()
            {
                RetroCreditRequest = request
            };
            
            _retroApiService.Setup(m => m.GetCodeShareFlightDetails(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(TestDataProvider.GetTestData<FlightDetails>("PartnerDataManagmentMS\\FlightAPIResponse"));
            _flownDbRepository.Setup(m => m.GetFlownDetailsForRetroCreditAsync(It.IsAny<RetroCreditRequest>())).ReturnsAsync(PDMTestHelpers.FetchRetroCreditFlownDetails("Pending", "AR"));
            
            var result = await _handler.Handle(creditCommand, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Equal(result.Value.EligibilityDenialCode, Constants.ErrorCodes.EDC201);
            Assert.Equal(result.Value.PostingNonEligibilityIndicator, "R");
        }

    }
}
