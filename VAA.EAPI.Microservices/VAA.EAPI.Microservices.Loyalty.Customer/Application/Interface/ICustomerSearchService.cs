using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Interface
{
    public interface ICustomerSearchService
    {
        Task<Result<Response<IEnumerable<Customers>>>> GetCustomerProfileDetails(CustomerProfileRequest customerProfileRequest);
    }
}
