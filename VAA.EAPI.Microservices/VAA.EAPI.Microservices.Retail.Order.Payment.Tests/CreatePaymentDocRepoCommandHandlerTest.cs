using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Exceptions;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class CreatePaymentDocRepoCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<Infrastructure.Repositories.CreatePaymentDocRepoCommandHandler>> _logger;
        private readonly Infrastructure.Repositories.CreatePaymentDocRepoCommandHandler _handler;
        public CreatePaymentDocRepoCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<Infrastructure.Repositories.CreatePaymentDocRepoCommandHandler>>();
            _handler = new Infrastructure.Repositories.CreatePaymentDocRepoCommandHandler(_mediator.Object, _logger.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Repositories.CreatePaymentDocRepoCommandHandler(null, _logger.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Repositories.CreatePaymentDocRepoCommandHandler(_mediator.Object, null));
        }

        [Fact]
        public async Task When_Handler_Should_Fail()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<Domain.CPD.CreatePaymentDocumentRequest>("PaymentMS\\CreatePaymentDocumentRequest");
            ConfigReader.Set("PaymentDBEndpoint", "Endpoint_Url");
            ConfigReader.Set("PaymentDatabaseName", "Storage");
            ConfigReader.Set("PaymentCollection", "Collection");
            _mediator.Setup(x => x.Send(It.IsAny<VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosModifyRepoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Error Occured"));

            //Act
            //Assert
            await Assert.ThrowsAsync<CosmosDBException>(async () => await _handler.Handle(new Infrastructure.Repositories.CreatePaymentDocRepoCommand(request), new CancellationToken()));

        }
    }
}
