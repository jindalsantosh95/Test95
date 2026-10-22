using MediatR;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class PaymentDocumentReaderQueryHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Infrastructure.Repositories.PaymentDocumentReaderQueryHandler _handler;

        public PaymentDocumentReaderQueryHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _handler = new Infrastructure.Repositories.PaymentDocumentReaderQueryHandler(_mediator.Object);
        }

        [Fact]
        public void When_Mediator_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Repositories.PaymentDocumentReaderQueryHandler(null));
        }

        [Fact]
        public async Task When_Handler_Should_Success()
        {
            //Arrange
            JObject document =JObject.Parse(TestDataProvider.GetTestData("PaymentMS\\PaymentTransactionDocument"));
            ConfigReader.Set("PaymentDBEndpoint", "Endpoint_Url");
            ConfigReader.Set("PaymentDatabaseName", "Storage");
            ConfigReader.Set("PaymentCollection", "Collection");
            _mediator.Setup(x => x.Send(It.IsAny<VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Object> { document });

            //Act
            var result = await _handler.Handle(new Infrastructure.Repositories.PaymentDocumentReaderQuery() { TransactionId = "80369505", SqlQuery = "TestQuery" }, new CancellationToken());

            //Assert
            Assert.Equal("VS932GWYG7P24", result.OrderId);
            Assert.Equal("Payment authorized", result.Status);
        }

        [Fact]
        public async Task When_Handler_Should_Fail()
        {
            //Arrange
            ConfigReader.Set("PaymentDBEndpoint", "Endpoint_Url");
            ConfigReader.Set("PaymentDatabaseName", "Storage");
            ConfigReader.Set("PaymentCollection", "Collection");
            _mediator.Setup(x => x.Send(It.IsAny<VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosReaderRepoQuery>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Error Occured"));

            //Act
            //Assert
            await Assert.ThrowsAsync<CosmosDBException>(async () => await _handler.Handle(new Infrastructure.Repositories.PaymentDocumentReaderQuery() { TransactionId = "80369505", SqlQuery = "TestQuery" }, new CancellationToken()));

        }
    }
}
