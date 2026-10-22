using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.CustProfile;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query.CustProfileMS
{
    public interface ICustomerProfileQuery
    {
        Task<Result<Response<CustomerProfileResponse>>> Handle(CustomerProfileRequest request, CancellationToken cancellationToken);
    }
}
