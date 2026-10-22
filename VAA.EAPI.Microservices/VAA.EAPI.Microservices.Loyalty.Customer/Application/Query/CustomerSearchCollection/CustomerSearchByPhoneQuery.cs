using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection
{
    public class CustomerSearchByPhoneQuery : IRequest<Result<Response<IEnumerable<Infrastructure.Entities.CustomerSearchCollection.CustomerSearch>>>>
    {
        public string PhoneNumber { get; set; }

        public CustomerSearchByPhoneQuery(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
    }
}
