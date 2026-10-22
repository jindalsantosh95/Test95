using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.VacEnrolment
{
    public class VacEventEnrolmentCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<VacEventEnrolmentCommandHandler>> _mockLogger;
        private readonly Mock<IDeDupRepositoryEnrolment> _mockDeDupRepository;
        private readonly Mock<IFCNumberGenerationRepository> _mockFCNumberGenerationRepository;
        private readonly VacEventEnrolmentCommandHandler _handler;

        public VacEventEnrolmentCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<VacEventEnrolmentCommandHandler>>();
            _mockFCNumberGenerationRepository = new Mock<IFCNumberGenerationRepository>();
            _mockDeDupRepository = new Mock<IDeDupRepositoryEnrolment>();
            _handler = new VacEventEnrolmentCommandHandler(_mockMediator.Object,_mockLogger.Object, _mockFCNumberGenerationRepository.Object, _mockDeDupRepository.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VacEventEnrolmentCommandHandler(null, _mockLogger.Object, _mockFCNumberGenerationRepository.Object, _mockDeDupRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VacEventEnrolmentCommandHandler(_mockMediator.Object, null, _mockFCNumberGenerationRepository.Object, _mockDeDupRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenFCNumberGenerationRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VacEventEnrolmentCommandHandler(_mockMediator.Object, _mockLogger.Object, null, _mockDeDupRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenDeDupRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new VacEventEnrolmentCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockFCNumberGenerationRepository.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(new VacEventEnrolmentCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldEnrolVacMember_ForGoodRequest()
        {
            //Arrage
            var vacEnrolmentRequest = TestDataProvider.GetTestData<VacEventEnrolmentCommand>("CustomerMS\\VACEnrolment\\VacEnrolmenetRequest");
            var fcNumberGenerationResponse = TestDataProvider.GetTestData<Response<EnrolmentResponse>>("CustomerMS\\VACEnrolment\\FcGenerationRepoResponse");

            //Act
            _mockFCNumberGenerationRepository.Setup(s => s.GenerateFlyingClubNumber()).ReturnsAsync(fcNumberGenerationResponse);

            var result = await _handler.Handle(vacEnrolmentRequest, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Unit>(result.Value);
        }

        [Fact]
        public async Task Handler_ShouldUpgradeBasicToVacMember_ForGoodRequest()
        {
            //Arrage
            var vacEnrolmentRequest = TestDataProvider.GetTestData<VacEventEnrolmentCommand>("CustomerMS\\VACEnrolment\\VacUpgradeRequest");

            var customerDocument = TestDataProvider.GetTestData<Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities.Customer>("CustomerMS\\CustomerDocument\\CustomerDoc");

            var custDocResponse = new List<Infrastructure.Repositories.Enrolment.DuplicateCheck.Entities.Customer>
            {
                customerDocument
            };
            
            //Act
            _mockDeDupRepository.Setup(s => s.GetCustomersDetailsWithEmail(It.IsAny<string>())).ReturnsAsync(custDocResponse);

            var result = await _handler.Handle(vacEnrolmentRequest, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Unit>(result.Value);
        }
    }
}
