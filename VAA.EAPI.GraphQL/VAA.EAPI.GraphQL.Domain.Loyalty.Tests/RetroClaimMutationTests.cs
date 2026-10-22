using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.TxnStatementText;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests;

public sealed class RetroClaimMutationTests
{
    [Fact]
    public async Task VSRetroClaim_CreatesAndPostsTransaction_ForEligibleFlownFlight()
    {
        var dependencies = new TestDependencies();
        var request = CreateRequest();

        var response = await dependencies.CreateMutation().VSRetroClaim(request);

        var flight = Assert.Single(response.Flights);
        Assert.True(flight.Accepted);
        Assert.Equal(2_000, flight.PointsEarned);
        Assert.Equal(250, flight.TierPointsEarned);
        Assert.Equal(2_000, response.TotalPointsEarned);
        Assert.Equal(250, response.TotalTierPointsEarned);
        Assert.NotNull(dependencies.TransactionProvider.CreatedTransaction);
        Assert.Null(dependencies.CustomerProvider.LastCustomerId);
        Assert.Equal(request.MemberNumber, dependencies.CustomerProvider.LastMemberNumber);
        Assert.Equal(1, dependencies.CustomerTransactionQueryProvider.GetFlyingClubMemberCalls);
        Assert.Equal("customer-1", dependencies.CustomerTransactionQueryProvider.LastCustomerId);
        Assert.Equal(request.DepartureAirport, dependencies.ReferenceProvider.LastOriginAirport);
        Assert.Equal(request.ArrivalAirport, dependencies.ReferenceProvider.LastDestinationAirport);
        Assert.Equal(request.OperatingAirlineCode, dependencies.InRuleProvider.LastRequest.FlightSegments.Single().OperatingCarrier);
        Assert.Equal("MI", dependencies.TransactionProvider.CreatedTransaction.TransactionDocument.FlightDetails.VAPostingTypeCode);
        Assert.Equal("VS retro flight statement", dependencies.TransactionProvider.CreatedTransaction.TransactionDocument.StatementText);
        Assert.Equal(1, dependencies.MemberMgtProvider.GenerateStatementTextCalls);
        Assert.True(dependencies.VredProvider.PostCalled);
        Assert.Equal(0, dependencies.PdmProvider.ValidateInputsCalls);
        Assert.Equal(1, dependencies.PdmProvider.UpdateStatusCalls);
        Assert.Equal(request.Id.ToString(), dependencies.PdmProvider.StatusRequest.FlownRecordId);
        Assert.Equal(VAA.EAPI.GraphQL.Common.Constants.Application.VSLoyaltyProgrammeCode, dependencies.PdmProvider.StatusRequest.LoyaltyProgrammeCode);
        Assert.Equal("Allocated", dependencies.PdmProvider.StatusRequest.TicketStatus);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsRejectedFlight_WhenRulesRejectTheClaim()
    {
        var dependencies = new TestDependencies
        {
            InRuleProvider = new InRuleProvider
            {
                Response = new CreditPointsResponse
                {
                    Errors = new[] { new ErrorDetails { Code = "R001", Message = "Invalid rule" } }
                }
            }
        };

        var response = await dependencies.CreateMutation().VSRetroClaim(CreateRequest());

        var flight = Assert.Single(response.Flights);
        Assert.False(flight.Accepted);
        Assert.Equal("Points calculation was rejected.", flight.Reason);
        Assert.Null(dependencies.TransactionProvider.CreatedTransaction);
        Assert.False(dependencies.VredProvider.PostCalled);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsRejectedFlight_WhenMemberDoesNotExist()
    {
        var dependencies = new TestDependencies();
        dependencies.CustomerProvider.Response = new CustomerDTO();

        var response = await dependencies.CreateMutation().VSRetroClaim(CreateRequest());

        var flight = Assert.Single(response.Flights);
        Assert.False(flight.Accepted);
        Assert.Equal("Flying Club member was not found.", flight.Reason);
        Assert.Null(dependencies.ReferenceProvider.LastOriginAirport);
        Assert.Null(dependencies.TransactionProvider.CreatedTransaction);
        Assert.Equal(0, dependencies.CustomerTransactionQueryProvider.GetFlyingClubMemberCalls);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsRejectedFlight_WhenFlightPredatesEligibleMemberEnrollmentPeriod()
    {
        var dependencies = new TestDependencies();
        var request = CreateRequest();
        dependencies.CustomerTransactionQueryProvider.FlyingClubMemberResponse = new FlyingClubMemberAPIResponse
        {
            FlyingClubMemberDocuments =
            [
                new FlyingClubMember { EnrollmentDate = request.DepartureDate.AddMonths(6) }
            ]
        };

        var response = await dependencies.CreateMutation().VSRetroClaim(request);

        var flight = Assert.Single(response.Flights);
        Assert.False(flight.Accepted);
        Assert.Equal("This flight can't be credited because it took place over 6 months before the member joined Flying Club.", flight.Reason);
        Assert.Null(dependencies.ReferenceProvider.LastOriginAirport);
        Assert.Null(dependencies.TransactionProvider.CreatedTransaction);
    }

    [Fact]
    public async Task VSRetroClaim_Throws_WhenRouteIsNotBookable()
    {
        var dependencies = new TestDependencies();
        dependencies.ReferenceProvider.Route = new AirportDistanceAndCountries();

        await Assert.ThrowsAsync<RequestValidationException>(() => dependencies.CreateMutation().VSRetroClaim(CreateRequest()));

        Assert.Null(dependencies.InRuleProvider.LastRequest);
        Assert.Null(dependencies.TransactionProvider.CreatedTransaction);
    }

    [Theory]
    [InlineData("ArrivalAirport")]
    [InlineData("MemberNumber")]
    [InlineData("OperatingAirlineCode")]
    [InlineData("OperatingFlightNumber")]
    [InlineData("DepartureAirport")]
    [InlineData("FlownCabinClass")]
    [InlineData("Id")]
    [InlineData("DepartureDate")]
    [InlineData("InitiatedBy")]
    [InlineData("IataAgentNumber")]
    public async Task VSRetroClaim_ReturnsRejectedFlight_WhenRequiredInputIsMissing(string field)
    {
        var dependencies = new TestDependencies();
        var request = CreateRequest();
        SetMissingRequiredField(request, field);

        await Assert.ThrowsAsync<RequestValidationException>(() => dependencies.CreateMutation().VSRetroClaim(request));
        Assert.Equal(0, dependencies.CustomerProvider.GetCustomerCalls);
        Assert.Equal(0, dependencies.PdmProvider.UpdateStatusCalls);
    }

    [Fact]
    public async Task VSRetroClaim_Throws_WhenRequestIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new TestDependencies().CreateMutation().VSRetroClaim(null));
    }

    [Theory]
    [InlineData("FareBasis")]
    [InlineData("TicketDesignator")]
    [InlineData("PassengerType")]
    public async Task VSRetroClaim_ValidatesPdmInputs_WhenAnyPdmValidationInputIsProvided(string inputName)
    {
        var dependencies = new TestDependencies();
        var request = CreateRequest();

        switch (inputName)
        {
            case "FareBasis": request.FareBasis = "Y26"; break;
            case "TicketDesignator": request.TicketDesignator = "ITF"; break;
            case "PassengerType": request.PassengerType = "ITF"; break;
            default: throw new ArgumentOutOfRangeException(nameof(inputName));
        }

        await dependencies.CreateMutation().VSRetroClaim(request);

        Assert.Equal(1, dependencies.PdmProvider.ValidateInputsCalls);
        Assert.Equal(request.FareBasis, dependencies.PdmProvider.ValidationRequest.FareBasis);
        Assert.Equal(request.TicketDesignator, dependencies.PdmProvider.ValidationRequest.TicketDesignator);
        Assert.Equal(request.PassengerType, dependencies.PdmProvider.ValidationRequest.PassengerType);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsRejectedFlight_WhenPdmValidationReturnsAnError()
    {
        var dependencies = new TestDependencies();
        dependencies.PdmProvider.ValidationResponse = new ValidateVSClaimInputsResponse
        {
            Success = false,
            ErrorMessage = "Fare basis is not eligible for a retro claim."
        };
        var request = CreateRequest();
        request.PassengerType = "ITF";

        var response = await dependencies.CreateMutation().VSRetroClaim(request);

        var flight = Assert.Single(response.Flights);
        Assert.False(flight.Accepted);
        Assert.Equal("Fare basis is not eligible for a retro claim.", flight.Reason);
        Assert.Equal(1, dependencies.PdmProvider.ValidateInputsCalls);
        Assert.Null(dependencies.InRuleProvider.LastRequest);
        Assert.Null(dependencies.TransactionProvider.CreatedTransaction);
        Assert.False(dependencies.VredProvider.PostCalled);
        Assert.Equal(0, dependencies.PdmProvider.UpdateStatusCalls);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsFallbackRejection_WhenPdmValidationIsUnsuccessfulWithoutAnErrorMessage()
    {
        var dependencies = new TestDependencies();
        dependencies.PdmProvider.ValidationResponse = new ValidateVSClaimInputsResponse { Success = false };
        var request = CreateRequest();
        request.FareBasis = "Y26";

        var response = await dependencies.CreateMutation().VSRetroClaim(request);

        AssertPdmValidationFailure(response, dependencies);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsFallbackRejection_WhenPdmValidationResponseIsNull()
    {
        var dependencies = new TestDependencies();
        dependencies.PdmProvider.ValidationResponse = null;
        var request = CreateRequest();
        request.FareBasis = "Y26";

        var response = await dependencies.CreateMutation().VSRetroClaim(request);

        AssertPdmValidationFailure(response, dependencies);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsUnacceptedZeroPointResult_WhenVredDoesNotRespond()
    {
        var dependencies = new TestDependencies();
        dependencies.VredProvider.Response = null;

        var response = await dependencies.CreateMutation().VSRetroClaim(CreateRequest());

        AssertVredPostingFailure(response, dependencies);
    }

    [Fact]
    public async Task VSRetroClaim_ReturnsUnacceptedZeroPointResult_WhenVredReturnsANonOkStatus()
    {
        var dependencies = new TestDependencies();
        dependencies.VredProvider.Response = new VRedTransactionResponse { Status = Status.Error };

        var response = await dependencies.CreateMutation().VSRetroClaim(CreateRequest());

        AssertVredPostingFailure(response, dependencies);
    }

    private static VSRetroClaimRequest CreateRequest() => new()
    {
        Id = 42,
        TicketNumber = "9321234567890",
        MemberNumber = "1234567890",
        InitiatedBy = "CCAGENT",
        ArrivalAirport = "JFK",
        OperatingAirlineCode = "VS",
        OperatingFlightNumber = "3",
        DepartureAirport = "LHR",
        FlownCabinClass = "Upper",
        DepartureDate = new DateTime(2026, 8, 15),
        IataAgentNumber = "1234567",
        LoyaltyProgramme = "VS",
        MarketingAirlineCode = "VS",
        MarketingFlightNumber = "3",
        OriginalBookedClass = "A",
        RecordLocatorOperatingCarrier = "ABC123"
    };

    private static void SetMissingRequiredField(VSRetroClaimRequest request, string field)
    {
        switch (field)
        {
            case "ArrivalAirport": request.ArrivalAirport = " "; break;
            case "MemberNumber": request.MemberNumber = null; break;
            case "OperatingAirlineCode": request.OperatingAirlineCode = string.Empty; break;
            case "OperatingFlightNumber": request.OperatingFlightNumber = " "; break;
            case "DepartureAirport": request.DepartureAirport = null; break;
            case "FlownCabinClass": request.FlownCabinClass = string.Empty; break;
            case "Id": request.Id = 0; break;
            case "DepartureDate": request.DepartureDate = DateTime.MinValue; break;
            case "InitiatedBy": request.InitiatedBy = string.Empty; break;
            case "IataAgentNumber": request.IataAgentNumber = " "; break;
            default: throw new ArgumentOutOfRangeException(nameof(field));
        }
    }

    private sealed class TestDependencies
    {
        internal CustomerProvider CustomerProvider { get; } = new();
        internal PdmProvider PdmProvider { get; } = new();
        internal ReferenceProvider ReferenceProvider { get; } = new();
        internal InRuleProvider InRuleProvider { get; set; } = new();
        internal ProductsProvider ProductsProvider { get; } = new();
        internal PartnersProvider PartnersProvider { get; } = new();
        internal TransactionProvider TransactionProvider { get; } = new();
        internal VredProvider VredProvider { get; } = new();
        internal CustomerTransactionQueryProvider CustomerTransactionQueryProvider { get; } = new();
        internal LoyaltyMemberMgtProvider MemberMgtProvider { get; } = new();

        internal TestDependencies()
        {
            CustomerTransactionQueryProvider.FlyingClubMemberResponse = new FlyingClubMemberAPIResponse
            {
                FlyingClubMemberDocuments =
                [
                    new FlyingClubMember { EnrollmentDate = new DateTime(2024, 1, 1) }
                ]
            };

            MemberMgtProvider.StatementTextResponse = new TransactionStatementTextResponse { StatementText = "VS retro flight statement" };
        }

        internal RetroClaimMutation CreateMutation() => new(
            NullLogger<RetroClaimMutation>.Instance,
            CustomerProvider,
            PdmProvider,
            ReferenceProvider,
            InRuleProvider,
            ProductsProvider,
            PartnersProvider,
            TransactionProvider,
            VredProvider,
            CustomerTransactionQueryProvider,
            MemberMgtProvider);
    }

    private sealed class CustomerProvider : ICustomerProvider
    {
        internal int GetCustomerCalls { get; private set; }
        internal string LastCustomerId { get; private set; }
        internal string LastMemberNumber { get; private set; }
        internal CustomerDTO Response { get; set; } = new()
        {
            Customer = new CustomerCustMS
            {
                CustomerId = "customer-1",
                FirstName = "Test",
                LastName = "Member",
                Status = "Active",
                MemberTier = "Gold",
                MemberAccountType = "Individual"
            }
        };

        public Task<CustomerDTO> GetCustomer(string customerId, string memberNumber)
        {
            GetCustomerCalls++;
            LastCustomerId = customerId;
            LastMemberNumber = memberNumber;
            if (Response.Customer != null)
            {
                Response.Customer.LoyaltyNumber = memberNumber;
            }

            return Task.FromResult(Response);
        }
    }

    private static void AssertPdmValidationFailure(VSRetroClaimResponse response, TestDependencies dependencies)
    {
        var flight = Assert.Single(response.Flights);
        Assert.False(flight.Accepted);
        Assert.Equal("VS retro claim input validation could not be completed.", flight.Reason);
        Assert.Equal(1, dependencies.PdmProvider.ValidateInputsCalls);
        Assert.Null(dependencies.InRuleProvider.LastRequest);
        Assert.Null(dependencies.TransactionProvider.CreatedTransaction);
        Assert.False(dependencies.VredProvider.PostCalled);
        Assert.Equal(0, dependencies.PdmProvider.UpdateStatusCalls);
    }

    private static void AssertVredPostingFailure(VSRetroClaimResponse response, TestDependencies dependencies)
    {
        var flight = Assert.Single(response.Flights);
        Assert.False(flight.Accepted);
        Assert.Equal(0, flight.PointsEarned);
        Assert.Equal(0, flight.TierPointsEarned);
        Assert.Equal("The retro transaction was created but could not be posted to VRed.", flight.Reason);
        Assert.Equal(0, response.TotalPointsEarned);
        Assert.Equal(0, response.TotalTierPointsEarned);
        Assert.True(dependencies.VredProvider.PostCalled);
        Assert.Equal(0, dependencies.PdmProvider.UpdateStatusCalls);
    }

    private sealed class CustomerTransactionQueryProvider : ICustomerTransactionQueryProvider
    {
        internal int GetFlyingClubMemberCalls { get; private set; }
        internal string LastCustomerId { get; private set; }
        internal FlyingClubMemberAPIResponse FlyingClubMemberResponse { get; set; }

        public Task<AwardResponse> GetAwardDetails(string customerId) => throw new NotSupportedException();

        public Task<VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.TransactionResponse<Transaction>> CustomerTransactionQuery(string url, string customerId) => throw new NotSupportedException();

        public Task<AwardMutationResponse> MutateAward(AwardRequest request, System.Net.Http.HttpMethod methodType) => throw new NotSupportedException();

        public Task<FlyingClubMemberMutationResponse> UpdateFlyingClubMember(FlyingClubMemberRequest request) => throw new NotSupportedException();

        public Task<FlyingClubMemberAPIResponse> GetFlyingClubMember(string customerId)
        {
            GetFlyingClubMemberCalls++;
            LastCustomerId = customerId;
            return Task.FromResult(FlyingClubMemberResponse);
        }

        public Task<VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.TransactionResponse<Transaction>> RelatedTransactionsQuery(string customerId, string transactionId) => throw new NotSupportedException();

        public Task<MemberCustomAttributesResponse> GetMemberCustomAttributes(string customerId) => throw new NotSupportedException();
    }

    private sealed class LoyaltyMemberMgtProvider : ILoyaltyMemberMgtProvider
    {
        internal int GenerateStatementTextCalls { get; private set; }
        internal TransactionStatementTextResponse StatementTextResponse { get; set; }

        public Task<RewardBookingPointCalculationResponse> RetrieveRewardBookingPoints(RewardBookingPointCalculationRequest rewardBookingPointCalculationRequest) => throw new NotSupportedException();

        public Task<TransactionStatementTextResponse> GenerateStatetmentText(TransactionStatementTextRequest statementRequest)
        {
            GenerateStatementTextCalls++;
            return Task.FromResult(StatementTextResponse);
        }
    }

    private sealed class PdmProvider : IPartnerDataManagementQueryProvider
    {
        internal TicketStatusRequest StatusRequest { get; private set; }
        internal int UpdateStatusCalls { get; private set; }
        internal ValidateVSClaimInputsRequest ValidationRequest { get; private set; }
        internal ValidateVSClaimInputsResponse? ValidationResponse { get; set; } = new() { Success = true };
        internal int ValidateInputsCalls { get; private set; }

        public Task<VSFlownDetailsResponse> GetVSRetroFlownData(GetVSRetroFlownDataRequest request)
        {
            throw new NotSupportedException("The request-driven VS retro flow must not retrieve flown data.");
        }

        public Task<ValidateVSClaimInputsResponse> ValidateVSClaimInputs(ValidateVSClaimInputsRequest request)
        {
            ValidateInputsCalls++;
            ValidationRequest = request;
            return Task.FromResult(ValidationResponse!);
        }

        public Task<EditVSRetroFlownDataResponse> UpdateVSRetroFlownDetails(EditVSRetroFlownDataRequest request)
        {
            throw new NotSupportedException("The PDM update-details endpoint is not used by VS retro claims.");
        }

        public Task<string> UpdateVSRetroFlownStatus(TicketStatusRequest request)
        {
            UpdateStatusCalls++;
            StatusRequest = request;
            return Task.FromResult("Updated");
        }
    }

    private sealed class ReferenceProvider : IReferenceDataMutationProvider
    {
        internal AirportDistanceAndCountries Route { get; set; } = new() { DistanceInMiles = "3451", OriginCountryCode = "GB", DestinationCountryCode = "US" };
        internal string LastOriginAirport { get; private set; }
        internal string LastDestinationAirport { get; private set; }

        public Task<CabinClassDetailsResponse> GetCabinClassDetails(CabinClassDetailsRequest request) => Task.FromResult(new CabinClassDetailsResponse { CabinClass = "C" });
        public Task<IEnumerable<CountryDetails>> GetCountryDetails() => Task.FromResult<IEnumerable<CountryDetails>>([]);
        public Task<AirportDistanceAndCountries> GetSegmentDistanceAndCountry(string originAirportCode, string destinationAirportCode)
        {
            LastOriginAirport = originAirportCode;
            LastDestinationAirport = destinationAirportCode;
            return Task.FromResult(Route);
        }

        public Task<string> GetSegmentsDistance(string originAirportCode, string destinationAirportCode) => Task.FromResult("3451");
    }

    private sealed class InRuleProvider : IInRuleMutationProvider
    {
        internal CreditPointsResponse Response { get; set; } = new()
        {
            SegmentResponses = new[]
            {
                new SegmentResponses { PointsAward = "2000", BasePoints = "1800", CabinBonusPoints = "200", TierPointsAward = "250" }
            }
        };

        internal AirlineEarnedPointsRequest LastRequest { get; private set; }

        public Task<CreditPointsResponse> CalculatePoints(AirlineEarnedPointsRequest request)
        {
            LastRequest = request;
            return Task.FromResult(Response);
        }
    }

    private sealed class ProductsProvider : IProductsQueryProvider
    {
        public Task<IEnumerable<ProductDetails>> GetProductDetails() => Task.FromResult<IEnumerable<ProductDetails>>([new ProductDetails { RowId = "product-1", ProductCategory = "Flight", ProductName = "Flight" }]);
    }

    private sealed class PartnersProvider : IPartnersProfilesQueryProvider
    {
        public Task<IEnumerable<PartnerProfileDetails>> GetPartnerProfileDetails() => Task.FromResult<IEnumerable<PartnerProfileDetails>>([new PartnerProfileDetails { RowId = "partner-1", PartnerCode = "VS", Name = "Virgin Atlantic" }]);
    }

    private sealed class TransactionProvider : ICustomerTransactionMutationProvider<TransactionResponse>
    {
        internal TransactionRequest CreatedTransaction { get; private set; }

        public Task<TransactionResponse> CreateTransaction(TransactionRequest txnRequest)
        {
            CreatedTransaction = txnRequest;
            return Task.FromResult(new TransactionResponse { CommandStatus = Status.OK, Transaction = txnRequest.TransactionDocument });
        }

        public Task<TransactionResponse> Mutate(VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership.PartnerMembershipRequest partnerMembership) => throw new NotSupportedException();
        public Task<VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile.MemberPreferenceUpdateResponse> UpdateVACMemberPreference(VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile.MemberPreferenceUpdateRequest updateMemberPreferenceRequest) => throw new NotSupportedException();
        public Task<TransactionResponse> CancelTransaction(TransactionRequest txnRequest) => throw new NotSupportedException();
    }

    private sealed class VredProvider : IVredMutationProvider
    {
        internal bool PostCalled { get; private set; }
        internal VRedTransactionResponse? Response { get; set; } = new() { Status = Status.OK };

        public Task<VRedTransactionResponse> PostTransactionToVred(VredSyncTransactionRequest vredTransactionRequest)
        {
            PostCalled = true;
            return Task.FromResult(Response!);
        }

        public Task<VRedTransactionResponse> CancelTransactionVred(VredCancelTransactionRequest vredCancelRequest) => throw new NotSupportedException();
    }
}
