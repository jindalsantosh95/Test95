using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Application.Commands.InflightFetchOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Validations.InflightOrders
{
    public interface IValidatePayload
    {
        public List<ErrorDetails> ValidateSearchOrderCommand(OrderSearchCommand orderSearchCommand, string guid);
    }
}
