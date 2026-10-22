using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations.HouseHoldAccount;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using XunitHttpClientFactory = VAA.EAPI.GraphQL.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class InviteMemberMutationTests
{
    private readonly IServiceProvider _serviceProvider;

    public InviteMemberMutationTests()
    {
        Setup.CreateTestEnvironmentSettings();
        _serviceProvider = CreateServiceProvider();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTokenProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(null!, GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), null!, GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHttpConnectorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), null!, GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOAuthTokenProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), null!, GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCustomerProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), null!, GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenHouseholdMutationProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), null!, GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenInviteDocumentProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), null!, GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenInviteePendingDocumentProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), null!, GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenInviterPendingDocumentProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), null!, GetGroupDocumentProvider(), GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenGroupDocumentProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), null!, GetFlyingClubMemberProvider()));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFlyingClubMemberProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new InviteMemberMutation(GetTokenB2CProvider(), GetLogger(), GetHttpConnector(), GetOAuthTokenProvider(), GetCustomerService(), GetHouseholdMutationProvider(), GetCreateInviteDocumentProvider(), GetInvitePendingDocumentProvider(), GetInvitePendingDocumentProvider(), GetGroupDocumentProvider(), null!));
    }

    [Fact]
    public async Task InviteMember_ThrowsArgumentNullException_WhenLoginTokenIsMissing()
    {
        SetAccessToken(null);

        await Assert.ThrowsAsync<ArgumentNullException>(() => _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("9834304343", "Test Account"));
    }

    [Fact]
    public async Task InviteMember_ReturnsGraphQlError_WhenTokenValidationFails()
    {
        SetAccessToken("access-token");
        GetTokenProvider().Exception = CreateHttpRequestException(CreateValidationErrorResponse());

        await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("9834304343", "Test Account"));
    }

    [Fact]
    public async Task InviteMember_ReturnsGraphQlError_WhenLeaderCustomerLookupFails()
    {
        SetAccessToken("access-token");
        GetCustomerServiceFake().Exception = CreateHttpRequestException(CreateValidationErrorResponse());

        await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("9834304343", "Test Account"));
    }

    [Fact]
    public async Task InviteMember_CreatesInviteDocument_ForAdultInvitee()
    {
        SetAccessToken("access-token");
        var createInviteDocumentProvider = GetCreateInviteDocumentProviderFake();

        var result = await _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("9834304343", "Test Account");

        Assert.Equal("Successfully sent household invitation", result);
        Assert.Equal(1, createInviteDocumentProvider.CreateInviteDocumentAttempts);
    }

    [Fact]
    public async Task InviteMember_AddsMinorInviteeToHousehold()
    {
        SetAccessToken("access-token");
        var customerService = GetCustomerServiceFake();
        customerService.Customer = CreateCustomer(DateTime.Today.AddYears(-17));
        var householdMutationProvider = GetHouseholdMutationProviderFake();

        var result = await _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("9834304343", "Test Account");

        Assert.Equal("Request successfully initiated", result);
        Assert.Equal(1, householdMutationProvider.LinkingAttempts);
    }

    [Fact]
    public async Task InviteMember_ReturnsGraphQlError_WhenLeaderIsAlreadyLinked()
    {
        SetAccessToken("access-token");
        GetFlyingClubMemberProviderFake().Response = new FcMemberDetails { FlyingClubMemberDocuments = [new FlyingClubMember { HeadOfGroupMemberNumber = "existing-head" }] };

        await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("9834304343", "Test Account"));
    }

    [Fact]
    public async Task InviteMember_ReturnsInvalidFcNumberErrorBeforeLookingUpInvalidInvitee()
    {
        SetAccessToken("access-token");
        var customerService = GetCustomerServiceFake();
        customerService.GuardInvalidInviteeLookup = true;

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => _serviceProvider.GetRequiredService<InviteMemberMutation>().InviteMember("invalid-member", "Household"));
        var error = Assert.Single(exception.Errors);

        Assert.Equal("10164", error.Extensions!["code"]);
        Assert.Equal("Invited FC member number is not recognised", error.Extensions["message"]);
        Assert.Equal(1, customerService.LeaderLookupAttempts);
        Assert.Equal(0, customerService.InviteeLookupAttempts);
    }

    private IServiceProvider CreateServiceProvider() =>
        new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<ITokenB2CProvider<string>, TokenProvider>()
            .AddSingleton<ILogger<InviteMemberMutation>>(NullLogger<InviteMemberMutation>.Instance)
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>()
            .AddSingleton<IAccountCustomerService, CustomerService>()
            .AddSingleton<IHouseHoldMutationProvider, HouseholdMutationProvider>()
            .AddSingleton<ICustomerTransactionQueryProvider<string>, CustomerTransactionProvider<string>>()
            .AddSingleton<ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>>, CustomerTransactionProvider<InviteHouseHoldResponse<InvitePending>>>()
            .AddSingleton<ICustomerTransactionQueryProvider<GroupDocumentDTO>, CustomerTransactionProvider<GroupDocumentDTO>>()
            .AddSingleton<ICustomerTransactionQueryProvider<FcMemberDetails>, CustomerTransactionProvider<FcMemberDetails>>()
            .AddSingleton<InviteMemberMutation>()
            .BuildServiceProvider();

    private ITokenB2CProvider<string> GetTokenB2CProvider() => _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();
    private ILogger<InviteMemberMutation> GetLogger() => _serviceProvider.GetRequiredService<ILogger<InviteMemberMutation>>();
    private IHttpConnector GetHttpConnector() => _serviceProvider.GetRequiredService<IHttpConnector>();
    private IOAuthTokenProvider GetOAuthTokenProvider() => _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
    private IAccountCustomerService GetCustomerService() => _serviceProvider.GetRequiredService<IAccountCustomerService>();
    private IHouseHoldMutationProvider GetHouseholdMutationProvider() => _serviceProvider.GetRequiredService<IHouseHoldMutationProvider>();
    private ICustomerTransactionQueryProvider<string> GetCreateInviteDocumentProvider() => _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<string>>();
    private ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> GetInvitePendingDocumentProvider() => _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>>>();
    private ICustomerTransactionQueryProvider<GroupDocumentDTO> GetGroupDocumentProvider() => _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<GroupDocumentDTO>>();
    private ICustomerTransactionQueryProvider<FcMemberDetails> GetFlyingClubMemberProvider() => _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<FcMemberDetails>>();
    private TokenProvider GetTokenProvider() => (TokenProvider)GetTokenB2CProvider();
    private CustomerService GetCustomerServiceFake() => (CustomerService)GetCustomerService();
    private HouseholdMutationProvider GetHouseholdMutationProviderFake() => (HouseholdMutationProvider)GetHouseholdMutationProvider();
    private CustomerTransactionProvider<string> GetCreateInviteDocumentProviderFake() => (CustomerTransactionProvider<string>)GetCreateInviteDocumentProvider();
    private CustomerTransactionProvider<FcMemberDetails> GetFlyingClubMemberProviderFake() => (CustomerTransactionProvider<FcMemberDetails>)GetFlyingClubMemberProvider();

    private static CustomerDTO CreateCustomer(DateTime dateOfBirth) => new()
    {
        Customer = new CustomerCustMS
        {
            CustomerId = "customer-id",
            LoyaltyNumber = "9834304343",
            DateOfBirth = dateOfBirth,
            ContactDetails = new Contact { EmailDetails = [new Email { PreferredFlag = "Y", emailId = "member@example.test" }] }
        }
    };

    private static HttpResponseMessage CreateValidationErrorResponse() => new()
    {
        StatusCode = System.Net.HttpStatusCode.UnprocessableEntity,
        Content = new StringContent("[{\"code\":\"10164\",\"message\":\"Invited FC member number is not recognised\",\"errorSource\":\"Consuming application\"}]")
    };

    private static HttpRequestException CreateHttpRequestException(HttpResponseMessage response) =>
        new("Http connector failed.", new HttpResponseException(response), response.StatusCode);

    private static void SetAccessToken(string? token)
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext(new Dictionary<string, object?>()) };
        AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(), new AuthToken { Token = token }));
    }

    private sealed class TokenProvider : ITokenB2CProvider<string>
    {
        public Exception? Exception { get; set; }

        public Task<string> ValidateTokenAsync(string token) => Exception is not null
            ? Task.FromException<string>(Exception)
            : Task.FromResult(Json.Serialize(new TokenB2CResponse { CustomerId = "customer-id", FcNumber = "leader-member" }));
    }

    private sealed class OAuthTokenProvider : IOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("service-token");
    }

    private sealed class CustomerService : IAccountCustomerService
    {
        public CustomerDTO Customer { get; set; } = CreateCustomer(DateTime.Today.AddYears(-18));
        public Exception? Exception { get; set; }
        public bool GuardInvalidInviteeLookup { get; set; }
        public int LeaderLookupAttempts { get; private set; }
        public int InviteeLookupAttempts { get; private set; }

        public Task<CustomerDTO> GetCustomer(string customerId, string memberNumber)
        {
            if (GuardInvalidInviteeLookup && memberNumber == "invalid-member")
            {
                InviteeLookupAttempts++;
                throw new InvalidOperationException("The invitee must not be looked up before validation.");
            }

            LeaderLookupAttempts++;

            return Exception is not null
                ? Task.FromException<CustomerDTO>(Exception)
                : Task.FromResult(Customer);
        }

        public Task<List<CustomerProfileDetails>> GetCustomerDetailsByEmail(CustomerProfileRequest request) => throw new NotSupportedException();
    }

    private sealed class HouseholdMutationProvider : IHouseHoldMutationProvider
    {
        public int LinkingAttempts { get; private set; }

        public Task<ResponseMessage> HouseHoldLinking(HouseHoldLinkingRequest linking)
        {
            LinkingAttempts++;
            return Task.FromResult(new ResponseMessage { Message = "Request successfully initiated" });
        }
    }

    private sealed class CustomerTransactionProvider<T> : ICustomerTransactionQueryProvider<T>
    {
        public T Response { get; set; } = CreateResponse();
        public int CreateInviteDocumentAttempts { get; private set; }

        public Task<T> CustomerTransactionQuery(string url, string customerId) => Task.FromResult(Response);
        public Task<T> CustomerTransactionQueryCustomerProfile(string url, string customerId) => Task.FromResult(Response);
        public Task<T> CustomerTransactionUpdateAddressHouseHold(string url, string customerId) => Task.FromResult(Response);

        public Task<T> CustomerTransactionCreateHouseHoldInviteDoc(string url, string groupName, CustomerDTO leaderFcNumber, CustomerDTO inviteeFcNumber)
        {
            CreateInviteDocumentAttempts++;
            return Task.FromResult(Response);
        }

        public Task<T> CustomerTransactionDeleteHouseHoldInviteDoc(string url, string leaderFcNumber, string inviteeFcNumber) => Task.FromResult(Response);
        public Task<T> CustomerTransactionGetInviteDocInvitee(string url, string memberNumber) => Task.FromResult(Response);
        public Task<T> CustomerTransactionGetInviteDocInviter(string url, string memberNumber) => Task.FromResult(Response);
        public Task<T> CreateTransaction(VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction.TransactionRequest transactionRequest) => Task.FromResult(Response);
        public Task<T> GetMembershipChangeDocByCustomerID(string customerId) => Task.FromResult(Response);

        private static T CreateResponse()
        {
            if (typeof(T) == typeof(string))
                return (T)(object)"Successfully sent household invitation";

            if (typeof(T) == typeof(InviteHouseHoldResponse<InvitePending>))
                return (T)(object)new InviteHouseHoldResponse<InvitePending> { InviteHouseHold = [] };

            if (typeof(T) == typeof(GroupDocumentDTO))
                return (T)(object)new GroupDocumentDTO { GroupDocuments = [] };

            if (typeof(T) == typeof(FcMemberDetails))
                return (T)(object)new FcMemberDetails { FlyingClubMemberDocuments = [new FlyingClubMember()] };

            return default!;
        }
    }

}
