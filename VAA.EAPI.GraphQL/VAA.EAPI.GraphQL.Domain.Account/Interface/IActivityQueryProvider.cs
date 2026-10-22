using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IActivityQueryProvider
    {
        Task<IEnumerable<ActivityDetail>> GetActivitiesQueryData(TokenB2CResponse claim, int page, int pageSize);
        Task<IEnumerable<ActivityDetail>> GetActivitiesQueryData(TokenB2CResponse claim, CustomerCustMS customer, FcMemberDetails memberDetails, int page, int pageSize);
        Task<VredStatementResponse> GetVredStatements(string memberNumber);
    }
}
