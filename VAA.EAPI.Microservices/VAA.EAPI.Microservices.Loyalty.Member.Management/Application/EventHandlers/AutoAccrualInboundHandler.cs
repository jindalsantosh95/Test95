using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.CustomerTransactionMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.RuleEngineMs;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers
{
    public class AutoAccrualInboundHandler : IAutoAccrualInboundHandler
    {
        private readonly ILogger<AutoAccrualInboundHandler> _logger;
        private readonly CustomerSearchDocumentServiceQueryHandler _customerSearchDocumentServiceQueryHandler;
        private readonly ICustomerDocumentQuery _customerDocumentQuery;
        private readonly FlyingClubMemberDocumentServiceQueryHandler _flyingClubMemberDocumentServiceQueryHandler;
        private readonly IAutoAccrualReferenceValidationService _referenceValidationService;
        private readonly ICommonRefDBServices _commonRefDbServices;
        private readonly IRulesEngineService _rulesEngineService;
        private readonly ICustomerTransactionService _customerTransactionService;
        private readonly ICachedPartners _cachedPartners;
        private readonly ICachedFlightProduct _cachedFlightProduct;
        private readonly TransactionStatementTextQueryHandler _transactionStatementTextQueryHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public AutoAccrualInboundHandler(ILogger<AutoAccrualInboundHandler> logger, CustomerSearchDocumentServiceQueryHandler customerSearchDocumentServiceQueryHandler, ICustomerDocumentQuery customerDocumentQuery, FlyingClubMemberDocumentServiceQueryHandler flyingClubMemberDocumentServiceQueryHandler, IAutoAccrualReferenceValidationService referenceValidationService, ICommonRefDBServices commonRefDbServices, IRulesEngineService rulesEngineService, ICustomerTransactionService customerTransactionService, ICachedPartners cachedPartners, ICachedFlightProduct cachedFlightProduct, TransactionStatementTextQueryHandler transactionStatementTextQueryHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerSearchDocumentServiceQueryHandler = customerSearchDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(customerSearchDocumentServiceQueryHandler));
            _customerDocumentQuery = customerDocumentQuery ?? throw new ArgumentNullException(nameof(customerDocumentQuery));
            _flyingClubMemberDocumentServiceQueryHandler = flyingClubMemberDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(flyingClubMemberDocumentServiceQueryHandler));
            _referenceValidationService = referenceValidationService ?? throw new ArgumentNullException(nameof(referenceValidationService));
            _commonRefDbServices = commonRefDbServices ?? throw new ArgumentNullException(nameof(commonRefDbServices));
            _rulesEngineService = rulesEngineService ?? throw new ArgumentNullException(nameof(rulesEngineService));
            _customerTransactionService = customerTransactionService ?? throw new ArgumentNullException(nameof(customerTransactionService));
            _cachedPartners = cachedPartners ?? throw new ArgumentNullException(nameof(cachedPartners));
            _cachedFlightProduct = cachedFlightProduct ?? throw new ArgumentNullException(nameof(cachedFlightProduct));
            _transactionStatementTextQueryHandler = transactionStatementTextQueryHandler ?? throw new ArgumentNullException(nameof(transactionStatementTextQueryHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        }

        public async Task<TransactionDocument> HandleAsync(string eventRequestPayload, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventRequestPayload);
            cancellationToken.ThrowIfCancellationRequested();

            var inboundEvent = Json.Deserialize<AutoAccrualInboundEvent>(eventRequestPayload);
            var request = inboundEvent?.Data?.AccrualRequest;

            if (request is null || string.IsNullOrWhiteSpace(inboundEvent.Data.CorrelationId))
            {
                throw new AutoAccrualValidationException("Auto accrual message is missing correlationId or accrualRequest.");
            }

            try
            {
                ValidateOriginAndDestination(request);
                ValidateOperatingAirlineCode(request);
                var customer = await GetCustomerAsync(request.FfpDetails.FfpAccountNumber, cancellationToken);
                ValidatePnrName(request, customer.PNRName);
                ValidateMemberStatus(customer.Status);
                await _referenceValidationService.ValidateAsync(request, cancellationToken);

                var flyingClubMember = await GetFlyingClubMemberAsync(customer.CustomerId, cancellationToken);

                //Fetch distance for the route from common reference database and pass it to rules engine for accrual calculation.
                var distance = await _commonRefDbServices.GetDistanceOfRouteForInRuleAsync(request.FlightSegment.Origin, request.FlightSegment.Destination, cancellationToken)
                    ?? throw new AutoAccrualValidationException("Route distance could not be retrieved for Auto Accrual.");

                //fetch the points and miles calculation from rules engine for the given flight segment and member details.
                var calculation = await _rulesEngineService.EarnAirlineAccrualCalculatorAsync((EarnAirlineAccrualCalculatorRequest)(request, flyingClubMember, distance), cancellationToken);
                var segment = calculation.SegmentResponses?.FirstOrDefault()
                    ?? throw new AutoAccrualValidationException("Rules Engine did not return a segment calculation for Auto Accrual.");

                //fetch the product and partner reference data
                var product = await _cachedFlightProduct.GetFlightProductAsync()
                    ?? throw new AutoAccrualValidationException("Flight product reference data could not be retrieved for Auto Accrual.");
                var partners = await _cachedPartners.GetCachedPartnersReferenceData();
                var partner = partners.IsSuccess ? partners.Value.Value?.Partners?.FirstOrDefault(candidate => candidate.Active && candidate.PartnerCode.Is(request.FlightDetails?.OperatingFlight?.OperatingCarrierCode)) : null;

                if (partner is null)
                {
                    throw new AutoAccrualValidationException("Operating airline partner reference data could not be retrieved for Auto Accrual.");
                }

                // Create the transaction document and publish the transaction create event to Customer Transaction MS.
                var transactionData = (TransactionCreateEventData)(customer, flyingClubMember, request, segment, product, partner, distance.DistanceInMiles);
                transactionData.TransactionDocument.StatementText = await GetStatementTextAsync(transactionData, flyingClubMember, partner, cancellationToken);

                var transactionEvent = new CloudEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.TryParse(inboundEvent.Data.CorrelationId, out var correlationId) ? correlationId : Guid.Empty,
                    Data = transactionData,
                    Type = EnumOperations.GetEnumDescription(LoyaltyMemberEventType.TransactionCreateActionExecution),
                    Source = VAA.EAPI.Common.Helpers.ConfigReader.Get(Constants.Environment.CustomerTransactionTopicName),
                    Subject = EnumOperations.GetEnumDescription(LoyaltyMemberEventType.TransactionCreateActionExecution),
                    Time = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    Specversion = Constants.Application.SpecVersion
                };

                await PublishEventAsync(transactionEvent, cancellationToken);
                var transactionResponse = await _customerTransactionService.GetTransactionStatusAsync(transactionEvent.Id, cancellationToken);

                if (transactionResponse.CommandStatus != Status.OK || transactionResponse.Transaction is null)
                {
                    throw new AutoAccrualValidationException("Customer Transaction MS did not create the Auto Accrual transaction.");
                }

                // Publish the confirmed transaction to VRed for accrual processing.
                var vredTransactionEvent = new CloudEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    CorrelationId = Guid.TryParse(inboundEvent.Data.CorrelationId, out correlationId) ? correlationId : Guid.Empty,
                    Data = (VredCreateTransactionEventData)(transactionData.TransactionDocument, partner, product),
                    Type = EnumOperations.GetEnumDescription(LoyaltyMemberEventType.VredCreateTransactionEvent),
                    Source = VAA.EAPI.Common.Helpers.ConfigReader.Get(Constants.Environment.VredServiceBusTopicName),
                    Subject = EnumOperations.GetEnumDescription(LoyaltyMemberEventType.VredCreateTransactionEvent),
                    Time = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    Specversion = Constants.Application.SpecVersion
                };

                await PublishEventAsync(vredTransactionEvent, cancellationToken);

                return transactionResponse.Transaction;
            }
            catch (AutoAccrualValidationException ex)
            {
                throw new AutoAccrualValidationException(
                    $"CorrelationId: {inboundEvent.Data.CorrelationId}. {ex.Message}", ex.ErrorCode);
            }
        }

        private async Task<Customer> GetCustomerAsync(string memberNumber, CancellationToken cancellationToken)
        {
            Result<Response<CustomerSearch>> member;
            try
            {
                member = await _customerSearchDocumentServiceQueryHandler.Handle(
                    new CustomerSearchDocumentServiceQuery { MemberNumber = memberNumber },
                    cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AutoAccrualValidationException("Member could not be retrieved for Auto Accrual validation.", Constants.Application.AutoAccrualFfpAccountNotFound);
            }

            var customerId = member.IsSuccess ? member.Value.Value?.CustomerId : null;
            if (string.IsNullOrWhiteSpace(customerId))
                throw new AutoAccrualValidationException("Member could not be retrieved for Auto Accrual validation.", Constants.Application.AutoAccrualFfpAccountNotFound);

            var response = await _customerDocumentQuery.Handle(new CustomerDocumentServiceQuery { CustomerId = customerId }, cancellationToken);
            return response.IsSuccess && response.Value.Value is not null
                ? response.Value.Value
                : throw new AutoAccrualValidationException("Member could not be retrieved for Auto Accrual validation.", Constants.Application.AutoAccrualFfpAccountNotFound);
        }

        private async Task<FlyingClubMember> GetFlyingClubMemberAsync(string customerId, CancellationToken cancellationToken)
        {
            var response = await _flyingClubMemberDocumentServiceQueryHandler.Handle(
                new FlyingClubMemberDocumentServiceQuery { CustomerId = customerId },
                cancellationToken);

            return response.IsSuccess && response.Value.Value is not null
                ? response.Value.Value
                : throw new InvalidOperationException("Flying Club member could not be retrieved for Auto Accrual calculation.");
        }

        private async Task<string> GetStatementTextAsync(TransactionCreateEventData transactionData, FlyingClubMember flyingClubMember, PartnerProfile partner, CancellationToken cancellationToken)
        {
            var statementTextRequest = (StatementTextRequest)(transactionData, flyingClubMember, partner);
            var response = await _transactionStatementTextQueryHandler.Handle(new TransactionStatementTextQuery { StatementDescriptionRequest = statementTextRequest }, cancellationToken);

            if (!response.IsSuccess || response.Value.Value is null)
            {
                throw new AutoAccrualValidationException("Statement text could not be retrieved for Auto Accrual.");
            }

            return response.Value.Value.StatementText;
        }

        private async Task PublishEventAsync(VAA.EAPI.Domain.Events.CloudEvent transactionEvent, CancellationToken cancellationToken)
        {
            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand
            {
                Id = transactionEvent.Id,
                Type = transactionEvent.Type,
                Data = transactionEvent.Data,
                Subject = transactionEvent.Subject
            }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = transactionEvent,
                    ServiceBusConnectionString = VAA.EAPI.Common.Helpers.ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = transactionEvent.Source
                }, cancellationToken);
            }
            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand
                {
                    Id = transactionEvent.Id,
                    Type = transactionEvent.Type,
                    Data = transactionEvent.Data,
                    Subject = transactionEvent.Subject
                }, cancellationToken);
                throw;
            }
        }

        private static void ValidateOriginAndDestination(AutoAccrualRequest request)
        {
            if (request.FlightSegment!.Origin!.Equals(request.FlightSegment.Destination, StringComparison.OrdinalIgnoreCase))
            {
                throw new AutoAccrualValidationException("Origin and destination cannot be the same.");
            }
        }

        private static void ValidateOperatingAirlineCode(AutoAccrualRequest request)
        {
            var producer = request.Producer!;
            var operatingAirline = request.FlightDetails!.OperatingFlight!.OperatingCarrierCode!;
            var isValid = producer.Equals(Constants.Application.MUAirlineCode, StringComparison.OrdinalIgnoreCase)
                ? operatingAirline.Equals(Constants.Application.MUAirlineCode, StringComparison.OrdinalIgnoreCase) || operatingAirline.Equals(Constants.Application.FMAirlineCode, StringComparison.OrdinalIgnoreCase)
                : operatingAirline.Equals(producer, StringComparison.OrdinalIgnoreCase);

            if (!isValid)
                throw new AutoAccrualValidationException("Operating airline is not valid", Constants.Application.AutoAccrualInvalidAirlineCode);
        }

        private static void ValidatePnrName(AutoAccrualRequest request, string pnrName)
        {
            var profileName = pnrName?.Split('/', 2);
            var surname = Normalise(request.PassengerName!.SurName!);
            var givenName = Normalise(request.PassengerName!.GivenName!);

            if (profileName is not { Length: 2 } ||
                !Normalise(profileName[0]).Equals(surname, StringComparison.OrdinalIgnoreCase))
            {
                throw new AutoAccrualValidationException("Passenger surname does not match the member PNR name.", Constants.Application.AutoAccrualInvalidSurname);
            }

            if (givenName.Length < 3 ||
                !Normalise(profileName[1]).StartsWith(givenName[..3], StringComparison.OrdinalIgnoreCase))
            {
                throw new AutoAccrualValidationException("Passenger given name does not match the member PNR name.", Constants.Application.AutoAccrualInvalidGivenName);
            }
        }

        private static void ValidateMemberStatus(string status)
        {
            if (Constants.Application.MemberStatusesForAccrual.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            var errorCode = status switch
            {
                var value when value.Is("Suspended") || value.Is("Fraud") => Constants.Application.AutoAccrualFfpAccountSuspended,
                var value when value.Is("Cancelled") || value.Is("Deceased") || value.Is("Pending Cancellation") => Constants.Application.AutoAccrualFfpAccountClosed,
                var value when value.Is("Merged") => Constants.Application.AutoAccrualFfpAccountMerged,
                _ => throw new InvalidOperationException($"Unsupported member status '{status}' returned for Auto Accrual validation.")
            };

            throw new AutoAccrualValidationException("Member status does not allow accrual.", errorCode);
        }

        private static string Normalise(string value) => new string(value.Where(char.IsLetter).ToArray());
    }
}

