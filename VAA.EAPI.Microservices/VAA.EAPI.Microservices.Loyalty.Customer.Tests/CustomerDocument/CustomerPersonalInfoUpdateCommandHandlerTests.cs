using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using CSharpFunctionalExtensions;
using System.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.CustomerDocument
{
    public class CustomerPersonalInfoUpdateCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<CustomerPersonalInfoUpdateCommandHandler>> _mockLogger;
        private readonly Mock<IGetCustTransDocumentService<FlyingClubMember>> _mockCustTransFcmService;
        private readonly CustomerPersonalInfoUpdateCommandHandler _handler;

        public CustomerPersonalInfoUpdateCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<CustomerPersonalInfoUpdateCommandHandler>>();
            _mockCustTransFcmService = new Mock<IGetCustTransDocumentService<FlyingClubMember>>();
            _handler = new CustomerPersonalInfoUpdateCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustTransFcmService.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerPersonalInfoUpdateCommandHandler(null, _mockLogger.Object, _mockCustTransFcmService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerPersonalInfoUpdateCommandHandler(_mockMediator.Object, null, _mockCustTransFcmService.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustTransDocumentServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CustomerPersonalInfoUpdateCommandHandler(_mockMediator.Object, _mockLogger.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullRequest()
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForExistingMatchingEmailinDB()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = "VAC" }}));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10161a", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Provided email already exists in Database", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullNameWhileUpdate()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.FirstName = null;

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("G012", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("One of the mandatory field is missing in the request.", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullContactInfoWhileUpdate()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.PhoneNumber = null;

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("G012", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("One of the mandatory field is missing in the request.", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullDOBWhileUpdate()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.DateOfBirth = null;

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("G012", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("One of the mandatory field is missing in the request.", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }


        [Fact]
        public async Task Handler_ShouldThrowError_ForMinorsWhileUpdate()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.DateOfBirth = DateTime.UtcNow.AddYears(-17).ToString();

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10195", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Account can't be updated due to age restrictions. User age should be more than 18", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForBrithYearLessThan1900()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.DateOfBirth = "1899-02-15";

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10195", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Account can't be updated due to age restrictions.", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForInvalidCharacterLengthInName()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.FirstName = "TestNameTestNameTestNameTestName";

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10194", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Invalid Character Lenghth in the Input Fields", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForInvalidCharacterLengthInPhoneNumber()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.PhoneNumber = "071";

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10194", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Invalid Character Lenghth in the Input Fields", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForInvalidCharacterInName()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.LastName = "Test$$LastName";

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10193", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Invalid Character in the Input Fields", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForInvalidCharacterInPhoneNumber()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");
            vacUpdateRequest.LastName = "07123 45678";

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10193", result.Value.Errors.FirstOrDefault().Code);
            Assert.Contains("Invalid Character in the Input Fields", result.Value.Errors.FirstOrDefault().Message);
            Assert.Null(result.Value.Value);
        }

        [Fact]
        public async Task Handler_ShouldUpdateDB_ForGoodRequest()
        {
            //Arrage
            var vacUpdateRequest = TestDataProvider.GetTestData<CustomerPersonalInfoUpdateCommand>("CustomerMS\\ManageAccount\\VacUpdateRequest");

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            var customerSearchDocResponse = new Response<CustomerSearch>
            {
                Value = TestDataProvider.GetTestData<CustomerSearch>("CustomerSearch\\CustomerSearchDocument")
            };

            var flyingClubMemberDocResponse = TestDataProvider.GetTestData<FlyingClubMember>("FlyingClubMember\\FcmDoc");
            flyingClubMemberDocResponse.TierCode = "VAC";

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<CustomerSearchByEmailQuery>(), CancellationToken.None)).ReturnsAsync(Result.Success(new Response<CustomerTierInfo> { Value = new CustomerTierInfo() { Tier = string.Empty } }));

            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            _mockMediator.Setup(s => s.Send(It.IsAny<CustomerSearchQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerSearchDocResponse);

            _mockCustTransFcmService.Setup(x => x.GetCosmosDoc(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flyingClubMemberDocResponse);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            var result = await _handler.Handle(vacUpdateRequest, CancellationToken.None);

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.Null(result.Value.Errors);
            Assert.NotNull(result.Value.Value);
            Assert.Equal("Successfully updated", result.Value.Value);            
        }
    }
}
