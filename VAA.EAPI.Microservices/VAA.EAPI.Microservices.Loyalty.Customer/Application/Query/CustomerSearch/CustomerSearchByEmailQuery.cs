using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch
{
    public class CustomerSearchByEmailQuery : IRequest<Result<Response<CustomerTierInfo>>>
    {
        public string EmailID { get; set; }

        public CustomerSearchByEmailQuery(string emailId)
        {
            EmailID = emailId;
        }
    }
}
