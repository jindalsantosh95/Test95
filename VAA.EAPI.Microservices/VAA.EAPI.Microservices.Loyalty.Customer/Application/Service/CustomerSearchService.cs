using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Service
{
    public class CustomerSearchService : ICustomerSearchService
    {
        private readonly ICustomerProfileService _customerProfileService;

        public CustomerSearchService(ICustomerProfileService customerProfileService)
        {
            _customerProfileService = customerProfileService ?? throw new ArgumentNullException(nameof(customerProfileService));
        }

        public async Task<Result<Response<IEnumerable<Customers>>>> GetCustomerProfileDetails(CustomerProfileRequest customerProfileRequest)
        {
            return Result.Success(new Response<IEnumerable<Customers>> { Status = Status.OK, Value = await _customerProfileService.GetCustomerDetails(customerProfileRequest) });
        }
    }
}
