using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IMemberProfileMutationProvider<T>
    {
        public Task<string> UpdateMemberProfile(MemberProfileUpdateRequest updateMemberProfileRequest);

    }
}
