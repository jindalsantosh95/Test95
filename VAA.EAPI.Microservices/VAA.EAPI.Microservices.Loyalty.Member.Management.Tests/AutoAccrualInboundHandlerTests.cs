using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class AutoAccrualInboundHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AutoAccrualInboundHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
            VAA.EAPI.Common.Helpers.ConfigReader.Set("MicroServiceDBKey", "UseDevelopmentStorage=true");
            VAA.EAPI.Common.Helpers.ConfigReader.Set("MicroServiceEventStoreTableName", "eventstore");
            VAA.EAPI.Common.Helpers.ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "test-service-bus-connection");
            VAA.EAPI.Common.Helpers.ConfigReader.Set(Constants.Environment.VredServiceBusTopicName, "vredeventmessages");
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<CustomerSearchDocumentServiceQueryHandler, FakeCustomerSearchDocumentServiceQueryHandler>()
                .AddSingleton<ICustomerDocumentQuery, FakeCustomerDocumentQuery>()
                .AddSingleton<FlyingClubMemberDocumentServiceQueryHandler, FakeFlyingClubMemberDocumentServiceQueryHandler>()
                .AddSingleton<IAutoAccrualReferenceValidationService, PassingReferenceValidationService>()
                .AddSingleton<ICommonRefDBServices, FakeCommonRefDbServices>()
                .AddSingleton<IRulesEngineService, FakeRulesEngineService>()
                .AddSingleton<ICustomerTransactionService, FakeCustomerTransactionService>()
                .AddSingleton<ICachedPartners, FakeCachedPartners>()
                .AddSingleton<ICachedFlightProduct, FakeCachedFlightProduct>()
                .AddSingleton<ITxnStatementTextService, FakeTransactionStatementTextService>()
                .AddSingleton<TransactionStatementTextQueryHandler>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(VAA.EAPI.Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<AutoAccrualInboundHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task HandleAsync_WhenOriginMatchesDestination_ReturnsCleanValidationError()
        {
            var handler = _serviceProvider.GetRequiredService<AutoAccrualInboundHandler>();

            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() =>
                handler.HandleAsync(Payload("LHR", "LHR", "VN", "VN"), CancellationToken.None));

            Assert.Equal("CorrelationId: correlation-id. Origin and destination cannot be the same.", exception.Message);
        }

        [Fact]
        public async Task HandleAsync_WhenOperatingAirlineIsInvalid_ReturnsCleanValidationError()
        {
            var handler = _serviceProvider.GetRequiredService<AutoAccrualInboundHandler>();

            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() =>
                handler.HandleAsync(Payload("LHR", "ARN", "VN", "SK"), CancellationToken.None));

            Assert.Equal("CorrelationId: correlation-id. Operating airline is not valid", exception.Message);
            Assert.Equal(Constants.Application.AutoAccrualInvalidAirlineCode, exception.ErrorCode);
        }

        [Theory]
        [InlineData("OTHER", "TEST", "003.006")]
        [InlineData("TEST", "OTHER", "003.007")]
        public async Task HandleAsync_WhenPnrNameDoesNotMatch_ReturnsApplicableNlaErrorCode(string surname, string givenName, string errorCode)
        {
            var handler = _serviceProvider.GetRequiredService<AutoAccrualInboundHandler>();

            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() =>
                handler.HandleAsync(MemberValidationPayload(surname, givenName, "789621323"), CancellationToken.None));

            Assert.Equal(errorCode, exception.ErrorCode);
        }

        [Theory]
        [InlineData("unknown", "050.000")]
        [InlineData("http-not-found", "050.000")]
        [InlineData("suspended", "050.001")]
        [InlineData("fraud", "050.001")]
        [InlineData("cancelled", "050.002")]
        [InlineData("deceased", "050.002")]
        [InlineData("pending-cancellation", "050.002")]
        [InlineData("merged", "050.003")]
        public async Task HandleAsync_WhenMemberCannotAccrue_ReturnsApplicableNlaErrorCode(string accountNumber, string errorCode)
        {
            var handler = _serviceProvider.GetRequiredService<AutoAccrualInboundHandler>();

            var exception = await Assert.ThrowsAsync<AutoAccrualValidationException>(() =>
                handler.HandleAsync(MemberValidationPayload("TEST", "TEST", accountNumber), CancellationToken.None));

            Assert.Equal(errorCode, exception.ErrorCode);
        }

        [Fact]
        public async Task HandleAsync_WhenRequestIsValid_ReturnsCreatedTransaction()
        {
            var handler = _serviceProvider.GetRequiredService<AutoAccrualInboundHandler>();

            var transaction = await handler.HandleAsync(ValidPayload(), CancellationToken.None);

            Assert.Equal("transaction-id", transaction.TransactionId);
        }

        private static string Payload(string origin, string destination, string producer, string operatingAirline) =>
            JsonSerializer.Serialize(new
            {
                data = new
                {
                    correlationId = "correlation-id",
                    accrualRequest = new
                    {
                        producer,
                        flightDetails = new { operatingFlight = new { operatingCarrierCode = operatingAirline } },
                        flightSegment = new { origin, destination }
                    }
                }
            });

        private static string ValidPayload() => JsonSerializer.Serialize(new
        {
            data = new
            {
                correlationId = "11111111-1111-1111-1111-111111111111",
                accrualRequest = new
                {
                    producer = "VN",
                    passengerName = new { surName = "TEST", givenName = "TEST" },
                    ffpDetails = new { ffpAccountNumber = "789621323" },
                    flightDetails = new { marketingFlight = new { marketingCarrierCode = "VS", marketingFlightNumber = "7494" }, operatingFlight = new { operatingCarrierCode = "VN", operatingFlightNumber = "0316" } },
                    scheduledDepartureDate = "2026-10-01",
                    flightSegment = new { origin = "LHR", destination = "TLV" },
                    bookingDetails = new { cabinClassCode = "Y", operatingCarrierBookingClass = "L", marketingCarrierBookingClass = "L" },
                    documentDetails = new { documentNumber = "1234567890123", documentCouponNumber = 1, documentIssueDateTime = "2026-09-01T10:00:00Z" }
                }
            }
        });

        private static string MemberValidationPayload(string surname, string givenName, string accountNumber) => JsonSerializer.Serialize(new
        {
            data = new
            {
                correlationId = "correlation-id",
                accrualRequest = new
                {
                    producer = "VN",
                    passengerName = new { surName = surname, givenName },
                    ffpDetails = new { ffpAccountNumber = accountNumber },
                    flightDetails = new { operatingFlight = new { operatingCarrierCode = "VN" } },
                    flightSegment = new { origin = "LHR", destination = "TLV" }
                }
            }
        });

        private sealed class PassingReferenceValidationService : IAutoAccrualReferenceValidationService
        {
            public Task ValidateAsync(AutoAccrualRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        private sealed class FakeCustomerSearchDocumentServiceQueryHandler : CustomerSearchDocumentServiceQueryHandler
        {
            public FakeCustomerSearchDocumentServiceQueryHandler(IHttpConnector apiConnector, ILogger<CustomerSearchDocumentServiceQueryHandler> logger) : base(apiConnector, logger) { }

            public override Task<Result<Response<CustomerSearch>>> Handle(CustomerSearchDocumentServiceQuery request, CancellationToken cancellationToken)
            {
                if (request.MemberNumber == "http-not-found")
                {
                    throw new HttpRequestException("Member not found", null, HttpStatusCode.NotFound);
                }

                return Task.FromResult(request.MemberNumber == "unknown"
                    ? Result.Failure<Response<CustomerSearch>>("Member not found")
                    : Result.Success(new Response<CustomerSearch>
                    {
                        Status = Status.OK,
                        Value = new CustomerSearch
                        {
                            CustomerId = request.MemberNumber is "suspended" or "fraud" or "cancelled" or "deceased" or "pending-cancellation" or "merged"
                                ? $"{request.MemberNumber}-customer-id"
                                : "customer-id"
                        }
                    }));
            }
        }

        private class FakeCustomerDocumentQuery : ICustomerDocumentQuery
        {
            public Task<Result<Response<Customer>>> Handle(CustomerDocumentServiceQuery request, CancellationToken cancellationToken) => Task.FromResult(Result.Success(new Response<Customer>
            {
                Status = Status.OK,
                Value = new Customer
                {
                    CustomerId = request.CustomerId,
                    PNRName = "TEST/TEST",
                    Status = request.CustomerId switch
                    {
                        "suspended-customer-id" => "Suspended",
                        "fraud-customer-id" => "Fraud",
                        "cancelled-customer-id" => "Cancelled",
                        "deceased-customer-id" => "Deceased",
                        "pending-cancellation-customer-id" => "Pending Cancellation",
                        "merged-customer-id" => "Merged",
                        _ => "Active"
                    }
                }
            }));
        }

        private sealed class FakeFlyingClubMemberDocumentServiceQueryHandler : FlyingClubMemberDocumentServiceQueryHandler
        {
            public FakeFlyingClubMemberDocumentServiceQueryHandler(IHttpConnector apiConnector, ILogger<FlyingClubMemberDocumentServiceQueryHandler> logger) : base(apiConnector, logger) { }

            public override Task<Result<Response<FlyingClubMember>>> Handle(FlyingClubMemberDocumentServiceQuery request, CancellationToken cancellationToken) =>
                Task.FromResult(Result.Success(new Response<FlyingClubMember>
                {
                    Status = Status.OK,
                    Value = new FlyingClubMember { CustomerId = "customer-id", MemberNumber = "1074850130", TierCode = "Red", MemberType = "FlyingClub", Status = "Active" }
                }));
        }

        private sealed class FakeCommonRefDbServices : ICommonRefDBServices
        {
            public Task<Result<Response<MemberAccountReferenceData>>> GetMemberAccountReferenceData() => throw new NotSupportedException();
            public Task<Result<Response<PartnersResponse>>> GetPartnersReferenceData() => throw new NotSupportedException();
            public Task<Domain.Entities.BirthdayBonus.Product> GetProductReferenceDataByName(string productName) => throw new NotSupportedException();
            public BookingDetailsResult ProcessBookingClassData(PreValidationPartnerData partnerBookingData, string bookingClass, string flownClass, DateTime activityDate, string listFareValue, string listTicketValue, string listPaxValue) => throw new NotSupportedException();
            public Task<int> GetAutoAccrualValidationCount(string procedure, AutoAccrualRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<AutoAccrualPartnerTerms> GetAutoAccrualPartnerTerms(string partnerCode, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<Infrastructure.Models.Distance> GetDistanceOfRouteForInRuleAsync(string origin, string destination, CancellationToken cancellationToken) => Task.FromResult(new Infrastructure.Models.Distance { OriginCountryCode = "GB", DestinationCountryCode = "IL", DistanceInMiles = 2210 });
        }

        private sealed class FakeRulesEngineService : IRulesEngineService
        {
            public Task<TResponse> ExecuteAsync<TRequest, TResponse>(string operation, HttpMethod method, TRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<TResponse> ExecuteAsync<TResponse>(string operation, HttpMethod method, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<EarnAirlineAccrualCalculatorResponse> EarnAirlineAccrualCalculatorAsync(EarnAirlineAccrualCalculatorRequest request, CancellationToken cancellationToken) => Task.FromResult(new EarnAirlineAccrualCalculatorResponse
            {
                SegmentResponses = [new SegmentResponse { PointsAward = 3250, BasePoints = 2500, CabinBonusPoints = 500, TierBonusPoints = 250, TierPointsAward = 25 }]
            });
        }

        private sealed class FakeCustomerTransactionService : ICustomerTransactionService
        {
            public Task<TResponse> ExecuteAsync<TRequest, TResponse>(string operation, HttpMethod method, TRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<TResponse> ExecuteAsync<TResponse>(string operation, HttpMethod method, CancellationToken cancellationToken) => throw new NotSupportedException();
            public Task<TransactionCommandStatusResponse> GetTransactionStatusAsync(string requestId, CancellationToken cancellationToken) => Task.FromResult(new TransactionCommandStatusResponse
            {
                CommandStatus = Status.OK,
                Transaction = new TransactionDocument
                {
                    TransactionId = "transaction-id",
                    CreditDetails = [
                        new Creditdetail { CreditValue = "2500", PointSubType = Constants.Application.BaseMilesPointSubType },
                        new Creditdetail { CreditValue = "500", PointSubType = Constants.Application.CabinBonusPointSubType },
                        new Creditdetail { CreditValue = "250", PointSubType = Constants.Application.TierBonusPointSubType }
                    ]
                }
            });
        }

        private sealed class FakeCachedPartners : ICachedPartners
        {
            public Task<Result<Response<PartnersResponse>>> GetCachedPartnersReferenceData() => Task.FromResult(Result.Success(new Response<PartnersResponse>
            {
                Status = Status.OK,
                Value = new PartnersResponse { Partners = [new Domain.Entities.PartnersRetrieval.PartnerProfile { PartnerId = "partner-id", PartnerCode = "VN", Name = "Vietnam Airlines", PartnerType = "Airline", PartnerSubtype = "Partner", Active = true }] }
            }));
        }

        private sealed class FakeCachedFlightProduct : ICachedFlightProduct
        {
            public Task<Domain.Entities.BirthdayBonus.Product> GetFlightProductAsync() => Task.FromResult(new Domain.Entities.BirthdayBonus.Product { ProductId = "product-id", ProductName = "Flight" });
        }

        private sealed class FakeTransactionStatementTextService : ITxnStatementTextService
        {
            public Task<StatementTextResponse> GetTransactionStatementText(TransactionStatementTextQuery request) => Task.FromResult(new StatementTextResponse { StatementText = "VAA/Vietnam Airlines, LHR - TLV, Y" });
        }

    }
}
