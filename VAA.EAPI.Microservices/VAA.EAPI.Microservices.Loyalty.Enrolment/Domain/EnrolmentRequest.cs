using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Channel;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain
{

    public class EnrolmentRequest : ChannelUser, IRequest<Result<Response<string>>>
    {
        public Customer CustomerDetails { get; set; }

    }
}
