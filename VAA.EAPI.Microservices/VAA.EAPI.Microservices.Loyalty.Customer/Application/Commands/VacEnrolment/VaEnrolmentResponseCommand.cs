using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VaEnrolmentResponseCommand : IRequest<Result<Response<VaEnrolmentResponseResponse>>>
    {
        public VaEnrolmentResponseCommand(string correlationId)
        {
            CorrelationId = correlationId;           
        }

        public string CorrelationId { get; set; }
       
    }
}
