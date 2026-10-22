using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests;

public class UpdateMemberProfileMutationTests
{
    private readonly StubMemberProfileMutationProvider _memberProfileProvider = new();
    private readonly StubReferenceDataProvider _referenceDataProvider = new();
    private readonly StubPreferenceProvider _preferenceProvider = new();
    private readonly ICustomerProvider _customerProvider = new StubCustomerProvider();
    private readonly ILogger<UpdateMemberProfileMutation> _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<UpdateMemberProfileMutation>.Instance;

    [Fact]
    public void ConstructorShouldThrowNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UpdateMemberProfileMutation(null, _logger, _customerProvider, _referenceDataProvider, _preferenceProvider));
        Assert.Throws<ArgumentNullException>(() => new UpdateMemberProfileMutation(_memberProfileProvider, null, _customerProvider, _referenceDataProvider, _preferenceProvider));
        Assert.Throws<ArgumentNullException>(() => new UpdateMemberProfileMutation(_memberProfileProvider, _logger, null, _referenceDataProvider, _preferenceProvider));
        Assert.Throws<ArgumentNullException>(() => new UpdateMemberProfileMutation(_memberProfileProvider, _logger, _customerProvider, null, _preferenceProvider));
        Assert.Throws<ArgumentNullException>(() => new UpdateMemberProfileMutation(_memberProfileProvider, _logger, _customerProvider, _referenceDataProvider, null));
    }

    [Fact]
    public async Task UpdateMemberProfileShouldSuccess()
    {
        var request = TestDataProvider.GetTestData<VACMemberProfileUpdateRequest>("Loyalty//VACProfileUpdate//VACMemberProfileUpdateRequest");
        _referenceDataProvider.BlacklistedEmails = TestDataProvider.GetTestData<IEnumerable<BlacklistedEmailList>>("Account//BlackListedEmails");
        _memberProfileProvider.Response = Application.ProfileUpdateMSSuccess;
        _preferenceProvider.Response = TestDataProvider.GetTestData<MemberPreferenceUpdateResponse>("Loyalty//VACProfileUpdate//MemberPreferenceUpdateResponse");

        var mutation = new UpdateMemberProfileMutation(_memberProfileProvider, _logger, _customerProvider, _referenceDataProvider, _preferenceProvider);
        var result = await mutation.UpdateMemberProfile(request);

        Assert.Equal(Application.ProfileUpdateGraphQlSuccess, result);
    }

    private sealed class StubMemberProfileMutationProvider : IMemberProfileMutationProvider<string>
    {
        public string Response { get; set; }
        public Task<string> UpdateMemberProfile(MemberProfileUpdateRequest request) => Task.FromResult(Response);
    }

    private sealed class StubReferenceDataProvider : ILoyaltyReferenceDataProvider
    {
        public IEnumerable<BlacklistedEmailList> BlacklistedEmails { get; set; } = [];
        public Task<IEnumerable<AirportInfo>> GetAirports() => Task.FromResult<IEnumerable<AirportInfo>>([]);
        public Task<IEnumerable<AirlineInfo>> GetAirlines() => Task.FromResult<IEnumerable<AirlineInfo>>([]);
        public Task<IEnumerable<BlacklistedEmailList>> GetBlacklistedEmails() => Task.FromResult(BlacklistedEmails);
    }

    private sealed class StubCustomerProvider : ICustomerProvider
    {
        public Task<CustomerDTO> GetCustomer(string customerId, string memberNumber) => throw new InvalidOperationException("Customer lookup was not expected.");
    }

    private sealed class StubPreferenceProvider : ICustomerTransactionMutationProvider<MemberPreferenceUpdateResponse>
    {
        public MemberPreferenceUpdateResponse Response { get; set; }
        public Task<MemberPreferenceUpdateResponse> UpdateVACMemberPreference(MemberPreferenceUpdateRequest request) => Task.FromResult(Response);
        public Task<MemberPreferenceUpdateResponse> Mutate(PartnerMembershipRequest request) => throw new NotSupportedException();
        public Task<MemberPreferenceUpdateResponse> CreateTransaction(TransactionRequest request) => throw new NotSupportedException();
        public Task<MemberPreferenceUpdateResponse> CancelTransaction(TransactionRequest request) => throw new NotSupportedException();
    }
}
