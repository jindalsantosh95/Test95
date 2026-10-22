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
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class UpdateMemberAddressCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockmediator;
        private readonly Mock<ILogger<UpdateMemberAddressCommand>> _mockLogger;
        private readonly Mock<ICommonRefDBRepository> _mockcommonRefDBRepository;
        private readonly UpdateMemberAddressCommandHandler _mockHandler;

        public UpdateMemberAddressCommandHandlerTests()
        {
            _mockmediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<UpdateMemberAddressCommand>>();
            _mockcommonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _mockHandler = new UpdateMemberAddressCommandHandler(_mockmediator.Object, _mockLogger.Object, _mockcommonRefDBRepository.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateMemberAddressCommandHandler(null, _mockLogger.Object, _mockcommonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdateMemberAddressCommandHandler(_mockmediator.Object, null, _mockcommonRefDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenRefDBRepositorObjectIsNull()
        {
            Assert.Throws<ArgumentException>(() =>
            new UpdateMemberAddressCommandHandler(_mockmediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnSucceess_ForGoodRequest()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressCommand>("CustomerMS\\Account\\UpdateMemberAddressCommandRequest");

            var customerSearchDocResponse = new Response<CustomerSearch>
            {
                Value = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument")
            };

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            var countryCodeResponse = TestDataProvider.GetTestData<List<CountryDetails>>("CustomerMS\\Account\\CountryCodeDetailsResponse");

            _mockmediator.Setup(s => s.Send(It.IsAny<CustomerSearchQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerSearchDocResponse);
            _mockmediator.Setup(s => s.Send(It.IsAny<FetchCustomerDocumentQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerDocResponse);

            _mockcommonRefDBRepository.Setup(s => s.GetCountryCodeDetails()).ReturnsAsync(countryCodeResponse);

            // Act
            var result = await _mockHandler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result.Errors);
            Assert.Equal(Status.OK, result.Status);
            Assert.Contains("Updated Member Address Successfully", result.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForEmptyCustomerId()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<UpdateMemberAddressCommand>("CustomerMS\\Account\\UpdateMemberAddressCommandRequest");

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
