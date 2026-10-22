using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Infrastructure.Extentions;
using VAA.EAPI.Infrastructure.HealthChecks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Application.OrderChangeNotif;
using VAA.EAPI.NDC.Order.Application.OrderCreate;
using VAA.EAPI.NDC.Order.Application.OrderHistory;
using VAA.EAPI.NDC.Order.Application.OrderList;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ConfigMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp;
using VAA.EAPI.NDC.Order.Infrastructure.Services.InRule;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OfferMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderTicketMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentIFGMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;

namespace VAA.EAPI.NDC.Order
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var host = new HostBuilder()
                    .ConfigureAppConfiguration(builder =>
                    {
                        builder.AddCommandLine(args);
                        builder.AddEnvironmentConfiguration();
                    })
                    .ConfigureFunctionsWorkerDefaults(worker =>
                    {
                        worker.AddDefaultMiddleware();
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddDefaultServices();
                        services.AddHealthChecks()
                           .AddTypeActivatedCheck<HttpHealthCheck>(name: "OfferMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OfferMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OfferMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ConfigMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.ConfigMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ConfigurationMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ValidationMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.ValidationMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ValidationMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "BrandMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.BrandMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.BrandMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "SeatMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.SeatMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.SeatMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "FlightServicesMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.FlightServicesMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.FlightServicesMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderDataMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OrderDataMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderDataMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "Air4OrderSearchMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.Air4OrderSearchMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.Air4OrderSearchMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderSearchMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OrderSearchMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderSearchMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ReferenceMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.ReferenceMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ReferenceMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OrderMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "Air4OrderMgmtMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.Air4OrderMgmtMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderMgmtMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderMgmtMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OrderMgmtMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderChangeMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderTicketMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OrderTicketMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderTicketMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "EmailMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.EmailMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.EmailMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "PaymentMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.PaymentMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.PaymentMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "OrderNotificationMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.OrderNotificationMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.OrderNotificationMSSubscriptionKey)] }
                                            }
                       }).AddTypeActivatedCheck<HttpHealthCheck>(name: "IFGPaymentMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.IFGPaymentMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.PaymentIFGMSSubscriptionKey)] }
                                            }
                        }).AddTypeActivatedCheck<HttpHealthCheck>(name: "ARCPaymentMSHealthCheck",
                                      failureStatus: HealthStatus.Unhealthy,
                                      tags: new[] { nameof(HttpHealthCheck) },
                                      args: new object[]
                                      {
                                            ConfigReader.Get(Constants.Environment.ARCPaymentMSHeartBeatUrl),
                                            HttpMethod.Get,
                                            HttpStatusCode.OK,
                                            new Dictionary<string, IEnumerable<string>>
                                            {
                                                { "Ocp-Apim-Subscription-Key", [ConfigReader.Get(Constants.Environment.ARCPaymentMSSubscriptionKey)] }
                                            } 
                        });
                        services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();
                        services.AddStackExchangeRedisCache(options => options.Configuration = ConfigReader.Get(Common.Constants.Environment.RedisCacheConnectionString));
                        services.AddMediatR(config =>
                        {
                            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                            config.RegisterServicesFromAssemblyContaining(typeof(ValidationMSCommandHandler));
                            config.RegisterServicesFromAssemblyContaining(typeof(TrackAnalyticsEventCommand));
                            config.RegisterServicesFromAssemblyContaining(typeof(ServiceBusTopicMessageEventCommand));
                            config.RegisterServicesFromAssemblyContaining(typeof(ReferenceMSCurrencyQuery));
                        });
                        services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
                        services.AddSingleton(typeof(IOrderChangeNotifCommand), typeof(OrderChangeNotifCommandHandler));
                        services.AddSingleton(typeof(ITtlExpiryCommand), typeof(TtlExpiryCommandHandler)); 
                        services.AddSingleton(typeof(IOrderChangeAmendApisCommand), typeof(OrderChangeAmendApisCommandHandler));
                        services.AddTransient<IOrderChangeSplitPnrCommand, OrderChangeSplitPnrCommandHandler>();
                        services.AddSingleton(typeof(IOrderListCommand), typeof(OrderListCommandHandler));
                        services.AddSingleton(typeof(IOrderListQuery), typeof(OrderListQueryHandler));
                        services.AddSingleton(typeof(ICurrencyConversionQuery), typeof(CurrencyConversionQueryHandler));
                        services.AddSingleton(typeof(IOrderNotificationMSCommand), typeof(OrderNotificationMSCommandHandler));
                        services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
                        services.AddSingleton<IInRuleService, InRuleService>();
                        services.AddSingleton(typeof(IOrderCreateIFGCommand), typeof(OrderCreateIFGCommandHandler));
                        services.AddSingleton(typeof(IOrderCreateEasyPayCommand), typeof(OrderCreateEasyPayCommandHandler));
                        services.AddSingleton(typeof(IOrderHistoryQuery), typeof(OrderHistoryQueryHandler));
                        services.AddSingleton(typeof(IBrandMSPTCCommand), typeof(BrandMSCommandHandlerPTC));
                        services.AddSingleton(typeof(IBrandMSCommand), typeof(BrandMSCommandHandler));
                        services.AddSingleton<IGdsOrderResponseMapper, GdsOrderResponseMapper>();
                        services.AddSingleton<OrderDataMSQueryHandler>();
                        services.AddSingleton<ValidationMSCommandHandler>();
                        services.AddSingleton<OrderSearchMSCommandHandler>();
                        services.AddSingleton<OrderSearchMSOrderHistoryCommandHandler>();
                        services.AddSingleton<PaymentMSRetrieveQueryHandler>();
                        services.AddSingleton<ManageCustomerRepoCommandHandler>();
                        services.AddSingleton<SplitPnrRepoCommandHandler>();
                        services.AddSingleton<PublishOrderDocMSCommandHandler>();
                        services.AddSingleton<PublishSplitOrderHistoryCommandHandler>();
                        services.AddSingleton<ReferenceMSCurrencyQueryHandler>();
                        services.AddSingleton<OfferMSQueryHandler>();
                        services.AddSingleton<OrderMSCommandHandler>();
                        services.AddSingleton<OrderTicketMSCommandHandler>();
                        services.AddSingleton<AirportDetailsQueryHandler>();
                        services.AddSingleton<TrackAnalyticsEventCommandHandler>();
                        services.AddSingleton<PaymentIFGMSAuthorizeCommandHandler>();
                        services.AddSingleton<PaymentIFGMSOrderCommandHandler>();
                        services.AddSingleton<PaymentIFGMSExchangeCommandHandler>();
                        services.AddSingleton<PaymentIFGMSOrderRefundCommandHandler>();
                        services.AddSingleton<PaymentIFGMSAuthorizeCancelCommandHandler>();
                        services.AddSingleton<SeatMSQueryHandler>();
                        services.AddSingleton<SeatMSRetrieveSeatMapQueryHandler>();
                        services.AddSingleton<SeatMSSeatFulfillmentCommandHandler>();
                        services.AddSingleton<FlightServicesDocQueryHandler>();
                        services.AddSingleton<IFGAgentFopLookUpCommandHandler>();
                        services.AddSingleton<ValidationMSAgencyOCNDetailsQueryHandler>();
                        services.AddSingleton<ISplitPnrPaymentCommand, SplitPnrPaymentCommandHandler>();
                        services.AddSingleton(typeof(IHttpClientFactory), typeof(Common.Http.HttpClientFactory)); 
                        services.AddSingleton<ICreatePaymentDocumentCommand, CreatePaymentDocumentCommandHandler>();
                        services.AddSingleton<ISendOrderEmailCommand, SendOrderEmailCommandHandler>();
                        services.AddSingleton<ICityCodeQuery, CityCodeQueryHandler>();
                        services.AddSingleton<ICheckAgentStatusQuery, CheckAgentStatusQueryHandler>();
                        services.AddSingleton<ITransactionCommand, TransactionCommandHandler>();
                        services.AddSingleton<IOrderChangeDisruptionCommand, OrderChangeDisruptionCommandHandler>();
                        services.AddSingleton<IOrderChangeCancelHoldBookingCommand, OrderChangeCancelHoldBookingCommandHandler>();
                        services.AddSingleton<IOrderChangeReshopDisruptionCommand, OrderChangeReshopDisruptionCommandHandler>();
                        services.AddSingleton<VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt.INdcQueueMgmtFetchDuplicatePnrsCommand, VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt.NdcQueueMgmtFetchDuplicatePnrsCommandHandler>();
                        services.AddSingleton<VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt.INdcQueueMgmtRemoveDuplicatePnrsCommand, VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt.NdcQueueMgmtRemoveDuplicatePnrsCommandHandler>();
                        services.AddSingleton<IOrderChangeDuplicatePNRNotifCommand, OrderChangeDuplicatePNRNotifCommandHandler>();
                        services.AddSingleton<IARCOrderDataMSQuery, ARCOrderDataMSQueryHandler>();
                        services.AddSingleton<IOrderChangeReshopHoldBookingCommand, OrderChangeReshopHoldBookingCommandHandler>();
                        services.AddSingleton<IOrderChangeReshopConfirmHoldBookingCommand, OrderChangeReshopConfirmHoldBookingCommandHandler>();
                        services.AddSingleton<SabreOrderManagementMSCommandHandler>();
                        services.AddHttpContextAccessor();
                        services.AddSingleton<ITrackAnalyticsEventCommandHandler, TrackAnalyticsEventCommandHandler>();
                        services.AddSingleton<IServiceBusTopicMessageEventCommandHandler, ServiceBusTopicMessageEventCommandHandler>();
                        services.AddSingleton<PaymentMSAuthorizeCommandHandler>();
                        services.AddSingleton<PaymentMSCaptureCommandHandler>();
                        services.AddSingleton<PaymentMSRefundCommandHandler>();
                        services.AddSingleton<OrderManagementMSCommandHandler>();
                        services.AddSingleton<OrderCancelCommandHandler>();
                    })
                    .ConfigureOpenApi()
                    .Build();

            return host.RunAsync();
        }
    }
}

