using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.InflightProfile
{
    public class CustomerProfileCommand : IRequest<Result<Response<CustomerProfile>>>
    {
        public string CustomerId { get; set; }
        public string MemberStatus { get; set; }

        public CustomerProfileCommand(string customerId, string membrStatus)
        {
            CustomerId = customerId;
            MemberStatus = membrStatus;
        }
    }
}