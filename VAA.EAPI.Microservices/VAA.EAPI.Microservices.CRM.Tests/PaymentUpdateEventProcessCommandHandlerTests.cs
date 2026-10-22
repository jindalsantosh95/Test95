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
    public class PaymentUpdateEventProcessCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<PaymentUpdateEventProcessCommandHandler>> _logger;
        private readonly PaymentUpdateEventProcessCommandHandler _handler;
        public PaymentUpdateEventProcessCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<PaymentUpdateEventProcessCommandHandler>>();
            _handler = new PaymentUpdateEventProcessCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new PaymentUpdateEventProcessCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PaymentUpdateEventProcessCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task PaymentUpdateEventProcessCommandHandler_ShouldReturnSucceed_WithGoodResponse()
        {
            var request = TestDataProvider.GetTestData<PaymentUpdateEventProcessCommand>("PaymentMS\\PaymentUpdateEventProcessCommandData");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            var countryDetailsRequest = TestDataProvider.GetTestData<CountryDetails>("PaymentMS\\CountryDetailsData");

            _mediator.Setup(m => m.Send(It.IsAny<ServiceResponseInsertCommand>(), It.IsAny<CancellationToken>()))
                         .Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(paymentDetailsRequest);
            _mediator.Setup(m => m.Send(It.IsAny<RetrieveCountryDetailsRepoQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(countryDetailsRequest);
            HttpResponseMessage msg = new HttpResponseMessage();
            msg.Headers.Location = new Uri("https://vaauat.crm.com");
            _mediator.Setup(m => m.Send(It.IsAny<UpdatePaymentDetailsRepoCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(msg.Headers);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(Unit.Value == result.Value);
        }
    }
}
