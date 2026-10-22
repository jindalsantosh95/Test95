using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class ManageAccountUpdateResponseCommand : IRequest<Result<Response<ManageAccountUpdateResponse>>>
    {
        public ManageAccountUpdateResponseCommand(string correlationId, string updatyeType)
        {
            CorrelationId = correlationId;
            UpdateType = updatyeType;
        }
        public string CorrelationId { get; set; }
        public string UpdateType { get; set; }
    }
}
