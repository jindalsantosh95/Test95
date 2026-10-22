using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class AuthorizePaymentCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<Application.Command.CPD.AuthorizePaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Application.Command.CPD.AuthorizePaymentCommandHandler _handler;
        private readonly Mock<IAzureBlobService> _azureBlobService;

        public AuthorizePaymentCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<Application.Command.CPD.AuthorizePaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _azureBlobService = new Mock<IAzureBlobService>();
            _handler = new Application.Command.CPD.AuthorizePaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object, _azureBlobService.Object);
            ConfigReader.Set("OrderMgmtServiceBusConnectionString", "dummyConnString");

        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.AuthorizePaymentCommandHandler(null, _logger.Object, _httpConnector.Object, _azureBlobService.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.AuthorizePaymentCommandHandler(_mediator.Object, null, _httpConnector.Object, _azureBlobService.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.AuthorizePaymentCommandHandler(_mediator.Object, _logger.Object, null, _azureBlobService.Object));
        }

        [Fact]
        public async Task Handler_Exception()
        {
            //Arrange

            var request = TestDataProvider.GetTestData<Domain.CPD.AuthorizePaymentRequest>("PaymentMS\\PaymentRequest");
            var response = new Domain.CPD.PaymentResponse { Errors = new List<ErrorDetails>() { new() { Code = "911" } } };

            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("CPDSaltkey", "az1sx2dc3fv");
            ConfigReader.Set("CPDAccount", "123000");
            ConfigReader.Set("CPDMotoAccount", "123001");


            //Act
            var result = await _handler.Handle(new Application.Command.CPD.AuthorizePaymentCommand() { AuthorizePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
            Assert.Equal("911", response.Errors.First().Code);
        }

        [Fact]
        public async Task When_Handler_ShouldNotThrowError()
        {
            //Arrange

            var request = TestDataProvider.GetTestData<Domain.CPD.AuthorizePaymentRequest>("PaymentMS\\PaymentRequest");
            var initializeRS = TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse");
            var authorizeRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\AuthorisationResponse");
            var initializePaymentResponse = new Response<InitializePaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = initializeRS,
                IgnoreNullValues = false,
            };
            var authorizePaymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = authorizeRS,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<InitializePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(initializePaymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.AuthorizePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(authorizePaymentResponse);
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([new CountryId { CountryCode = "IN", DialingCode = "91", Id = "356" }]);

            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("CPDSaltkey", "az1sx2dc3fv");
            ConfigReader.Set("CPDAccount", "123000");
            ConfigReader.Set("CPDMotoAccount", "123001");
            ConfigReader.Set("CpdCurrencyBlob", "abc.txt");
            ConfigReader.Set("CpdCountryBlob", "xyz.txt");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.AuthorizePaymentCommand() { AuthorizePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("VS932GWYG7P24", result.Value.Value.OrderID);
            Assert.Equal("Payment authorized", result.Value.Value.TransactionStatus);
            Assert.Equal(47.54, result.Value.Value.Amount);
        }

        [Fact]
        public async Task When_InitializeHandler_ThrowError()
        {
            //Arrange

            var request = TestDataProvider.GetTestData<Domain.CPD.AuthorizePaymentRequest>("PaymentMS\\PaymentRequest");
            var initializeRS = TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse");
            initializeRS.Transaction = null;
            initializeRS.Status = [new() { Code = "210", Text = "Invalid HMAC: 4c914de1805d11dc6ba1195a9e2154d9c025a66965278eb44f779261a7ae2965eb8d4e0ddaaa6fb45cc6f46544c2a1cc2ae3604d25545c02adb472c0866a4ef8" }];

            var initializePaymentResponse = new Response<InitializePaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                Value = initializeRS,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<InitializePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(initializePaymentResponse);
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([new CountryId { CountryCode = "IN", DialingCode = "91", Id = "356" }]);

            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("CPDSaltkey", "az1sx2dc3fv");
            ConfigReader.Set("CPDAccount", "123000");
            ConfigReader.Set("CPDMotoAccount", "123001");
            ConfigReader.Set("CpdCurrencyBlob", "abc.txt");
            ConfigReader.Set("CpdCountryBlob", "xyz.txt");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.AuthorizePaymentCommand() { AuthorizePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.Value.Errors.Count > 0);
            Assert.Equal("G066", result.Value.Errors.First().Code);
        }

        [Fact]
        public async Task When_AuthorizeHandler_ThrowError()
        {
            //Arrange

            var request = TestDataProvider.GetTestData<Domain.CPD.AuthorizePaymentRequest>("PaymentMS\\PaymentRequest");
            var initializeRS = TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse");
            var authorizeRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\AuthorisationResponse");

            authorizeRS.Status = [new() { Code = "52", Text = "Invalid amount:50000" }];

            var initializePaymentResponse = new Response<InitializePaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = initializeRS,
                IgnoreNullValues = false,
            };
            var authorizePaymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.BadRequest,
                Value = authorizeRS,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<InitializePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(initializePaymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.AuthorizePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(authorizePaymentResponse);
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([new CountryId { CountryCode = "IN", DialingCode = "91", Id = "356" }]);

            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("CPDSaltkey", "az1sx2dc3fv");
            ConfigReader.Set("CPDAccount", "123000");
            ConfigReader.Set("CPDMotoAccount", "123001");
            ConfigReader.Set("CpdCurrencyBlob", "abc.txt");
            ConfigReader.Set("CpdCountryBlob", "xyz.txt");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.AuthorizePaymentCommand() { AuthorizePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.Value.Errors.Count > 0);
            Assert.Equal("G066", result.Value.Errors.First().Code);
        }


        [Fact]
        public async Task When_handler_Should_Throw_CosmosDbException()
        {
            var request = TestDataProvider.GetTestData<Domain.CPD.AuthorizePaymentRequest>("PaymentMS\\PaymentRequest");
            var initializeRS = TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse");
            var authorizeRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\AuthorisationResponse");
            var document = TestDataProvider.GetTestData<Domain.CPD.CreatePaymentDocumentRequest>("PaymentMS\\CreatePaymentDocumentRequest");
            var initializePaymentResponse = new Response<InitializePaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = initializeRS,
                IgnoreNullValues = false,
            };
            var authorizePaymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = authorizeRS,
                IgnoreNullValues = false,
            };

            _mediator.Setup(x => x.Send(It.IsAny<InitializePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(initializePaymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.AuthorizePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(authorizePaymentResponse);
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync([new CountryId { CountryCode = "IN", DialingCode = "91", Id = "356" }]);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.CreatePaymentDocRepoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CosmosDBException("Error Occured"));

            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("CPDSaltkey", "az1sx2dc3fv");
            ConfigReader.Set("CPDAccount", "123000");
            ConfigReader.Set("CPDMotoAccount", "123001");
            ConfigReader.Set("CpdCurrencyBlob", "abc.txt");
            ConfigReader.Set("CpdCountryBlob", "xyz.txt");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.AuthorizePaymentCommand() { AuthorizePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error Occured", result.Error);

        }

    }
}
