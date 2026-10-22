using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands
{
    public class UpdateOrderCommand: IRequest<Result<Response<string>>>
    {
        public UpdateOrderRequest UpdateOrderRequest { get; set; }

        public UpdateOrderCommand(UpdateOrderRequest updateOrderRequest)
        {
            UpdateOrderRequest = updateOrderRequest;
        }
    }
}
