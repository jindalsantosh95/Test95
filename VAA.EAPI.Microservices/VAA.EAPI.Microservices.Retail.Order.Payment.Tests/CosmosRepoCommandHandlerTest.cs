using MediatR;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using Xunit;
using VAA.EAPI.Infrastructure.Services;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using System.Threading;
using CSharpFunctionalExtensions;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class CosmosRepoCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<Infrastructure.Repositories.CosmosRepoCommandHandler>> _logger;
        private readonly Infrastructure.Repositories.CosmosRepoCommandHandler _handler;

        public CosmosRepoCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _handler = new Infrastructure.Repositories.CosmosRepoCommandHandler(_mediator.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Repositories.CosmosRepoCommandHandler(null));
        }
        
    }
    
}
