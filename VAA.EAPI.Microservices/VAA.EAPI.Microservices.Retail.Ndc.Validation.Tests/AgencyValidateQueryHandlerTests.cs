using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Caching;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Services.Interfaces;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Tests
{
    public class AgencyValidateQueryHandlerTests
    {
        private readonly IMediator _mediator;
        private readonly IAgencyDataRepository _agencyDataRepository;
        private readonly ILogger<AgencyValidateQueryHandler> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAgencyDataService _agencyDataService;
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly ILogger<AgencyDataService> _serviceLogger;
        private readonly ILogger<DistributedCacheService> _cacheLogger;
        private readonly IDistributedCache _distributedCache;
        private readonly AgencyValidateQueryHandler _validateQueryHandler;

        public AgencyValidateQueryHandlerTests()
        {
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AgencyValidateQueryHandler>.Instance;
            _serviceLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AgencyDataService>.Instance;
            _cacheLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<DistributedCacheService>.Instance;
            _agencyDataService = new AgencyDataService(new Infrastructure.Models.AgencyDataContext(), _serviceLogger);
            _distributedCache = new DistributedCache();
            _distributedCacheService = new DistributedCacheService(_distributedCache , _cacheLogger);
            _agencyDataRepository = new AgencyDataRepository(_agencyDataService , _distributedCacheService);
            _validateQueryHandler = new AgencyValidateQueryHandler(_mediator, _agencyDataRepository , _logger);

            ConfigReader.Set("AgencySlidingExpirationInDay", "1");
            ConfigReader.Set("AgencyAbsoluteExpirationInDay", "1");
            ConfigReader.Set("callsecuritysvcflag", "false");
        }

        [Fact]
        public async Task MissingAgentSubKey_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("", "test","IATA");

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10142");
        }

        [Fact]
        public async Task MissingAgentEnvironment_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("123", "","IATA");

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10141");
        }

        [Fact]
        public async Task MissingKeyType_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("123", "test", "IATA")
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "", Name = "testagent", PartyType = EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller }]
                },
                KeyType = ""
            };

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10151");
        }

        [Fact]
        public async Task MissingParticipants_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("123", "test", "IATA");

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10143");
        }

        [Fact]
        public async Task MissingAgencyID_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("123", "test", "IATA")
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1" , AgencyID = "" , Name = "testagent", PartyType = EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller}]
                },
                KeyType = "TEST"
            };

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10143");
        }

        [Fact]
        public async Task MissingPartyType_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("123", "test", "IATA")
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "456789", Name = "testagent" }]
                },
                KeyType = "TEST"
            };

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10148");
        }

        [Fact]
        public async Task InvalidAgencyID_ShowThrowValidationError()
        {
            //Arrange
            var req = new AgencyValidateQuery("123", "test", "IATA")
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "45E6", Name = "testagent" , PartyType = EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller}]
                },
                KeyType = "TEST"
            };

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Contains(result.Value.Errors, x => x.Code == "10144");
        }

        [Fact]
        public async Task AgencyDataValidationTest()
        {
            //Arrange
            var req = new AgencyValidateQuery("456789", "testenv", "IATA")
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "456789", Name = "testagent", PartyType = EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller }]
                },
                KeyType = "DIRECT"
            };
            var data = new List<AgencyData>() { new() { AgencyId = "456789", AccountCode = "NDC", BspActiveStatusInd = true , KeyStatus = "ACTIVE" , KeyType = "DIRECT", BusinessType = "IATA", RequestAgencyId = "456789", RequestAgencyIdType = "IATA", CountryCode = "GB"} };
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);

            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }

            await _distributedCache.SetAsync("456789-testenv-direct",output.ToArray() , token: TestContext.Current.CancellationToken);

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Equal("ACTIVE", result.Value.Value.KeyStatus);
            Assert.Equal("NDC", result.Value.Value.AgencyDetails.AccountCode);
            Assert.Equal("GB", result.Value.Value.AgencyDetails.CountryCode);
            Assert.Equal("456789", result.Value.Value.AgencyDetails.RequestAgencyId);
            Assert.True(result.Value.Value.AgencyPermissions.BspActiveStatusInd);
        }

        [Fact]
        public async Task ValidateIndirectAgent()
        {
            //Arrange
            var req = new AgencyValidateQuery("456789", "testenv", "IATA")
            {
                Parties = new()
                {
                    Participants = [new() { Ordinal = "1", AgencyID = "456789", Name = "testagent", PartyType = EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Seller },
                                    new() { Ordinal = "2", AgencyID = "99999", Name = "ttc", PartyType = EAPI.Domain.Virgin.Models.Common.Participant.PartyTypeEnum.Distributor }]
                },
                KeyType = "INDIRECT"
            };


            var data = new List<AgencyData>() { new() { AgencyId = "456789", AccountCode = "NDC", BspActiveStatusInd = true, KeyStatus = "ACTIVE", KeyType = "INDIRECT", BusinessType = "IATA", RequestAgencyId = "456789", AgencyIdType = "IATA",RequestAgencyIdType = "IATA", CountryCode = "GB" }, 
                                                new() { AgencyId = "99999", AccountCode = "NDC", BspActiveStatusInd = true, KeyStatus = "ACTIVE", KeyType = "INDIRECT", BusinessType = "AGGREGATOR", RequestAgencyId = "99999", AgencyIdType = "TTC", RequestAgencyIdType = "IATA", CountryCode = "GB" } };
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);

            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                gzip.Write(bytes, 0, bytes.Length);
            }

            await _distributedCache.SetAsync("456789-testenv-indirect", output.ToArray(), token: TestContext.Current.CancellationToken);

            //Act
            var result = await _validateQueryHandler.Handle(req, new CancellationToken());

            //Assert
            Assert.Equal("ACTIVE", result.Value.Value.KeyStatus);
            Assert.Equal("INDIRECT", result.Value.Value.KeyType);
            Assert.Equal("NDC", result.Value.Value.AgencyDetails.AccountCode);
            Assert.Equal("GB", result.Value.Value.AgencyDetails.CountryCode);
            Assert.Equal("99999", result.Value.Value.AggregatorDetails.AgencyId);
            Assert.Equal("TTC", result.Value.Value.AggregatorDetails.AgencyIdType);
            Assert.True(result.Value.Value.AgencyPermissions.BspActiveStatusInd);
        }

        [Fact]
        public async Task SecuritySubscriptionFallsBackToAggregatorOnlyQueue()
        {
            var previousCallSecurityServiceFlag = ConfigReader.Get<bool>("callsecuritysvcflag");
            ConfigReader.Set("callsecuritysvcflag", "true");
            try
            {
                var mediator = new SecuritySubscriptionMediator(
                    new SubscriptionResponse { Error = new ErrorDetails { Code = "VE002", Message = "Subscription not found or deleted" }, Status = Status.OK },
                    new SubscriptionResponse { SubscriptionState = "ENABLED", SubscriptionKeys = [new SubscriptionKey { Key = "456789" }], Status = Status.OK });
                var repository = new AgencyDataRepositoryStub(CreateIndirectAgencyData());
                var handler = new AgencyValidateQueryHandler(mediator, repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<AgencyValidateQueryHandler>.Instance);
                var result = await handler.Handle(CreateIndirectRequest(), CancellationToken.None);

                Assert.Equal(Status.OK, result.Value.Status);
                Assert.Equal(["456789-99999", "99999"], mediator.RequestedAgencyIds);
            }
            finally
            {
                ConfigReader.Set("callsecuritysvcflag", previousCallSecurityServiceFlag.ToString());
            }
        }

        [Fact]
        public async Task SecuritySubscriptionFallsBackToAggregatorWhenReturnedKeyDoesNotMatchRequest()
        {
            var previousCallSecurityServiceFlag = ConfigReader.Get<bool>("callsecuritysvcflag");
            ConfigReader.Set("callsecuritysvcflag", "true");
            try
            {
                var mediator = new SecuritySubscriptionMediator(
                    new SubscriptionResponse { SubscriptionState = "ENABLED", SubscriptionKeys = [new SubscriptionKey { Key = "different-key" }], Status = Status.OK },
                    new SubscriptionResponse { SubscriptionState = "ENABLED", SubscriptionKeys = [new SubscriptionKey { Key = "requested-key" }], Status = Status.OK });
                var repository = new AgencyDataRepositoryStub(CreateIndirectAgencyData());
                var handler = new AgencyValidateQueryHandler(mediator, repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<AgencyValidateQueryHandler>.Instance);
                var request = CreateIndirectRequest();
                request.AgentSubscriptionKey = "requested-key";

                var result = await handler.Handle(request, CancellationToken.None);

                Assert.Equal(Status.OK, result.Value.Status);
                Assert.Equal(["456789-99999", "99999"], mediator.RequestedAgencyIds);
            }
            finally
            {
                ConfigReader.Set("callsecuritysvcflag", previousCallSecurityServiceFlag.ToString());
            }
        }

        [Fact]
        public async Task SecuritySubscriptionReturnsErrorWhenAggregatorOnlyFallbackFails()
        {
            var previousCallSecurityServiceFlag = ConfigReader.Get<bool>("callsecuritysvcflag");
            ConfigReader.Set("callsecuritysvcflag", "true");
            try
            {
                var mediator = new SecuritySubscriptionMediator(new SubscriptionResponse { Error = new() });
                var repository = new AgencyDataRepositoryStub(CreateIndirectAgencyData());
                var handler = new AgencyValidateQueryHandler(mediator, repository, Microsoft.Extensions.Logging.Abstractions.NullLogger<AgencyValidateQueryHandler>.Instance);
                var result = await handler.Handle(CreateIndirectRequest(), CancellationToken.None);

                Assert.Contains(result.Value.Errors, x => x.Code == "10140");
                Assert.Equal(2, mediator.RequestedAgencyIds.Count);
            }
            finally
            {
                ConfigReader.Set("callsecuritysvcflag", previousCallSecurityServiceFlag.ToString());
            }
        }

        private static AgencyValidateQuery CreateIndirectRequest() => new("456789", "testenv", "IATA")
        {
            Parties = new()
            {
                Participants =
                [
                    new() { AgencyID = "456789", Name = "testagent", PartyType = Participant.PartyTypeEnum.Seller },
                    new() { AgencyID = "99999", Name = "aggregator", PartyType = Participant.PartyTypeEnum.Distributor }
                ]
            },
            KeyType = "DIRECT"
        };

        private static List<AgencyData> CreateIndirectAgencyData() =>
        [
            new() { AgencyId = "456789", KeyStatus = "ACTIVE", KeyType = "DIRECT", BusinessType = "TIDS", RequestAgencyId = "456789", AgencyIdType = "TIDS", RequestAgencyIdType = "TIDS", AccountCode = "NDC" },
            new() { AgencyId = "99999", KeyStatus = "ACTIVE", KeyType = "INDIRECT", BusinessType = "AGGREGATOR", RequestAgencyId = "99999", AgencyIdType = "TIDS", RequestAgencyIdType = "TIDS" }
        ];

    }

    internal sealed class AgencyDataRepositoryStub(List<AgencyData> agencyData) : IAgencyDataRepository
    {
        public Task<List<AgencyData>> GetAgencyDetails(AgencyValidateQuery requestData) => Task.FromResult(agencyData);
    }

    internal sealed class SecuritySubscriptionMediator(SubscriptionResponse first, SubscriptionResponse? second = null) : IMediator
    {
        public List<string> RequestedAgencyIds { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not SecuritySvcRetrieveQuery query)
                throw new NotSupportedException($"Unsupported request type: {request.GetType().FullName}");
            RequestedAgencyIds.Add(query.AgencyID);
            var response = RequestedAgencyIds.Count == 1 ? first : second ?? first;
            return Task.FromResult((TResponse)(object)response);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Publish(object notification, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => throw new NotSupportedException();
    }
}
