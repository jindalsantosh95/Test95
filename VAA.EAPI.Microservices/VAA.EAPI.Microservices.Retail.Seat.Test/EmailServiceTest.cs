using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Microservices.Retail.Seat.Application;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Seat.Tests
{
    public class EmailServiceTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailService> _loggerService;

        // Initialize _mediator in the constructor to fix CS8618
        public EmailServiceTest()
        {
            _mediator = new Mock<IMediator>();
            _loggerService = Microsoft.Extensions.Logging.Abstractions.NullLogger<EmailService>.Instance;
            _emailService = new EmailService(_mediator.Object, _loggerService);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            ConfigReader.Set("EmailServiceBusConnectionString", "dummyConnStringEmail");
            ConfigReader.Set("EmailMSServiceBusTopicName", "seatconfirmationemailmessagetopicname");
        }

        // Add tests for AddEventForEmailReceiptAsync method
        [Fact]
        public async Task AddEventForEmailReceiptAsync_ShouldReturnTrue_WhenEmailEventGenerated()
        {
            // Arrange

            var request = new Domain.OrderEmailRequest()
            {
                RecordLocator = "12345",
                Email = "abc@example.com"
            };
            // Act
            var result = await _emailService.AddEventForEmailReceiptAsync(request);

            // Assert
            Assert.True(result);
        }
    }
}
