using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders
{
    public interface IValidatePayloadAssociateFlights
    {
        public List<ErrorDetails> ValidateOrderAssociationRequestCommand(OrderAssociationRequest orderAssociationRequest, string guid);

    }
}
