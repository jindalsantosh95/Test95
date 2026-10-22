using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerProfile;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface ICustomerServiceProvider
    {
        Task<List<CustomerProfileDetails>> GetCustomerDetailsByEmail(CustomerProfileRequest emailAddress);
        Task<CustomerEnrolmentResponse> EnrolVAAccount(CustomerEnrolmentRequest enrolmentRequest);

    }
}
