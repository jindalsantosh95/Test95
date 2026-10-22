using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class DeDupCheckCommand : IRequest<Result<Response<EnrolmentResponse>>>
    {
        public EnrolmentRequestCommand DeDupRequest { get; set; }
    }
}
