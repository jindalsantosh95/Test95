using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class EnrolmentRequestCommand : IRequest<Result<Response<EnrolmentResponse>>>
    {
        public string Id { get; set; }
        public EnrolmentRequest Data { get; set; }
    }
}
