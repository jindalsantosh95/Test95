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
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class CapturePaymentCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<Application.Command.CPD.CapturePaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Mock<IAzureBlobService> _azureBlobService;
        private readonly Application.Command.CPD.CapturePaymentCommandHandler _handler;

        public CapturePaymentCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<Application.Command.CPD.CapturePaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _azureBlobService = new Mock<IAzureBlobService>();
            _handler = new Application.Command.CPD.CapturePaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object, _azureBlobService.Object);
            ConfigReader.Set("OrderMgmtServiceBusConnectionString", "dummyConnString");

        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.CapturePaymentCommandHandler(null, _logger.Object, _httpConnector.Object, _azureBlobService.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.CapturePaymentCommandHandler(_mediator.Object, null, _httpConnector.Object, _azureBlobService.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.CapturePaymentCommandHandler(_mediator.Object, _logger.Object, null, _azureBlobService.Object));
        }
        
        [Fact]
        public void When_azureBlobServiceObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Application.Command.CPD.CapturePaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object, null));
        }

        [Fact]
        public async Task Handler_Exception()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CapturePaymentRequest>("PaymentMS\\CaptureRequest");
            var response = new Domain.CPD.PaymentResponse { Errors = new List<ErrorDetails>() { new() { Code = "911" } } };

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.CapturePaymentCommand() { CapturePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
            Assert.Equal("911", response.Errors.First().Code);
        }

        [Fact]
        public async Task When_Handler_ShouldNotThrowError()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CapturePaymentRequest>("PaymentMS\\CaptureRequest");
            var captureRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            var GetTransactionAPIResponse = TestDataProvider.GetTestData<GetTransactionStatusResponse>("PaymentMS\\GetTransactionStatusResponse");
            var document = TestDataProvider.GetTestData<Domain.CPD.PaymentTransactionDocument>("PaymentMS\\PaymentTransactionDocument");

            var paymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = captureRS,
                IgnoreNullValues = false,
            };
            var TransactionStatusResponse = new Response<GetTransactionStatusResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = GetTransactionAPIResponse,
                IgnoreNullValues = false,
            };
            
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.CapturePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.GetTransactionStatusQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(TransactionStatusResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.PaymentDocumentReaderQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);
            _mediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()));
            _mediator.Setup(x => x.Send(It.IsAny<ServiceBusTopicMessageEventCommand>(), It.IsAny<CancellationToken>()));
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()));
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CountryId>());

            ConfigReader.Set("cpdcountryblob", "xyz.txt");
            ConfigReader.Set("cpdclientid", "123");
            ConfigReader.Set("servicebusconnectionstring", "abc.com");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.CapturePaymentCommand() { CapturePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("VS932GWYG7P24", result.Value.Value.OrderID);
            Assert.Equal("Operation Success", result.Value.Value.TransactionStatus);
            Assert.Equal(25.67, result.Value.Value.Amount);
        }

        [Fact]
        public async Task When_Transaction_Document_is_valid()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CapturePaymentRequest>("PaymentMS\\CaptureRequest");
            var GetTransactionAPIResponse = TestDataProvider.GetTestData<GetTransactionStatusResponse>("PaymentMS\\GetTransactionStatusResponse");
            var captureRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            var document = TestDataProvider.GetTestData<Domain.CPD.PaymentTransactionDocument>("PaymentMS\\PaymentTransactionDocument");

            var paymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = captureRS,
                IgnoreNullValues = false,
            };
            var TransactionStatusResponse = new Response<GetTransactionStatusResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = GetTransactionAPIResponse,
                IgnoreNullValues = false,
            };
            
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.CapturePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.GetTransactionStatusQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(TransactionStatusResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.PaymentDocumentReaderQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);
            _mediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()));
            _mediator.Setup(x => x.Send(It.IsAny<ServiceBusTopicMessageEventCommand>(), It.IsAny<CancellationToken>()));
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()));
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CountryId>());

            ConfigReader.Set("cpdcountryblob", "xyz.txt");
            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("servicebusconnectionstring", "abc.com");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.CapturePaymentCommand() { CapturePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("80262675", result.Value.Value.TransactionId);
            Assert.Equal("VS932GWYG7P24", result.Value.Value.OrderID);
            Assert.Equal("Operation Success", result.Value.Value.TransactionStatus);

        }

        [Fact]
        public async Task When_RLOC_Is_Null_Or_Empty()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CapturePaymentRequest>("PaymentMS\\CaptureRequest");
            request.BookingReferences.RLOC = null;
            var captureRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            captureRS.Status.ForEach(err =>
            {
                err.Code = "400";
                err.Text = "Element 'product': The attribute 'sku' is required but missing";
            });
            var document = TestDataProvider.GetTestData<Domain.CPD.PaymentTransactionDocument>("PaymentMS\\PaymentTransactionDocument");

            var paymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.Error,
                Value = captureRS,
                IgnoreNullValues = false,
            };
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.CapturePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CountryId>());

            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("cpdcountryblob", "xyz.txt");
            ConfigReader.Set("RetryCount", "0");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.CapturePaymentCommand() { CapturePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("G009", result.Error);

        }

        [Fact]
        public async Task When_Handler_Should_Throw_CosmosDBException()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CapturePaymentRequest>("PaymentMS\\CaptureRequest");
            var captureRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            var GetTransactionAPIResponse = TestDataProvider.GetTestData<GetTransactionStatusResponse>("PaymentMS\\GetTransactionStatusResponse");
            var document = TestDataProvider.GetTestData<Domain.CPD.PaymentTransactionDocument>("PaymentMS\\PaymentTransactionDocument");
            var doc2 = TestDataProvider.GetTestData<VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosDocumentEvent>("PaymentMS\\CosmosDocumentEvent");
            var paymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = captureRS,
                IgnoreNullValues = false,
            };
            var TransactionStatusResponse = new Response<GetTransactionStatusResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = GetTransactionAPIResponse,
                IgnoreNullValues = false,
            };
            
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.CapturePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.GetTransactionStatusQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(TransactionStatusResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.PaymentDocumentReaderQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.CosmosRepoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CosmosDBException("Error Occured"));
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CountryId>());

            ConfigReader.Set("cpdcountryblob", "xyz.txt");

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.CapturePaymentCommand() { CapturePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Error Occured", result.Error);

        }

        [Fact]
        public async Task When_GetTransactionStatusAPI_Success()
        {
            // Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CapturePaymentRequest>("PaymentMS\\CaptureRequest_BatchWiseCardType");
            var captureRS = TestDataProvider.GetXmlTestData<Infrastructure.Models.CPD.PaymentResponse>("PaymentMS\\CaptureResponse");
            var GetTransactionAPIResponse = TestDataProvider.GetTestData<GetTransactionStatusResponse>("PaymentMS\\GetTransactionStatusResponse");
            var document = TestDataProvider.GetTestData<Domain.CPD.PaymentTransactionDocument>("PaymentMS\\PaymentTransactionDocument");
            var doc2 = TestDataProvider.GetTestData<VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosDocumentEvent>("PaymentMS\\CosmosDocumentEvent");
            
            var paymentResponse = new Response<Infrastructure.Models.CPD.PaymentResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = captureRS,
                IgnoreNullValues = false,
            };

            var TransactionStatusResponse = new Response<GetTransactionStatusResponse>
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = GetTransactionAPIResponse,
                IgnoreNullValues = false,
            };
            
            ConfigReader.Set("CPDClientId", "12300");
            ConfigReader.Set("cpdcountryblob", "xyz.txt");
            ConfigReader.Set("RetryCount", "1");
            ConfigReader.Set("servicebusconnectionstring", "abc.com");

            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.CapturePaymentCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Services.CPD.GetTransactionStatusQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(TransactionStatusResponse);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.PaymentDocumentReaderQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);
            _mediator.Setup(x => x.Send(It.IsAny<Infrastructure.Repositories.CosmosRepoCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Unit.Value);
            _mediator.Setup(x => x.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>()));
            _mediator.Setup(x => x.Send(It.IsAny<ServiceBusTopicMessageEventCommand>(), It.IsAny<CancellationToken>()));
            _mediator.Setup(x => x.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()));
            _azureBlobService.Setup(x => x.BlobJsonAsAsync<IEnumerable<CountryId>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<CountryId>());

            //Act
            var result = await _handler.Handle(new Application.Command.CPD.CapturePaymentCommand() { CapturePaymentRequest = request }, new CancellationToken());

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("80262675", result.Value.Value.TransactionId);
            Assert.Equal("VS932GWYG7P24", result.Value.Value.OrderID);
            Assert.Equal("Payment Captured", result.Value.Value.TransactionStatus);

        }
    }
}
