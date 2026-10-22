using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class UpdateEmailCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockmediator;
        private readonly Mock<ILogger<UpdateEmailCommandHandler>> _mockLogger;
        private readonly Mock<IDeDupRepositoryEnrolment> _mockdeduprepository;
        private readonly Mock<ICommonRefDBRepository> _mockcommonRefDBRepository;
        private readonly UpdateEmailCommandHandler _mockHandler;

        public UpdateEmailCommandHandlerTests()
        {
            _mockmediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<UpdateEmailCommandHandler>>();
            _mockdeduprepository = new Mock<IDeDupRepositoryEnrolment>();
            _mockcommonRefDBRepository = new Mock<ICommonRefDBRepository>();    
            _mockHandler = new UpdateEmailCommandHandler(_mockmediator.Object, _mockLogger.Object, _mockdeduprepository.Object, _mockcommonRefDBRepository.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateEmailCommandHandler(null, _mockLogger.Object, _mockdeduprepository.Object, _mockcommonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateEmailCommandHandler(_mockmediator.Object, null, _mockdeduprepository.Object, _mockcommonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenDeduprepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateEmailCommandHandler(_mockmediator.Object, _mockLogger.Object, null, _mockcommonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositorObjectIsNull()
        {
            Assert.Throws<ArgumentException>(() =>
            new UpdateEmailCommandHandler(_mockmediator.Object, _mockLogger.Object, _mockdeduprepository.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowNullException_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _mockHandler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldReturnError_IfNewEmailExistsinDB()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailCommand>("CustomerMS\\Account\\UpdateEmailCommandRequest");

            _mockdeduprepository.Setup(s => s.GetCustomersWithEmail(It.IsAny<string>())).ReturnsAsync(request.MemberNumber);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("10161a", result.Errors.First().Code);
            Assert.Contains("Provided email already exists in Database", result.Errors.First().Message);
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailCommand>("CustomerMS\\Account\\UpdateEmailCommandRequest");

            var customerSearchDocResponse = new Response<CustomerSearch>
            {
                Value = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument")
            };

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            var countryCodeResponse = TestDataProvider.GetTestData<List<CountryDetails>>("CustomerMS\\Account\\CountryCodeDetailsResponse");

            _mockdeduprepository.Setup(s => s.GetCustomersWithEmail(It.IsAny<string>())).ReturnsAsync(string.Empty);
            _mockmediator.Setup(s => s.Send(It.IsAny<CustomerSearchQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerSearchDocResponse);
            _mockmediator.Setup(s => s.Send(It.IsAny<FetchCustomerDocumentQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerDocResponse);

            _mockcommonRefDBRepository.Setup(s => s.GetCountryCodeDetails()).ReturnsAsync(countryCodeResponse);    

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Errors);
            Assert.Equal(Status.OK, result.Status);
            Assert.Contains("Email Update Successful", result.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_IfNoCountryCodeDetailsPresent()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailCommand>("CustomerMS\\Account\\UpdateEmailCommandRequest");

            var customerSearchDocResponse = new Response<CustomerSearch>
            {
                Value = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument")
            };

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            _mockdeduprepository.Setup(s => s.GetCustomersWithEmail(It.IsAny<string>())).ReturnsAsync(string.Empty);
            _mockmediator.Setup(s => s.Send(It.IsAny<CustomerSearchQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerSearchDocResponse);
            _mockmediator.Setup(s => s.Send(It.IsAny<FetchCustomerDocumentQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerDocResponse);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("G009", result.Errors.First().Code);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullCustomerId()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateEmailCommand>("CustomerMS\\Account\\UpdateEmailCommandRequest");

            _mockdeduprepository.Setup(s => s.GetCustomersWithEmail(It.IsAny<string>())).ReturnsAsync(string.Empty);
            _mockmediator.Setup(s => s.Send(It.IsAny<FetchCustomerDocumentQuery>(), It.IsAny<CancellationToken>())).Throws(new ArgumentNullException());

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("G009", result.Errors.First().Code);
        }
    } 
}
