using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class UpdatePaymentReqeustCommandHandlerV2Tests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdatePaymentReqeustCommandHandlerV2>> _logger;
        private readonly UpdatePaymentReqeustCommandHandlerV2 _handler;
        public UpdatePaymentReqeustCommandHandlerV2Tests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<UpdatePaymentReqeustCommandHandlerV2>>();
            _handler = new UpdatePaymentReqeustCommandHandlerV2(_mediator.Object, _logger.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CRMTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new UpdatePaymentReqeustCommandHandlerV2(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentReqeustCommandHandlerV2(_mediator.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorList_AfterValidation()
        {
            var request = TestDataProvider.GetTestData<UpdatePaymentReqeustCommandV2>("PaymentMS\\PaymentRequestDataWithError");

            var result = await _handler.Handle(request, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Status == Status.BadRequest);
        }
        [Fact]
        public async Task Handler_ShouldReturnAccepted_ForGoodRequest()
        {
            var request = TestDataProvider.GetTestData<UpdatePaymentReqeustCommandV2>("PaymentMS\\UpdatePaymentRequestCommandV2Data");
            var paymentDetailsRequest = TestDataProvider.GetTestData<PaymentDetails>("PaymentMS\\PaymentDetailsData");
            var countryDetails = new CountryDetails()
            {
                Value =
                [
                    new()
                    {
                        VaaCountryid  = "test",
                        VaaName = "Test Country"
                    }
                ]
            };

            _mediator.Setup(m => m.Send(It.IsAny<RetrievePaymentDetailsRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentDetailsRequest);
            _mediator.Setup(m => m.Send(It.IsAny<RetrieveCountryDetailsRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(countryDetails);
            _mediator.Setup(m => m.Send(It.IsAny<EventRequestInsertCommand>(), It.IsAny<CancellationToken>())).Equals(true);
            _mediator.Setup(m => m.Send(It.IsAny<EventStoreRequestDeleteCommand>(), It.IsAny<CancellationToken>())).Equals(true);

            var result = await _handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Status == Status.Accepted);
        }
    }
}

