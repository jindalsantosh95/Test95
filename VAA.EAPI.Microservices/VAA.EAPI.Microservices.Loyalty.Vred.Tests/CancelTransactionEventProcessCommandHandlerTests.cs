using MediatR;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredStatements;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CancelTransactionEventProcessCommandHandlerTests
    {
        private readonly Mock<ITransactionDBRepository> _mockTransactionDBRepository;
        private readonly Mock<IMediator> _mediator;        
        private readonly Mock<ILogger<CancelTransactionEventProcessCommandHandler>> _logger;
        private readonly CancelTransactionEventProcessCommandHandler _handler;       
       

        public CancelTransactionEventProcessCommandHandlerTests()
        {
            _mockTransactionDBRepository = new Mock<ITransactionDBRepository>();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CancelTransactionEventProcessCommandHandler>>();            
            _handler = new CancelTransactionEventProcessCommandHandler(_mediator.Object, _logger.Object, _mockTransactionDBRepository.Object);           
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelTransactionEventProcessCommandHandler(_mediator.Object, null, _mockTransactionDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelTransactionEventProcessCommandHandler(null, _logger.Object, _mockTransactionDBRepository.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTransactionDBRepositoryObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new CancelTransactionEventProcessCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public void CommandHandler_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionEventProcessCommand>("VRedMS\\Transaction\\CancelTransactionCloudEvent");
            VredStatementResponse vredStatementResponse = TestDataProvider.GetTestData<VredStatementResponse>("VRedMS\\Transaction\\VredStatementResponse");
            UpdateTransactionResponse response = TestDataProvider.GetTestData<UpdateTransactionResponse>("VRedMS\\Transaction\\CancelTransactionResponse");

            _mediator.Setup(m => m.Send(It.IsAny<VredStatementsRepoQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(vredStatementResponse);
            _mediator.Setup(m => m.Send(It.IsAny<CancelTransactionEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<CancelVredTransactionRepoQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(response);
            _mockTransactionDBRepository.Setup(m => m.AddTransactionAuditInfo("1-A976-1", "1-2OHIEB", "Test.mail@fly.virgin.com", "d", "rr", "t", "hh", "jj", "ll", "pp", "rtt", "gfgfd", "fsdf").Result);
           

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
            Assert.Equal("AZ1716305170926", Json.Deserialize<TransactionResponse>(result.Result.Value.Value).PartnerTransactionId);
            Assert.Equal("VAA", Json.Deserialize<TransactionResponse>(result.Result.Value.Value).PartnerCode);
            Assert.Equal("2105128793", Json.Deserialize<TransactionResponse>(result.Result.Value.Value).PartnerUserId);

        }

        [Fact]
        public void CommandHandler_Error()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionEventProcessCommand>("VRedMS\\Transaction\\CancelTransactionCloudEvent");
            VredStatementResponse vredStatementResponse = TestDataProvider.GetTestData<VredStatementResponse>("VRedMS\\Transaction\\VredStatementResponse");
            UpdateTransactionResponse response = TestDataProvider.GetTestData<UpdateTransactionResponse>("VRedMS\\Transaction\\CancelTransactionErrorResponse");

            _mediator.Setup(m => m.Send(It.IsAny<VredStatementsRepoQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(vredStatementResponse);
            _mediator.Setup(m => m.Send(It.IsAny<CancelTransactionEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _mediator.Setup(m => m.Send(It.IsAny<CancelVredTransactionRepoQuery>(), System.Threading.CancellationToken.None)).ReturnsAsync(response);
            _mockTransactionDBRepository.Setup(m => m.AddTransactionAuditInfo("1-A976-1", "1-2OHIEB", "Test.mail@fly.virgin.com", "d", "rr", "t", "hh", "jj", "ll", "pp", "rtt", "gfgfd", "fsdf").Result);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
            Assert.Contains("schema violations found", Json.Deserialize<TransactionErrorResponse>(result.Result.Value.Value).Message);
            Assert.Equal("400", Json.Deserialize<TransactionErrorResponse>(result.Result.Value.Value).Status);
        }
    }
}
