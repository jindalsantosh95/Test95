using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class PaymentUpdateEventProcessCommandHandlerV2Tests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PaymentUpdateEventProcessCommandHandlerV2>> _logger;
        private readonly PaymentUpdateEventProcessCommandHandlerV2 _handler;
        public PaymentUpdateEventProcessCommandHandlerV2Tests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PaymentUpdateEventProcessCommandHandlerV2>>();
            _handler = new PaymentUpdateEventProcessCommandHandlerV2(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new PaymentUpdateEventProcessCommandHandlerV2(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PaymentUpdateEventProcessCommandHandlerV2(_mediator.Object, null));
        }
        [Fact]
        public async Task Handler_ReturnErrorList_WithNullPaymentID()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommandV2>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            paymentDetailsRequest.Value.ForEach(x => x.VaaPaymentid = null);

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(Unit.Value == result.Value);
        }
        [Fact]
        public async Task Handler_ReturnErrorList_WithEmptyPaymentID()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommandV2>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            paymentDetailsRequest.Value.ForEach(x => x.VaaPaymentid = "");

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(Unit.Value == result.Value);
        }
        [Fact]
        public async Task Handler_ShouldReturnErrorList_ForVaaBankaccountdetailsenteredandVaaPaymentStage()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommandV2>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(Unit.Value == result.Value);
        }
        [Fact]
        public async Task Handler_ShouldReturnErrorList_ForNullVaaCountryid()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommandV2>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var CountryDetailsRequest = TestDataProvider.GetTestData<CountryDetails>("PaymentMS\\CountryDetailsData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            CountryDetailsRequest.Value.ForEach(x => x.VaaCountryid = null);
            paymentDetailsRequest.Value.ForEach(x => x.VaaBankaccountdetailsentered = false);
            paymentDetailsRequest.Value.ForEach(x => x.VaaPaymentStage = 0);

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);

            _mediator.Setup(m => m.Send(It.IsAny<RetrieveCountryDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(CountryDetailsRequest);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(Unit.Value == result.Value);
        }
        [Fact]
        public async Task Handler_ShouldReturnErrorList_ForEmptyVaaCountryid()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommandV2>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var CountryDetailsRequest = TestDataProvider.GetTestData<CountryDetails>("PaymentMS\\CountryDetailsData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            CountryDetailsRequest.Value.ForEach(x => x.VaaCountryid = "");
            paymentDetailsRequest.Value.ForEach(x => x.VaaBankaccountdetailsentered = false);
            paymentDetailsRequest.Value.ForEach(x => x.VaaPaymentStage = 0);

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);

            _mediator.Setup(m => m.Send(It.IsAny<RetrieveCountryDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(CountryDetailsRequest);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(Unit.Value == result.Value);
        }
        [Fact]
        public async Task Handler_ShouldSucceed_ForGoodRequest()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommandV2>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var CountryDetailsRequest = TestDataProvider.GetTestData<CountryDetails>("PaymentMS\\CountryDetailsData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            paymentDetailsRequest.Value.ForEach(x => x.VaaBankaccountdetailsentered = false);
            paymentDetailsRequest.Value.ForEach(x => x.VaaPaymentStage = 0);

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);
            _mediator.Setup(m => m.Send(It.IsAny<RetrieveCountryDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(CountryDetailsRequest);
            HttpResponseMessage msg = new HttpResponseMessage();
            msg.Headers.Location = new Uri("https://vaauat.crm.com");
            _mediator.Setup(m => m.Send(It.IsAny<UpdatePaymentDetailsRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(msg.Headers);
            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.True(Unit.Value == result.Value);
        }
    }
}
