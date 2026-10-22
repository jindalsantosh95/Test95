using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Mapper.OrderReshopNameCorrection
{
    public class OfferMSReshopNameCorrectionMapper
    {
        public static OrderReshopRequest MapRequest(OrderDocument orderDocument,OrderReshopRequest orderReshopRQ)
        {
            return new()
            {               
                Parties = orderDocument.Parties,
                OrderId = orderDocument?.OrderNumber.OrderId,
                PaxNameChangeDetails = orderReshopRQ?.PaxNameChangeDetails
            };
        }}
}
