using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.UpdateOrderMS.Domain;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.UpdateOrderMS
{
    public class PublishOrderDocMSCommand : IRequest<string>
    {
        public UpdateOrderRequest UpdateOrderRequest { get; set; }

        public PublishOrderDocMSCommand(UpdateOrderRequest updateOrderRequest)
        {
            UpdateOrderRequest = updateOrderRequest;
        }
    }
}
