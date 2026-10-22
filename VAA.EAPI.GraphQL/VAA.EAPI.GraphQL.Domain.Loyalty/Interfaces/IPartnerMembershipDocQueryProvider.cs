using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IPartnerMembershipDocQueryProvider
    {
        Task<PartnerMembershipResponse> GetPartnerMembershipDoc(string CustomerID);
    }
}
