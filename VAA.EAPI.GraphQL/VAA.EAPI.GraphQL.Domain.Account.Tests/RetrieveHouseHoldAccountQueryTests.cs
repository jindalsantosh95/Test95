using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Http;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Mappers;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;
using VAA.EAPI.GraphQL.Domain.Account.Queries.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class RetrieveHouseHoldAccountQueryTests
{
    private const string IdentityUrl = "https://testresources.virginatlantic.com/retrieve-household/identity";
    private const string CustomerUrl = "https://testresources.virginatlantic.com/youngflyer/customer/{customerid}";
    private const string CustomerTransactionsUrl = "https://testresources.virginatlantic.com/retrieve-household/customertransactions/";
    private readonly IServiceProvider _serviceProvider;
    private readonly IAccountCustomerService _customerService;
    private readonly ITokenB2CProvider<string> _tokenB2CProvider;
    private readonly IHttpConnector _connector;
    private readonly ICustomerTransactionQueryProvider<GroupDocumentDTO> _groupDocumentProvider;
    private readonly IOAuthTokenProvider _tokenProvider;
    private readonly ICustomerTransactionQueryProvider<FcMemberDetails> _flyingClubProvider;
    private readonly ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> _inviteeProvider;
    private readonly ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>> _inviterProvider;

    public RetrieveHouseHoldAccountQueryTests()
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigureBaseEnvironment();
        var httpClientFactory = new HttpClientFactory();

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IHttpClientFactory>(httpClientFactory)
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
            .AddSingleton<IAccountCustomerService, CustomerService>()
            .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
            .AddSingleton<ICustomerTransactionQueryProvider<GroupDocumentDTO>, CustomerTransactionQueryProvider<GroupDocumentDTO>>()
            .AddSingleton<ICustomerTransactionQueryProvider<FcMemberDetails>, CustomerTransactionQueryProvider<FcMemberDetails>>()
            .AddSingleton<ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>>, CustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>>>()
            .AddSingleton<RetrieveHouseHoldAccountQuery>()
            .BuildServiceProvider();

        _connector = _serviceProvider.GetRequiredService<IHttpConnector>();
        _tokenProvider = _serviceProvider.GetRequiredService<IOAuthTokenProvider>();
        _customerService = _serviceProvider.GetRequiredService<IAccountCustomerService>();
        _tokenB2CProvider = _serviceProvider.GetRequiredService<ITokenB2CProvider<string>>();
        _groupDocumentProvider = _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<GroupDocumentDTO>>();
        _flyingClubProvider = _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<FcMemberDetails>>();
        _inviteeProvider = _serviceProvider.GetRequiredService<ICustomerTransactionQueryProvider<InviteHouseHoldResponse<InvitePending>>>();
        _inviterProvider = _inviteeProvider;
    }

    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(null!, _customerService, _tokenB2CProvider, _connector, _groupDocumentProvider, _tokenProvider, _flyingClubProvider, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, null!, _tokenB2CProvider, _connector, _groupDocumentProvider, _tokenProvider, _flyingClubProvider, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, null!, _connector, _groupDocumentProvider, _tokenProvider, _flyingClubProvider, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, _tokenB2CProvider, null!, _groupDocumentProvider, _tokenProvider, _flyingClubProvider, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, _tokenB2CProvider, _connector, null!, _tokenProvider, _flyingClubProvider, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, _tokenB2CProvider, _connector, _groupDocumentProvider, null!, _flyingClubProvider, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, _tokenB2CProvider, _connector, _groupDocumentProvider, _tokenProvider, null!, _inviteeProvider, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, _tokenB2CProvider, _connector, _groupDocumentProvider, _tokenProvider, _flyingClubProvider, null!, _inviterProvider));
        Assert.Throws<ArgumentNullException>(() => new RetrieveHouseHoldAccountQuery(NullLogger<RetrieveHouseHoldAccountQuery>.Instance, _customerService, _tokenB2CProvider, _connector, _groupDocumentProvider, _tokenProvider, _flyingClubProvider, _inviteeProvider, null!));
    }

    [Fact]
    public async Task Query_RejectsMissingAccessToken()
    {
        SetRequestContext(null);

        await Assert.ThrowsAsync<ArgumentNullException>(() => Query.GetRetrieveHouseHoldAccountQueryDetails(CreateIndividualRequest()));
    }

    [Fact]
    public async Task Query_ReturnsExplainerPageForIndividualWithoutInvites()
    {
        SetRequestContext("jwt-token");

        var result = await Query.GetRetrieveHouseHoldAccountQueryDetails(CreateIndividualRequest());

        Assert.NotNull(result);
        Assert.Equal(PageResponseType.EXPLAINER_PAGE_WITHOUT_BUTTON, result.PageResponse);
    }

    [Fact]
    public async Task Query_ReturnsExplainerPageWithButton_ForAdultGoldMemberWithoutInvites()
    {
        try
        {
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://testresources.virginatlantic.com/retrieve-household/adult");
            SetRequestContext("jwt-token");

            var result = await Query.GetRetrieveHouseHoldAccountQueryDetails(CreateIndividualRequest());

            Assert.Equal(PageResponseType.EXPLAINER_PAGE_WITH_BUTTON, result.PageResponse);
        }
        finally
        {
            ConfigureBaseEnvironment();
        }
    }

    [Fact]
    public async Task Query_ReturnsAcceptDeclinePage_ForIndividualInvitee()
    {
        try
        {
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://testresources.virginatlantic.com/retrieve-household/invitee");
            SetRequestContext("jwt-token");

            var result = await Query.GetRetrieveHouseHoldAccountQueryDetails(CreateIndividualRequest());

            Assert.Equal(PageResponseType.ACCEPT_DECLINE, result.PageResponse);
            Assert.Equal("Test Household", result.Name);
            Assert.Equal("INVITER", result.InviterInfo.FCNumber);
        }
        finally
        {
            ConfigureBaseEnvironment();
        }
    }

    [Fact]
    public async Task Query_ReturnsResendCancelPage_ForIndividualInviter()
    {
        try
        {
            ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, "https://testresources.virginatlantic.com/retrieve-household/inviter");
            SetRequestContext("jwt-token");

            var result = await Query.GetRetrieveHouseHoldAccountQueryDetails(CreateIndividualRequest());

            Assert.Equal(PageResponseType.RESEND_CANCEL, result.PageResponse);
            Assert.Equal("Test Household", result.Name);
            Assert.Equal("INVITEE", Assert.Single(result.InviteeInfo).FCNumber);
        }
        finally
        {
            ConfigureBaseEnvironment();
        }
    }

    [Fact]
    public void GetManagedYoungFlyerCustomerIds_ReturnsOnlyActiveManagedGroupMembers()
    {
        var groupDocument = new GroupDocumentDTO
        {
            GroupDocuments =
            [
                new GroupDocument
                {
                    GroupHistory =
                    [
                        new GroupHistory { CustomerId = "managed-young-flyer", HoHManaged = true },
                        new GroupHistory { CustomerId = "former-young-flyer", HoHManaged = true, ExitDate = DateTime.UtcNow },
                        new GroupHistory { CustomerId = "ordinary-member", HoHManaged = false },
                        new GroupHistory { CustomerId = "MANAGED-YOUNG-FLYER", HoHManaged = true }
                    ]
                },
                new GroupDocument
                {
                    GroupHistory =
                    [
                        new GroupHistory { CustomerId = "non-current-group-member", HoHManaged = true }
                    ]
                }
            ]
        };

        var customerIds = HouseHoldAccountMapper.GetManagedYoungFlyerCustomerIds(groupDocument);

        Assert.Equal(["managed-young-flyer"], customerIds, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Query_ReturnsDownstreamErrorDetails_WhenIdentityValidationFails()
    {
        try
        {
            ConfigReader.Set(
                Common.Constants.Environment.IdentityMsUrl,
                "https://Testresources.virginatlantic.com/youngflyer/identity/ErrorParent");
            SetRequestContext("jwt-token");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() =>
                Query.GetRetrieveHouseHoldAccountQueryDetails(CreateIndividualRequest()));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Retrieve HouseHold Account Details Query failed", error.Message);
            Assert.Equal("IDENTITY-001", error.Extensions!["code"]);
            Assert.Equal("Token validation failed", error.Extensions!["message"]);
        }
        finally
        {
            ConfigureBaseEnvironment();
        }
    }

    private RetrieveHouseHoldAccountQuery Query => _serviceProvider.GetRequiredService<RetrieveHouseHoldAccountQuery>();

    private static RetrieveHHAccountRequest CreateIndividualRequest() => new()
    {
        MemberType = HouseHoldMemberType.Individual,
        HouseHoldFcNumber = string.Empty
    };

    private static void ConfigureBaseEnvironment()
    {
        ConfigReader.Set(Common.Constants.Environment.IdentityMsUrl, IdentityUrl);
        ConfigReader.Set(Common.Constants.Environment.CustomerUrlGetbyCustID, CustomerUrl);
        ConfigReader.Set(Common.Constants.Environment.CustomerUrlGetByMemNum, "https://testresources.virginatlantic.com/retrieve-household/member/{memberid}");
        ConfigReader.Set(Common.Constants.Environment.CustTransactionMsUrl, CustomerTransactionsUrl);
    }

    private static void SetRequestContext(string? token)
    {
        var context = new TestFunctionContext();
        context.Features.Set(new JwtPrincipalFeature(new System.Security.Claims.ClaimsPrincipal(), new AuthToken { Token = token }));
        AsyncContext.Current = new AsyncContext { Context = context };
    }

    private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
    {
        public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("test-token");
    }
}
