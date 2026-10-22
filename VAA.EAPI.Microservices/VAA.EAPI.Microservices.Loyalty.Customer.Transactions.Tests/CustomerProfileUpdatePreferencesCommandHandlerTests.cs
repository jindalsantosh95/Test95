using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using Xunit;
using CustomerProfileDocResponse = VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile.CustomerProfileDocResponse;
using CustomerProfilePreference = VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile.Preference;
using InfrastructureConstants = VAA.EAPI.Infrastructure.Constants.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests;

public class CustomerProfileUpdatePreferencesCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CustomerProfileUpdatePreferencesCommandHandler> _logger = NullLogger<CustomerProfileUpdatePreferencesCommandHandler>.Instance;
    private readonly CustomerProfileUpdatePreferencesCommandHandler _handler;
    private readonly ITableStorageService<EventStoreDatabaseEntity> _tableStorage;
    CustomerProfileDTO? replacedDoc = null;

    public CustomerProfileUpdatePreferencesCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
    {
        ConfigReader.Set(InfrastructureConstants.MicroServiceDBKey, "test-db-key");
        ConfigReader.Set(InfrastructureConstants.MicroServiceEventStoreTableName, "test-event-store");

        ConfigReader.Set(EnvironmentSettings.CustomerProfileDocType, "CustomerProfile");
        ConfigReader.Set(EnvironmentSettings.TransactionDBEndPoint, "https://tempuri.org");
        ConfigReader.Set(EnvironmentSettings.TransactionDB, "transactions-db");
        ConfigReader.Set(EnvironmentSettings.TransactionDBCollection, "transactions-collection");
        ConfigReader.Set(EnvironmentSettings.ServiceBusConnectionString, "test-connection-string");

        _serviceProvider = new ServiceCollection()
            .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>>()
            .AddSingleton<IMessageBusFactory , MessageBusFactory>()
            .AddSingleton<ICosmosClientFactory, TestCosmosClientFactory>()
            .AddSingleton<CosmosModifyServiceCommandHandler>()
            .AddSingleton<EventStoreRequestInsertCommandHandler>()
            .AddSingleton<EventStoreRequestDeleteCommandHandler>()
            .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
            .AddSingleton<CustomerProfileQueryHandler>()
            .AddSingleton<ICustomerProfileUpdatePreferencesCommand, CustomerProfileUpdatePreferencesCommandHandler>()
            .AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                config.RegisterServicesFromAssemblyContaining(typeof(CustomerProfileQueryHandler));
            })
            .BuildServiceProvider();

        var customerProfileDoc = CreateCustomerProfileDto("test-customer-id");
        _tableStorage = _serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;

        _handler = new CustomerProfileUpdatePreferencesCommandHandler(CreateCustomerProfileQueryHandler(customerProfileDoc), CreateCosmosModifyServiceCommandHandler(doc => replacedDoc = (CustomerProfileDTO)doc), new EventStoreRequestInsertCommandHandler(_tableStorage, NullLogger<EventStoreRequestInsertCommandHandler>.Instance)!, new EventStoreRequestDeleteCommandHandler(_tableStorage, NullLogger<EventStoreRequestDeleteCommandHandler>.Instance), _logger, new ServiceBusTopicMessageEventCommandHandler(_serviceProvider.GetService<IMessageBusFactory>()));
    }

    [Fact]
    public async Task Handle_ShouldUpdatePreferenceToY_WhenPreferenceValueIsY()
    {
        AsyncContext.Current = new AsyncContext();
        AsyncContext.Current.SetCorrelationId(new Guid("11111111-1111-1111-1111-111111111111"));

        var request = new CustomerProfileCommand
        {
            CustomerId = "test-customer-id",
            LastUpdatedBy = "WEB-MYA",
            Preference =
            [
                new Preference { PreferenceType = PreferenceTypes.VAAMarketingPermissions, PreferenceValue = "Y" }
            ]
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.PreferenceValue);

        Assert.NotNull(replacedDoc);
        var updatedPref = replacedDoc.Preferences.Single(p => p.PreferenceType == PreferenceTypes.VAAMarketingPermissions.ToString());
        Assert.Equal("Y", updatedPref.PreferenceValue);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePreferenceToN_WhenPreferenceValueIsN()
    {
        AsyncContext.Current = new AsyncContext();
        AsyncContext.Current.SetCorrelationId(new Guid("22222222-2222-2222-2222-222222222222"));

        var request = new CustomerProfileCommand
        {
            CustomerId = "test-customer-id",
            LastUpdatedBy = "WEB-MYA",
            Preference =
            [
                new Preference { PreferenceType = PreferenceTypes.VAAMarketingPermissions, PreferenceValue = "N" }
            ]
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.PreferenceValue);

        Assert.NotNull(replacedDoc);
        var updatedPref = replacedDoc.Preferences.Single(p => p.PreferenceType == PreferenceTypes.VAAMarketingPermissions.ToString());
        Assert.Equal("N", updatedPref.PreferenceValue);
    }

    [Fact]
    public async Task Handle_ShouldFailToUpdatePreferenceToN_WhenFfpPreferenceValueIsInvalid()
    {
        AsyncContext.Current = new AsyncContext();
        AsyncContext.Current.SetCorrelationId(new Guid("22222222-2222-2222-2222-222222222222"));

        var request = new CustomerProfileCommand
        {
            CustomerId = "test-customer-id",
            LastUpdatedBy = "WEB-MYA",
            Preference =
            [
                new Preference { PreferenceType = PreferenceTypes.PreferredPartnerFFP, PreferenceValue = "Delta" }
            ]
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(response.PreferenceValue);
        Assert.Equal("10200", response.ErrorDetails.First().Code);        
    }


    [Fact]
    public async Task Handle_ShouldRollbackEventStore_WhenAuditPublishFails()
    {
        AsyncContext.Current = new AsyncContext();
        var correlationId = new Guid("33333333-3333-3333-3333-333333333333");
        AsyncContext.Current.SetCorrelationId(correlationId);

        var messageBusFactory = new MessageBusFactory();
        var auditBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get(EnvironmentSettings.ServiceBusConnectionString), Constants.Application.loyaltyAuditPreferenceSource);
        auditBus.OnMessage += _ => throw new InvalidOperationException("Simulated audit publish failure");

        var request = new CustomerProfileCommand
        {
            CustomerId = "test-customer-id",
            LastUpdatedBy = "WEB-MYA",
            Preference =
            [
                new Preference { PreferenceType = PreferenceTypes.DoNotEmail, PreferenceValue = "Y" }
            ]
        };

        var response = await new CustomerProfileUpdatePreferencesCommandHandler(CreateCustomerProfileQueryHandler(CreateCustomerProfileDto("test-customer-id")), CreateCosmosModifyServiceCommandHandler(doc => replacedDoc = (CustomerProfileDTO)doc), new EventStoreRequestInsertCommandHandler(_tableStorage, NullLogger<EventStoreRequestInsertCommandHandler>.Instance)!, new EventStoreRequestDeleteCommandHandler(_tableStorage, NullLogger<EventStoreRequestDeleteCommandHandler>.Instance), _logger, new ServiceBusTopicMessageEventCommandHandler(messageBusFactory)).Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.PreferenceValue);

        Assert.NotNull(replacedDoc);
        var updatedPref = replacedDoc.Preferences.Single(p => p.PreferenceType == PreferenceTypes.DoNotEmail.ToString());
        Assert.Equal("Y", updatedPref.PreferenceValue);

        var rolledBack = await _tableStorage!.RetrieveAsync(Constants.Application.LoyaltyAuditPreferenceEventType, correlationId.ToString());
        Assert.Null(rolledBack);
    }

    [Fact]
    public async Task Handle_ShouldRollbackEmailEventStore_WhenEmailPublishFails()
    {
        AsyncContext.Current = new AsyncContext();
        var auditCorrelationId = new Guid("44444444-4444-4444-4444-444444444444");
        var emailCorrelationId = new Guid("55555555-5555-5555-5555-555555555555");
        AsyncContext.Current.SetCorrelationId(auditCorrelationId);

        var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>();

        // After the audit event is published, switch correlation id so the email event stores under a different RowKey.
        var auditBus = (MessageBus)messageBusFactory!.GetClient(ConfigReader.Get(EnvironmentSettings.ServiceBusConnectionString), Constants.Application.loyaltyAuditPreferenceSource);
        auditBus.OnMessage += _ => AsyncContext.Current.SetCorrelationId(emailCorrelationId);

        var emailBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get(EnvironmentSettings.ServiceBusConnectionString), Constants.Application.ManagaeAccountUpdateTopicName);
        emailBus.OnMessage += _ => throw new InvalidOperationException("Simulated email publish failure");

        var request = new CustomerProfileCommand
        {
            CustomerId = "test-customer-id",
            LastUpdatedBy = "WEB-MYA",
            Preference =
            [
                new Preference { PreferenceType = PreferenceTypes.VAAMarketingPermissions, PreferenceValue = "Y" }
            ]
        };

        var response = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(response);
        Assert.True(response.PreferenceValue);

        Assert.NotNull(replacedDoc);
        var updatedPref = replacedDoc.Preferences.Single(p => p.PreferenceType == PreferenceTypes.VAAMarketingPermissions.ToString());
        Assert.Equal("Y", updatedPref.PreferenceValue);

        var auditEvent = await _tableStorage!.RetrieveAsync(Constants.Application.LoyaltyAuditPreferenceEventType, auditCorrelationId.ToString());
        Assert.NotNull(auditEvent);

        var rolledBackEmailEvent = await _tableStorage.RetrieveAsync(Constants.Application.UpdateCustomerEventSubject, emailCorrelationId.ToString());
        Assert.Null(rolledBackEmailEvent);
    }

    private static CustomerProfileDTO CreateCustomerProfileDto(string customerId) =>
        new()
        {
            Id = "profile-doc-id",
            CustomerId = customerId,
            CreatedBy = "seed",
            Brand = "VAA",
            SchemaVersion = Constants.Application.CustomerProfileSchemaVersion,
            Preferences = Array.Empty<CustomerProfilePreference>(),
            PartitionKey = customerId
        };

    private static CustomerProfileQueryHandler CreateCustomerProfileQueryHandler(CustomerProfileDTO customerProfileDoc)
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock
            .Setup(m => m.Send(It.IsAny<CustomerTransactionRepoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<object> { customerProfileDoc });

        return new CustomerProfileQueryHandler(mediatorMock.Object, NullLogger<TransactionBaseQueryHandler<CustomerProfileDocResponse>>.Instance);
    }

    private static CosmosModifyServiceCommandHandler CreateCosmosModifyServiceCommandHandler(Action<CosmosDocument> onReplace)
    {
        var containerMock = new Mock<Container>();
        containerMock
            .Setup(c => c.ReplaceItemAsync(
                It.IsAny<CosmosDocument>(),
                It.IsAny<string>(),
                It.IsAny<PartitionKey?>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<CosmosDocument, string, PartitionKey?, ItemRequestOptions, CancellationToken>((doc, _, _, _, _) => onReplace(doc))
            .ReturnsAsync((ItemResponse<CosmosDocument>)null!);

        var cosmosClient = new TestCosmosClient(containerMock.Object);
        var cosmosFactory = new TestCosmosClientFactory(cosmosClient);

        return new CosmosModifyServiceCommandHandler(NullLogger<CosmosModifyServiceCommandHandler>.Instance, cosmosFactory);
    }

    private sealed class TestCosmosClientFactory : ICosmosClientFactory
    {
        private readonly CosmosClient _cosmosClient;

        public TestCosmosClientFactory(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        }

        public CosmosClientOptions GetDefaultCosmosClientOptions() => new();

        public CosmosClient GetClient(string Endpoint, CosmosClientOptions options = null) => _cosmosClient;
    }

    private sealed class TestCosmosClient : CosmosClient
    {
        private const string TestEndpoint = "https://example.documents.azure.com:443/";
        private const string TestKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";

        private readonly Container _container;

        public TestCosmosClient(Container container)
            : base(TestEndpoint, TestKey, new CosmosClientOptions())
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public override Container GetContainer(string databaseId, string containerId) => _container;
    }
}
