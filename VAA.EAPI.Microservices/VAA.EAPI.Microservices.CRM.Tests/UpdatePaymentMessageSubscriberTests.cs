using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Application.Commands;
using VAA.EAPI.Microservices.CRM.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class UpdatePaymentMessageSubscriberTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdatePaymentMessageSubscriber>> _logger;

        public UpdatePaymentMessageSubscriberTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<UpdatePaymentMessageSubscriber>>();

        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentMessageSubscriber(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new UpdatePaymentMessageSubscriber(_mediator.Object, null));
        }

        [Fact(Skip = "We cant run this as we are setting this environment variable in other tests outside this collection, this is causing a race condition and making the test fail.")]
        public async Task UpdatePaymentMessageSubscriber_ShouldSucceed_WithGoodRequest()
        {
            // Arrange
            UpdatePaymentMessageSubscriber handler = new UpdatePaymentMessageSubscriber(_mediator.Object, _logger.Object);
            FunctionContext functionContext = new TestFunctionContext();
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Order", Data = new PaymentUpdateEventProcessCommand { } };
            System.Environment.SetEnvironmentVariable(CRM.Constants.Environment.CRMWebApiResponsiveFeature, "false");
            //CRM.Constants.Environment.CRMWebApiResponsiveFeature = false;
            // Act
            await handler.Run(Json.Serialize(cloudEvent), functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }
        [Fact(Skip = "We cant run this as we are setting this environment variable in other tests outside this collection, this is causing a race condition and making the test fail.")]
        public async Task UpdatePaymentMessageSubscriber_ShouldSucceed_WithCRMWebApiResponsiveFeature()
        {
            // Arrange
            UpdatePaymentMessageSubscriber handler = new UpdatePaymentMessageSubscriber(_mediator.Object, _logger.Object);
            FunctionContext functionContext = new TestFunctionContext();
            CloudEvent cloudEvent = new() { Id = "abcde-ffhfh-46575859-4657588t7", Source = "Order", Data = new PaymentUpdateEventProcessCommand { } };
            System.Environment.SetEnvironmentVariable(CRM.Constants.Environment.CRMWebApiResponsiveFeature, "true");

            // Act
            await handler.Run(Json.Serialize(cloudEvent), functionContext);

            // Assert
            // The service bus event Handler does not have any outcome. 
        }
    }
}
