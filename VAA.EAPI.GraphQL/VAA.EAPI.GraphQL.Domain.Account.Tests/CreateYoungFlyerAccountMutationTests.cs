using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Models.HouseHoldAccount;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class CreateYoungFlyerAccountMutationTests
{
    [Fact]
    public async Task CreateYoungFlyerAccount_RejectsMissingAccessToken()
    {
        SetAccessToken(null);
        var mutation = CreateMutation(new TokenProvider("{}"));

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01")));

        Assert.Contains("authenticated parent is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        var error = Assert.Single(exception.Errors);
        Assert.Equal(Common.Constants.Application.MissingB2CTokenErrorCode, error.Code);
        Assert.Equal(Common.Constants.Application.MissingB2CTokenErrorCode, error.Extensions!["code"]);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_RejectsTokenWithoutParentIdentifiers()
    {
        SetAccessToken("token");
        var mutation = CreateMutation(new TokenProvider("{}"));

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01")));

        Assert.Contains("parent could not be resolved", exception.Message, StringComparison.OrdinalIgnoreCase);
        var error = Assert.Single(exception.Errors);
        Assert.Equal(Common.Constants.Application.InValidB2CTokenErrorCode, error.Code);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_RejectsDateOfBirthAtConfiguredAgeLimit()
    {
        SetAccessToken("token");
        ConfigReader.Set(Common.Constants.Environment.YoungFlyerAgeLimit, "21");
        try
        {
            var claims = Json.Serialize(new TokenB2CResponse { CustomerId = "customer", FcNumber = "member" });
            var mutation = CreateMutation(new TokenProvider(claims));
            var dateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-21).ToString("yyyy-MM-dd");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest(dateOfBirth)));

            Assert.Contains("Young Flyer age limit of 21", exception.Message, StringComparison.OrdinalIgnoreCase);
            var error = Assert.Single(exception.Errors);
            Assert.Equal(Common.Constants.Application.MandatoryInputFieldsMissingCode, error.Code);
        }
        finally
        {
            ConfigReader.Set(Common.Constants.Environment.YoungFlyerAgeLimit, "18");
        }
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_RejectsInvalidDateOfBirthWithSpecificMessage()
    {
        SetAccessToken("token");
        var claims = Json.Serialize(new TokenB2CResponse { CustomerId = "customer", FcNumber = "member" });
        var mutation = CreateMutation(new TokenProvider(claims));

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest("not-a-date")));

        Assert.Contains("date of birth is invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("under-18", exception.Message, StringComparison.OrdinalIgnoreCase);
        var error = Assert.Single(exception.Errors);
        Assert.Equal(Common.Constants.Application.MandatoryInputFieldsMissingCode, error.Code);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_RejectsDateOfBirthOutsideIsoFormat()
    {
        SetAccessToken("token");
        var claims = Json.Serialize(new TokenB2CResponse { CustomerId = "customer", FcNumber = "member" });
        var mutation = CreateMutation(new TokenProvider(claims));

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest("01/01/2015")));

        Assert.Contains("date of birth is invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
        var error = Assert.Single(exception.Errors);
        Assert.Equal(Common.Constants.Application.MandatoryInputFieldsMissingCode, error.Code);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_ResolvesParentByFlyingClubNumber_WhenCustomerIdClaimIsAbsent()
    {
        SetAccessToken("token");
        var customerService = new CustomerService();
        var mutation = CreateMutation(
            new TokenProvider(Json.Serialize(new TokenB2CResponse { FcNumber = "parent-member" })),
            customerService,
            new EnrolmentProvider(),
            new HouseholdProvider());

        await mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01"));

        Assert.Null(customerService.CustomerId);
        Assert.Equal("parent-member", customerService.MemberNumber);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_DoesNotLinkHousehold_WhenEnrolmentHasNoMemberNumber()
    {
        SetAccessToken("token");
        var householdProvider = new HouseholdProvider();
        var mutation = CreateMutation(
            new TokenProvider(Json.Serialize(new TokenB2CResponse { FcNumber = "parent-member" })),
            enrolmentProvider: new EnrolmentProvider(new FlyingClubSignUpResponse { CustomerId = "child-customer" }),
            householdProvider: householdProvider);

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01")));

        Assert.Contains("did not return an eligible member", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, householdProvider.HouseholdLinkingAttempts);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_LinksNewChildToHousehold()
    {
        SetAccessToken("token");
        var householdProvider = new HouseholdProvider();
        var mutation = CreateMutation(
            new TokenProvider(Json.Serialize(new TokenB2CResponse { FcNumber = "parent-member" })),
            householdProvider: householdProvider);

        await mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01"));

        Assert.Equal(1, householdProvider.HouseholdLinkingAttempts);
        Assert.Equal("parent-customer", householdProvider.LastLinkingRequest.HeadAccount.CustomerId);
        Assert.Equal("child-customer", householdProvider.LastLinkingRequest.ChildAccount.CustomerId);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_PassesGuardianDetailsToEnrolment()
    {
        SetAccessToken("token");
        var enrolmentProvider = new EnrolmentProvider();
        var mutation = CreateMutation(
            new TokenProvider(Json.Serialize(new TokenB2CResponse { FcNumber = "parent-member" })),
            enrolmentProvider: enrolmentProvider);

        await mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01"));

        var guardian = enrolmentProvider.LastRequest.CustomerDetails.LoyaltyMember.GuardianDetails;
        Assert.NotNull(guardian);
        Assert.Equal("Parent Member", guardian.Name);
        Assert.Equal("parent-member", guardian.CardNumber);
        Assert.True(guardian.GuardianTermsAndConditionsAccepted);
    }

    [Fact]
    public async Task CreateYoungFlyerAccount_RejectsInactiveParentBeforeEnrolment()
    {
        SetAccessToken("token");
        var enrolmentProvider = new EnrolmentProvider();
        var householdProvider = new HouseholdProvider();
        var mutation = CreateMutation(new TokenProvider(Json.Serialize(new TokenB2CResponse { FcNumber = "parent-member" })), new CustomerService { Status = "Inactive" }, enrolmentProvider, householdProvider);

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.CreateYoungFlyerAccount(CreateRequest("2015-01-01")));

        Assert.Equal("10007", Assert.Single(exception.Errors).Code);
        Assert.Equal(0, enrolmentProvider.EnrolmentAttempts);
        Assert.Equal(0, householdProvider.HouseholdLinkingAttempts);
    }

    private static CreateYoungFlyerAccountMutation CreateMutation(
        ITokenB2CProvider<string> tokenProvider,
        CustomerService? customerService = null,
        EnrolmentProvider? enrolmentProvider = null,
        HouseholdProvider? householdProvider = null)
    {
        customerService ??= new CustomerService();
        enrolmentProvider ??= new EnrolmentProvider();
        householdProvider ??= new HouseholdProvider();
        using var serviceProvider = new ServiceCollection()
            .AddSingleton(tokenProvider)
            .AddSingleton<IAccountCustomerService>(customerService)
            .AddSingleton<IEnrolmentProvider>(enrolmentProvider)
            .AddSingleton<IHouseHoldMutationProvider>(householdProvider)
            .AddSingleton<YoungFlyerAccountRequestFactory>()
            .AddSingleton<CreateYoungFlyerAccountMutation>()
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<CreateYoungFlyerAccountMutation>();
    }

    private static CreateCustomerAccountRequest CreateRequest(string dateOfBirth) =>
        new() { CustomerDetails = new CustomerAccountDetails { DateOfBirth = dateOfBirth } };

    private static void SetAccessToken(string? token)
    {
        AsyncContext.Current = new AsyncContext { Context = new TestFunctionContext() };
        if (token is not null)
        {
            AsyncContext.Current.Context.Features.Set(new JwtPrincipalFeature(new ClaimsPrincipal(), new AuthToken { Token = token }));
        }
    }

    private sealed class TokenProvider(string response) : ITokenB2CProvider<string>
    {
        public Task<string> ValidateTokenAsync(string token) => Task.FromResult(response);
    }

    private sealed class CustomerService : IAccountCustomerService
    {
        public string Status { get; set; } = "Active";
        public string? CustomerId { get; private set; }
        public string? MemberNumber { get; private set; }

        public Task<CustomerDTO> GetCustomer(string customerId, string memberNumber)
        {
            CustomerId = customerId;
            MemberNumber = memberNumber;
            return Task.FromResult(new CustomerDTO
            {
                Customer = new CustomerCustMS
                {
                    CustomerId = "parent-customer",
                    FirstName = "Parent",
                    LastName = "Member",
                    LoyaltyNumber = "parent-member",
                    Status = Status
                }
            });
        }

        public Task<List<CustomerProfileDetails>> GetCustomerDetailsByEmail(CustomerProfileRequest request) => throw new NotSupportedException();
    }

    private sealed class EnrolmentProvider(FlyingClubSignUpResponse? youngFlyerResponse = null) : IEnrolmentProvider
    {
        public int EnrolmentAttempts { get; private set; }
        public CreateCustomerAccountRequest LastRequest { get; private set; } = new();
        public Task<FlyingClubSignUpResponse> EnrolFcMember(CreateCustomerAccountRequest request) => throw new NotSupportedException();
        public Task<FlyingClubSignUpResponse> EnrolYoungFlyerMember(CreateCustomerAccountRequest request)
        {
            EnrolmentAttempts++;
            LastRequest = request;
            return Task.FromResult(youngFlyerResponse ?? new FlyingClubSignUpResponse
            {
                CustomerId = "child-customer",
                MemberDetails = new MemberDetails { MemberNumber = "child-member" }
            });
        }
        public Task<EntraEnrolTokenResponse> EntraEnrolFcMember(CreateCustomerAccountRequest request) => throw new NotSupportedException();
    }

    private sealed class HouseholdProvider(List<string>? calls = null) : IHouseHoldMutationProvider
    {
        public int HouseholdLinkingAttempts { get; private set; }
        public HouseHoldLinkingRequest LastLinkingRequest { get; private set; } = new();

        public Task<ResponseMessage> HouseHoldLinking(HouseHoldLinkingRequest linking)
        {
            calls?.Add("link");
            HouseholdLinkingAttempts++;
            LastLinkingRequest = linking;
            return Task.FromResult(new ResponseMessage());
        }
    }

}
