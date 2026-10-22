using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType(OperationTypeNames.Mutation)]
    public class RetroClaimMutation
    {
        private readonly ILogger<RetroClaimMutation> _logger;
        private readonly ICustomerProvider _customerProvider;
        private readonly IPartnerDataManagementQueryProvider _partnerDataManagementProvider;
        private readonly IReferenceDataMutationProvider _referenceDataProvider;
        private readonly IInRuleMutationProvider _inRuleProvider;
        private readonly IProductsQueryProvider _productsProvider;
        private readonly IPartnersProfilesQueryProvider _partnerProfilesProvider;
        private readonly ICustomerTransactionMutationProvider<TransactionResponse> _transactionProvider;
        private readonly IVredMutationProvider _vredProvider;
        private readonly ICustomerTransactionQueryProvider _customerTransactionQueryProvider;
        private readonly ILoyaltyMemberMgtProvider _mbrMgmtProvider;

        public RetroClaimMutation(
            ILogger<RetroClaimMutation> logger,
            ICustomerProvider customerProvider,
            IPartnerDataManagementQueryProvider partnerDataManagementProvider,
            IReferenceDataMutationProvider referenceDataProvider,
            IInRuleMutationProvider inRuleProvider,
            IProductsQueryProvider productsProvider,
            IPartnersProfilesQueryProvider partnerProfilesProvider,
            ICustomerTransactionMutationProvider<TransactionResponse> transactionProvider,
            IVredMutationProvider vredProvider,
            ICustomerTransactionQueryProvider customerTransactionQueryProvider,
            ILoyaltyMemberMgtProvider mbrMgmtProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _partnerDataManagementProvider = partnerDataManagementProvider ?? throw new ArgumentNullException(nameof(partnerDataManagementProvider));
            _referenceDataProvider = referenceDataProvider ?? throw new ArgumentNullException(nameof(referenceDataProvider));
            _inRuleProvider = inRuleProvider ?? throw new ArgumentNullException(nameof(inRuleProvider));
            _productsProvider = productsProvider ?? throw new ArgumentNullException(nameof(productsProvider));
            _partnerProfilesProvider = partnerProfilesProvider ?? throw new ArgumentNullException(nameof(partnerProfilesProvider));
            _transactionProvider = transactionProvider ?? throw new ArgumentNullException(nameof(transactionProvider));
            _vredProvider = vredProvider ?? throw new ArgumentNullException(nameof(vredProvider));
            _customerTransactionQueryProvider = customerTransactionQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _mbrMgmtProvider = mbrMgmtProvider ?? throw new ArgumentNullException(nameof(mbrMgmtProvider));
        }

        [Authorize(Roles = ["Loyalty.VSRetroClaim.Create"])]
        [GraphQLName("loyaltyVSRetroClaim")]
        [GraphQLDescription("Create VS retro transactions for eligible flown activity.")]
        public async Task<VSRetroClaimResponse> VSRetroClaim(VSRetroClaimRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IEnumerable<ProductDetails> productDetails = null;
            IEnumerable<PartnerProfileDetails> partnerProfileDetails = null;
            AirportDistanceAndCountries airportDetails = null;

            var validationResult = new VSRetroClaimValidator().Validate(request);
            if (!validationResult.IsValid)
            {
                throw new RequestValidationException("VS retro claim mutation mandatory input is missing.", validationResult.Errors);
            }

            var customerResponse = request.CustomerId.IsNullOrWhiteSpace() ? await _customerProvider.GetCustomer(null, request.MemberNumber) : await _customerProvider.GetCustomer(request.CustomerId, null);

            if (customerResponse?.Customer == null)
            {
                return CreateResponse(new[]
                {
                    VSRetroClaimMapper.MapRequest(request, "Flying Club member was not found.")
                });
            }

            //member joined date validation[override scenario]
            var flyingClubMemberDoc = await _customerTransactionQueryProvider.GetFlyingClubMember(customerResponse?.Customer.CustomerId);

            if (request.DepartureDate <= flyingClubMemberDoc?.FlyingClubMemberDocuments?.FirstOrDefault()?.EnrollmentDate?.AddMonths(-6).Date)
            {
                return CreateResponse(new[]
                {
                    VSRetroClaimMapper.MapRequest(request, "This flight can't be credited because it took place over 6 months before the member joined Flying Club.")
                });
            }

            if (!request.FareBasis.IsNullOrWhiteSpace() ||
            !request.TicketDesignator.IsNullOrWhiteSpace() ||
            !request.PassengerType.IsNullOrWhiteSpace())
            {
                var pdmValidationResponse = await _partnerDataManagementProvider.ValidateVSClaimInputs(new ValidateVSClaimInputsRequest
                {
                    FareBasis = !request.FareBasis.IsNullOrWhiteSpace() ? request.FareBasis : null,
                    TicketDesignator = !request.TicketDesignator.IsNullOrWhiteSpace() ? request.TicketDesignator : null,
                    PassengerType = !request.PassengerType.IsNullOrWhiteSpace() ? request.PassengerType : null
                });

                if (pdmValidationResponse?.Success != true)
                {
                    var rejectionReason = pdmValidationResponse?.ErrorMessage.IsNullOrWhiteSpace() == false
                        ? pdmValidationResponse.ErrorMessage
                        : "VS retro claim input validation could not be completed.";

                    return CreateResponse(new[]
                    {
                        VSRetroClaimMapper.MapRequest(request, rejectionReason)
                    });
                }
            }

            var productDetailsTask = _productsProvider.GetProductDetails();
            var partnerProfileDetailsTask = _partnerProfilesProvider.GetPartnerProfileDetails();
            var airportDetailsTask = _referenceDataProvider.GetSegmentDistanceAndCountry(request.DepartureAirport, request.ArrivalAirport);

            await Task.WhenAll(productDetailsTask, partnerProfileDetailsTask, airportDetailsTask);

            productDetails = await productDetailsTask;
            partnerProfileDetails = await partnerProfileDetailsTask;
            airportDetails = await airportDetailsTask;

            var product = productDetails?.FirstOrDefault(item => item.ProductCategory.Is(Common.Constants.Application.ProductCategory));
            var partner = partnerProfileDetails?.FirstOrDefault(item => item.PartnerCode.Is(Common.Constants.Application.VSLoyaltyProgrammeCode));

            if (product == null || partner == null)
                throw new RequestValidationException("Either 'Partner' or 'Product' does not exist in the system");

            if (airportDetails == null || airportDetails.DistanceInMiles.IsNullOrWhiteSpace())
            {
                throw new RequestValidationException("Bookable Activity not found");
            }

            var pointsResponse = await _inRuleProvider.CalculatePoints(VSRetroClaimMapper.MapToPointsRequest(request, customerResponse, airportDetails));

            if (pointsResponse?.Errors?.Any() == true)
            {
                return CreateResponse(new[]
                { VSRetroClaimMapper.MapFlight(request, reason: "Points calculation was rejected.") });
            }

            if (!int.TryParse(pointsResponse?.SegmentResponses?.FirstOrDefault()?.PointsAward, out var pointsAward) || pointsAward <= 0)
                throw new RequestValidationException(Constants.Application.TxnNotQualifiedForPoints);

            //need to check for dupe check
            var transactionRequest = VSRetroClaimMapper.MapToTransactionRequest(request, customerResponse, pointsResponse?.SegmentResponses?.FirstOrDefault(), airportDetails, partner, product);

            transactionRequest.TransactionDocument.StatementText = (await _mbrMgmtProvider.GenerateStatetmentText(TransactionMapper.MapToTransactionStatementRequest(transactionRequest.TransactionDocument, partner, product, null, customerResponse.Customer))).StatementText;

            var transactionResponse = await _transactionProvider.CreateTransaction(transactionRequest);

            if (transactionResponse?.Errors?.Any() == true || transactionResponse?.CommandStatus != Status.OK || transactionResponse.Transaction?.TransactionId.IsNullOrWhiteSpace() == true)
            {
                return CreateResponse(new[]
                { VSRetroClaimMapper.MapFlight(request, pointsResponse?.SegmentResponses?.FirstOrDefault(), reason: "Unable to create the retro transaction.") });
            }

            var result = VSRetroClaimMapper.MapFlight(request, pointsResponse?.SegmentResponses?.FirstOrDefault(), accepted: true);

            var vredResponse = await _vredProvider.PostTransactionToVred(TransactionMapper.MapToVredTransactionRequest(
                new CreateTransactionMutationRequest(), transactionResponse.Transaction, partner, product, null, product));

            if (vredResponse == null || vredResponse.Status != Status.OK || vredResponse.Errors?.Any() == true)
            {
                result.Accepted = false;
                result.PointsEarned = 0;
                result.TierPointsEarned = 0;
                result.Reason = "The retro transaction was created but could not be posted to VRed.";
                return CreateResponse(new[] { result });
            }

            await UpdateFlownRecord(request);


            return CreateResponse(new[] { result });
        }

        private async Task UpdateFlownRecord(VSRetroClaimRequest request)
        {
            try
            {
                var updateResult = await _partnerDataManagementProvider.UpdateVSRetroFlownStatus(new TicketStatusRequest
                {
                    FlownRecordId = request.Id.ToString(),
                    TicketNumber = request.TicketNumber,
                    TicketStatus = "Allocated",
                    MemberNumber = request.MemberNumber,
                    LoyaltyProgrammeCode = Common.Constants.Application.VSLoyaltyProgrammeCode,
                    PostingTypeCode = Constants.Application.PostingTypeCode
                });

                if (!string.Equals(updateResult, "Ticket Number Updated successfully", StringComparison.Ordinal))
                {
                    _logger.LogWarning("VS retro transaction was created but flown record {FlownRecordId} was not acknowledged as allocated", request.Id);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "VS retro transaction was created but flown record {FlownRecordId} could not be allocated", request.Id);
            }
        }

        private VSRetroClaimResponse CreateResponse(IEnumerable<VSClaimDetails> flights)
        {
            var flightList = flights.ToList();
            return new VSRetroClaimResponse
            {
                Flights = flightList,
                TotalPointsEarned = flightList.Where(flight => flight.Accepted).Sum(flight => flight.PointsEarned),
                TotalTierPointsEarned = flightList.Where(flight => flight.Accepted).Sum(flight => flight.TierPointsEarned)
            };
        }
    }
}
