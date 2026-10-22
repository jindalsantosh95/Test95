using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Tests
{
    public class OrderChangeEventProcessCommandTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<OrderChangeEventSubscribeCommandHandler>> _logger;
        private readonly Mock<TableStorageService<ReferenceDataEntity>> _tableStorageService;
        private readonly OrderChangeEventSubscribeCommandHandler _handler;
        private readonly Mock<ICachedChannel> _mockcacheChannel;
        private readonly Mock<IOfferSearchItems> _offerSearchItems;
        public OrderChangeEventProcessCommandTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<OrderChangeEventSubscribeCommandHandler>>();
            _tableStorageService = new Mock<TableStorageService<ReferenceDataEntity>>();
            _mockcacheChannel = new Mock<ICachedChannel>();
            _offerSearchItems = new Mock<IOfferSearchItems>();
            _handler = new OrderChangeEventSubscribeCommandHandler(_mediator.Object, _logger.Object, _tableStorageService.Object, _mockcacheChannel.Object, _offerSearchItems.Object);
            OrderManagementTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenArgumentNullExceptionOccur()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<Air4CreateSessionCommand>(), System.Threading.CancellationToken.None)).Throws<ArgumentNullException>();
            //Act
            var result = await _handler.Handle(new OrderChangeEventProcessCommand(), new System.Threading.CancellationToken());
            //Assert
            Assert.True(result.IsFailure);
        }
        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExceptionOccurs()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<Air4CreateSessionCommand>(), System.Threading.CancellationToken.None)).Throws<Exception>();
            //Act
            var result = await _handler.Handle(new OrderChangeEventProcessCommand(), new System.Threading.CancellationToken());
            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task Handler_ShouldReturnFailureWhenExternalServiceIntegrationExceptionOccurs()
        {
            //Arrange
            _mediator.Setup(m => m.Send(It.IsAny<Air4CreateSessionCommand>(), System.Threading.CancellationToken.None)).Throws<ExternalServiceIntegrationException>();
            //Act
            var result = await _handler.Handle(new OrderChangeEventProcessCommand(), new System.Threading.CancellationToken());
            //Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeEventSubscribeCommandHandler(null, _logger.Object, _tableStorageService.Object, _mockcacheChannel.Object, _offerSearchItems.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeEventSubscribeCommandHandler(_mediator.Object, null, _tableStorageService.Object, _mockcacheChannel.Object, _offerSearchItems.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenTableStorageServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeEventSubscribeCommandHandler(_mediator.Object, _logger.Object, null, _mockcacheChannel.Object, _offerSearchItems.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenCachedChannelObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeEventSubscribeCommandHandler(_mediator.Object, _logger.Object, _tableStorageService.Object, null, _offerSearchItems.Object));
        }
        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenOfferSearchObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeEventSubscribeCommandHandler(_mediator.Object, _logger.Object, _tableStorageService.Object, _mockcacheChannel.Object, null));
        }
    }
}
