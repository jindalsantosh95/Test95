using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class EnrolVacCustomerCommand(CreateCustomerEntities enrolmentRequest, bool basicTypeIdentifier) : IRequest<Result<Unit>>
    {
        public CreateCustomerEntities VaCustomerDetails { get; set; } = enrolmentRequest;
        public bool IsBasic { get; set; } = basicTypeIdentifier;
    }
}
