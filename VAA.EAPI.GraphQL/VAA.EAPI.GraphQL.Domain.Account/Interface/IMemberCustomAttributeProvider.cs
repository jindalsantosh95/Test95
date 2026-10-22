using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IMemberCustomAttributeProvider
    {
        Task<IEnumerable<MemberCustomAttributesAccount>> GetMemberCustomerAttributeData(string customerId);

    }
}
