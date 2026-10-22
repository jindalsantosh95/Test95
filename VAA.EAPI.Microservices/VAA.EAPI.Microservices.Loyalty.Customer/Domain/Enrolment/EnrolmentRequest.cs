using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Channel;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment
{

    public partial class EnrolmentRequest : ChannelUser, IRequest<Result<Response<string>>>
    {
        public EnrolCustomerDetails CustomerDetails { get; set; }

    }
}
