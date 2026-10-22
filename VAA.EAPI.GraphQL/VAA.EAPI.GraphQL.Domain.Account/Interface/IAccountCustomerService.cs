using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IAccountCustomerService
    {
        Task<CustomerDTO> GetCustomer(string customerId, string memberNumber);
        Task<List<CustomerProfileDetails>> GetCustomerDetailsByEmail(CustomerProfileRequest request);
    }
}
