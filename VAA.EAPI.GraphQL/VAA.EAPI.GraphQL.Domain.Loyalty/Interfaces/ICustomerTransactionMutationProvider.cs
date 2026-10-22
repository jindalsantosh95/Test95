using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface ICustomerTransactionMutationProvider<T>
    {
        Task<T> Mutate(PartnerMembershipRequest partnerMembership);
        Task<T> CreateTransaction(TransactionRequest txnRequest);
        Task<MemberPreferenceUpdateResponse> UpdateVACMemberPreference(MemberPreferenceUpdateRequest updateMemberPreferenceRequest);
        Task<T> CancelTransaction(TransactionRequest txnRequest);
    }
}
