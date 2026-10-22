using Grpc.Net.Client.Balancer;
using MediatR;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.ManageAccount
{
    public class UpdatePhoneNumberEventCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<UpdatePhoneNumberEventCommandHandler>> _logger;
       
        public UpdatePhoneNumberEventCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<UpdatePhoneNumberEventCommandHandler>>();
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatePhoneNumberEventCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdatePhoneNumberEventCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task UpdatePhoneNumberEventCommandHandler_ValidScenario()
        {
            var updatePhoneNumberRequest = TestDataProvider.GetTestData<UpdatePhoneNumberEventCommand>("CustomerMS\\ManageAccount\\UpdatePhoneNumberRequest");
            var updatePhoneNumberresponse = TestDataProvider.GetTestData<ManageAccountUpdateResponse>("CustomerMS\\ManageAccount\\UpdatePhoneNumberResponse");

            _mediator.Setup(m => m.Send(It.IsAny<UpdatePhoneNumberEventCommandHandler>(), CancellationToken.None)).ReturnsAsync(updatePhoneNumberresponse);
            UpdatePhoneNumberEventCommandHandler handler = new UpdatePhoneNumberEventCommandHandler(_mediator.Object,_logger.Object);
           
            var result = await handler.Handle(updatePhoneNumberRequest, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Accepted",result.Status.ToString());
        }
       
    }
}
