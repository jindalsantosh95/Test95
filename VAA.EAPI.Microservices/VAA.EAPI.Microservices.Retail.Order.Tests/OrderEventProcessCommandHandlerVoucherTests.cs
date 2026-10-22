using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.OrderCreate;
using VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Tests
{
    public class OrderEventProcessCommandHandlerVoucherTests
    {
        [Fact]
        public async Task Handle_WhenVoucherApplied_ShouldPublishVoucherDetailsInOrderDocument()
        {
            var request = TestDataProvider.GetTestData<OrderCreateRequest>("OrderMS\\OrderMsRequest");
            request.VoucherApplied = true;
            request.VoucherDetails = new VoucherDetails
            {
                VoucherOwnerFcAccount = "1234567890",
                AppliedVouchers = [new VoucherRedemption { VoucherId = "TEST-VOUCHER-001", PointsUsed = 55000L }],
                TotalPointsUsed = 55000L
            };
            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");
            var orderDocumentHandler = new OrderDocumentCommandHandlerStub();
            OrderTestsCommon.CreateTestEnvironmentSettings();
            AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
            using var serviceProvider = new ServiceCollection()
                .AddMediatR(config => config.RegisterServicesFromAssemblyContaining<OrderEventProcessCommandHandlerVoucherTests>())
                .AddSingleton<IRequestHandler<OrderCreateServiceCommand, Result<Response<OrderResponse>>>>(new OrderCreateServiceCommandHandlerStub(response))
                .AddSingleton<IRequestHandler<ServiceResponseInsertCommand, bool>, ServiceResponseInsertCommandHandlerStub>()
                .AddSingleton<IRequestHandler<OrderDocumentCommand, bool>>(orderDocumentHandler)
                .AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandlerStub>()
                .AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandlerStub>()
                .BuildServiceProvider();
            var handler = new OrderEventProcessCommandHandler(
                serviceProvider.GetRequiredService<IMediator>(),
                NullLogger<OrderEventProcessCommandHandler>.Instance,
                new MetaPartnerEventCommandHandlerStub(),
                serviceProvider.GetRequiredService<IServiceBusTopicMessageEventCommandHandler>(),
                serviceProvider.GetRequiredService<ITrackAnalyticsEventCommandHandler>());

            var result = await handler.Handle(new OrderEventProcessCommand
            {
                Id = "123",
                Data = VAA.EAPI.Common.Serialisation.Json.Serialize(request)
            }, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(orderDocumentHandler.OrderDocument);
            Assert.True(orderDocumentHandler.OrderDocument.VoucherApplied);
            Assert.NotNull(orderDocumentHandler.OrderDocument.VoucherDetails);
            Assert.Equal("1234567890", orderDocumentHandler.OrderDocument.VoucherDetails.VoucherOwnerFcAccount);
            Assert.Equal(55000L, orderDocumentHandler.OrderDocument.VoucherDetails.TotalPointsUsed);
        }

        [Fact]
        public void MapOrderDocument_WhenVoucherDataIsNotProvided_ShouldUseSafeDefaults()
        {
            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");

            var orderDocument = OrderDocumentMapper.MapOrderDocument(response, Constants.Application.OrderDocumentType, (bool?)null);

            Assert.False(orderDocument.VoucherApplied);
            Assert.Null(orderDocument.VoucherDetails);
        }

        [Fact]
        public void MapOrderDocument_WhenVoucherDetailsAreMissing_ShouldNotMarkVoucherAsApplied()
        {
            var response = TestDataProvider.GetTestData<OrderResponse>("OrderMS\\OrderMsResponse");
            var request = new OrderCreateRequest { VoucherApplied = true };

            var orderDocument = OrderDocumentMapper.MapOrderDocument(response, Constants.Application.OrderDocumentType, request);

            Assert.False(orderDocument.VoucherApplied);
            Assert.Null(orderDocument.VoucherDetails);
        }

        private sealed class OrderCreateServiceCommandHandlerStub(OrderResponse response) : IRequestHandler<OrderCreateServiceCommand, Result<Response<OrderResponse>>>
        {
            public Task<Result<Response<OrderResponse>>> Handle(OrderCreateServiceCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Result.Success(new Response<OrderResponse> { Value = response }));
            }
        }

        private sealed class ServiceResponseInsertCommandHandlerStub : IRequestHandler<ServiceResponseInsertCommand, bool>
        {
            public Task<bool> Handle(ServiceResponseInsertCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }

        private sealed class OrderDocumentCommandHandlerStub : IRequestHandler<OrderDocumentCommand, bool>
        {
            public OrderDocument? OrderDocument { get; private set; }

            public Task<bool> Handle(OrderDocumentCommand request, CancellationToken cancellationToken)
            {
                OrderDocument = request.OrderDocument;
                return Task.FromResult(true);
            }
        }

        private sealed class ServiceBusTopicMessageEventCommandHandlerStub : IServiceBusTopicMessageEventCommandHandler
        {
            public Task<Unit> Handle(ServiceBusTopicMessageEventCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Unit.Value);
            }
        }

        private sealed class TrackAnalyticsEventCommandHandlerStub : ITrackAnalyticsEventCommandHandler
        {
            public Task<bool> Handle(TrackAnalyticsEventCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }

        private sealed class MetaPartnerEventCommandHandlerStub : IMetaPartnerEventCommandHandler
        {
            public Task<bool> Handle(MetaPartnerEventCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}
