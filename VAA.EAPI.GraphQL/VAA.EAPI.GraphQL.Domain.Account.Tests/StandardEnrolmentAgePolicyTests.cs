using System.Globalization;
using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests;

[Collection("EnvironmentDependentTests")]
public sealed class StandardEnrolmentAgePolicyTests : IDisposable
{
    public StandardEnrolmentAgePolicyTests()
    {
        Setup.CreateTestEnvironmentSettings();
        ConfigReader.Set(Environment.YoungFlyerAgeLimit, "18");
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "false");
    }

    [Theory]
    [InlineData("yyyy-MM-dd")]
    [InlineData("MM/dd/yyyy")]
    public void EnsureEligible_RejectsUnder18ApplicantWithYoungFlyerLoginAction(string dateFormat)
    {
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "true");
        var customer = new CustomerAccountDetails
        {
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-17).ToString(dateFormat, CultureInfo.InvariantCulture)
        };

        var exception = Assert.Throws<GraphQLException>(() => StandardEnrolmentAgePolicy.EnsureEligible(customer));
        var error = Assert.Single(exception.Errors);

        Assert.Equal(Constants.Application.Under18StandardEnrolmentErrorCode, error.Code);
        Assert.Equal(Constants.Application.Under18StandardEnrolmentErrorMessage, error.Message);
        Assert.Equal("loginToCreateYoungFlyer", error.Extensions!["nextAction"]);
    }

    [Theory]
    [InlineData("yyyy-MM-dd")]
    [InlineData("MM/dd/yyyy")]
    public void EnsureEligible_AllowsApplicantAtAgeLimit(string dateFormat)
    {
        var customer = new CustomerAccountDetails
        {
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18).ToString(dateFormat, CultureInfo.InvariantCulture)
        };

        StandardEnrolmentAgePolicy.EnsureEligible(customer);
    }

    [Theory]
    [InlineData("16", 17, true)]
    [InlineData("21", 18, false)]
    public void EnsureEligible_UsesFixedUnder18ThresholdWhenYoungFlyerAgeLimitChanges(string youngFlyerAgeLimit, int applicantAge, bool shouldReject)
    {
        ConfigReader.Set(Environment.YoungFlyerAgeLimit, youngFlyerAgeLimit);
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "true");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var customer = new CustomerAccountDetails
        {
            DateOfBirth = today.AddYears(-applicantAge).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        if (shouldReject)
        {
            var exception = Assert.Throws<GraphQLException>(() => StandardEnrolmentAgePolicy.EnsureEligible(customer));
            AssertUnder18PolicyError(exception);
            return;
        }

        StandardEnrolmentAgePolicy.EnsureEligible(customer);
    }

    [Fact]
    public void EnsureEligible_RejectsFutureDateOfBirthWhenUnder18BlockIsDisabledWithoutOverflow()
    {
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "false");
        var customer = new CustomerAccountDetails
        {
            DateOfBirth = DateOnly.MaxValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        };

        var exception = Assert.Throws<GraphQLException>(() => StandardEnrolmentAgePolicy.EnsureEligible(customer));

        AssertUnder18PolicyError(exception);
    }

    [Theory]
    [InlineData("yyyy-MM-dd")]
    [InlineData("MM/dd/yyyy")]
    public void EnsureEligible_AllowsUnder18ApplicantWhenFeatureFlagIsDisabled(string dateFormat)
    {
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "false");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var customer = new CustomerAccountDetails
        {
            DateOfBirth = today.AddYears(-17).ToString(dateFormat, CultureInfo.InvariantCulture)
        };

        StandardEnrolmentAgePolicy.EnsureEligible(customer);

        Assert.Equal(today.AddYears(-17).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), customer.DateOfBirth);
    }

    [Theory]
    [InlineData("1972-08-17")]
    [InlineData("08/17/1972")]
    public void EnsureEligible_NormalizesAdultDatesRegardlessOfCurrentCulture(string dateOfBirth)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            var customer = new CustomerAccountDetails { DateOfBirth = dateOfBirth };
            StandardEnrolmentAgePolicy.EnsureEligible(customer);

            Assert.Equal("1972-08-17", customer.DateOfBirth);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-date")]
    [InlineData("1972-02-30")]
    [InlineData("02/30/1972")]
    [InlineData("17/08/1972")]
    public void EnsureEligible_RejectsInvalidDateOfBirth(string? dateOfBirth)
    {
        var exception = Assert.Throws<GraphQLException>(() =>
            StandardEnrolmentAgePolicy.EnsureEligible(new CustomerAccountDetails { DateOfBirth = dateOfBirth }));
        var error = Assert.Single(exception.Errors);

        Assert.Equal(Common.Constants.Application.MandatoryInputFieldsMissingCode, error.Code);
        Assert.Equal("Date of birth must be a valid date in yyyy-MM-dd or MM/dd/yyyy format.", error.Message);
    }

    [Theory]
    [InlineData("yyyy-MM-dd")]
    [InlineData("MM/dd/yyyy")]
    public async Task EnrolFcMember_DoesNotCallStandardEnrolmentForUnder18Applicant(string dateFormat)
    {
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "true");
        var provider = new RecordingEnrolmentProvider();
        var mutation = CreateServiceProvider(provider).GetRequiredService<EnrolFlyingClubMemberMutation>();

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.EnrolFcMember(CreateUnder18Request(dateFormat)));

        AssertUnder18PolicyError(exception);
        Assert.False(provider.StandardEnrolmentCalled);
    }

    [Theory]
    [InlineData("yyyy-MM-dd")]
    [InlineData("MM/dd/yyyy")]
    public async Task EntraEnrolFcMember_DoesNotCallStandardEnrolmentForUnder18Applicant(string dateFormat)
    {
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "true");
        var provider = new RecordingEnrolmentProvider();
        var mutation = CreateServiceProvider(provider).GetRequiredService<EntraEnrolFlyingClubMemberMutation>();
        var request = new EnrolMemberAccountRequest { CustomerDetails = CreateUnder18Request(dateFormat).CustomerDetails };

        var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.EnrolFcMember(request));

        var error = Assert.Single(exception.Errors);
        Assert.Equal(Constants.Application.Under18StandardEnrolmentErrorCode, error.Code);
        Assert.Equal(Constants.Application.Under18StandardEnrolmentErrorMessage, error.Message);
        Assert.Equal("loginToCreateYoungFlyer", error.Extensions!["nextAction"]);
        Assert.False(provider.EntraEnrolmentCalled);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EnrolmentMutations_DelegateUnder18ApplicantWhenFeatureFlagIsDisabled(bool useEntraJourney)
    {
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "false");
        var provider = new RecordingEnrolmentProvider();
        var serviceProvider = CreateServiceProvider(provider);

        if (useEntraJourney)
        {
            await serviceProvider.GetRequiredService<EntraEnrolFlyingClubMemberMutation>()
                .EnrolFcMember(new EnrolMemberAccountRequest { CustomerDetails = CreateUnder18Request("yyyy-MM-dd").CustomerDetails });
            Assert.True(provider.EntraEnrolmentCalled);
            return;
        }

        await serviceProvider.GetRequiredService<EnrolFlyingClubMemberMutation>().EnrolFcMember(CreateUnder18Request("yyyy-MM-dd"));
        Assert.True(provider.StandardEnrolmentCalled);
    }

    [Theory]
    [InlineData("1972-08-17", "1972-08-17")]
    [InlineData("08/17/1972", "1972-08-17")]
    [InlineData("08/09/1972", "1972-08-09")]
    public async Task EnrolFcMember_DelegatesWithIsoDateOfBirth(string dateOfBirth, string expectedDateOfBirth)
    {
        var provider = new RecordingEnrolmentProvider();
        var mutation = CreateServiceProvider(provider).GetRequiredService<EnrolFlyingClubMemberMutation>();
        var request = new CreateCustomerAccountRequest
        {
            CustomerDetails = new CustomerAccountDetails { DateOfBirth = dateOfBirth }
        };

        await mutation.EnrolFcMember(request);

        Assert.True(provider.StandardEnrolmentCalled);
        Assert.Equal(expectedDateOfBirth, provider.DelegatedDateOfBirth);
    }

    [Theory]
    [InlineData("1972-08-17", "1972-08-17")]
    [InlineData("08/17/1972", "1972-08-17")]
    [InlineData("08/09/1972", "1972-08-09")]
    public async Task EntraEnrolFcMember_DelegatesWithIsoDateOfBirth(string dateOfBirth, string expectedDateOfBirth)
    {
        var provider = new RecordingEnrolmentProvider();
        var mutation = CreateServiceProvider(provider).GetRequiredService<EntraEnrolFlyingClubMemberMutation>();
        var request = new EnrolMemberAccountRequest
        {
            CustomerDetails = new CustomerAccountDetails { DateOfBirth = dateOfBirth }
        };

        await mutation.EnrolFcMember(request);

        Assert.True(provider.EntraEnrolmentCalled);
        Assert.Equal(expectedDateOfBirth, provider.DelegatedDateOfBirth);
    }

    public void Dispose()
    {
        ConfigReader.Set(Environment.YoungFlyerAgeLimit, "18");
        ConfigReader.Set(Environment.EnableUnder18StandardEnrolmentBlock, "false");
    }

    private static CreateCustomerAccountRequest CreateUnder18Request(string dateFormat) =>
        new()
        {
            CustomerDetails = new CustomerAccountDetails
            {
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-17).ToString(dateFormat, CultureInfo.InvariantCulture)
            }
        };

    private static void AssertUnder18PolicyError(GraphQLException exception)
    {
        var error = Assert.Single(exception.Errors);

        Assert.Equal(Constants.Application.Under18StandardEnrolmentErrorCode, error.Code);
        Assert.Equal(Constants.Application.Under18StandardEnrolmentErrorMessage, error.Message);
        Assert.Equal("loginToCreateYoungFlyer", error.Extensions!["nextAction"]);
    }

    private static IServiceProvider CreateServiceProvider(RecordingEnrolmentProvider provider) =>
        new ServiceCollection()
            .AddSingleton<IEnrolmentProvider>(provider)
            .AddSingleton<EnrolFlyingClubMemberMutation>()
            .AddSingleton<EntraEnrolFlyingClubMemberMutation>()
            .BuildServiceProvider();

    private sealed class RecordingEnrolmentProvider : IEnrolmentProvider
    {
        public string? DelegatedDateOfBirth { get; private set; }
        public bool StandardEnrolmentCalled { get; private set; }
        public bool EntraEnrolmentCalled { get; private set; }

        public Task<FlyingClubSignUpResponse> EnrolFcMember(CreateCustomerAccountRequest request)
        {
            StandardEnrolmentCalled = true;
            DelegatedDateOfBirth = request.CustomerDetails?.DateOfBirth;
            return Task.FromResult(new FlyingClubSignUpResponse());
        }

        public Task<FlyingClubSignUpResponse> EnrolYoungFlyerMember(CreateCustomerAccountRequest request) =>
            Task.FromResult(new FlyingClubSignUpResponse());

        public Task<EntraEnrolTokenResponse> EntraEnrolFcMember(CreateCustomerAccountRequest request)
        {
            EntraEnrolmentCalled = true;
            DelegatedDateOfBirth = request.CustomerDetails?.DateOfBirth;
            return Task.FromResult(new EntraEnrolTokenResponse());
        }
    }
}
