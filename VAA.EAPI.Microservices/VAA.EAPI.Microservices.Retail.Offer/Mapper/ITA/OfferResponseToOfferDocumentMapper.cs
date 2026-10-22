using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class OfferResponseToOfferDocumentMapper
    {
        public static OfferDocument RedisDocumentMapper(OfferResponse reshopResponse, string docId, string docType, ReshopRequest requestData)
        {
            return new OfferDocument
            {
                Id = docId,
                PartitionKey = docId,
                CreatedBy = Constants.Application.NdcChannel,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = docType,
                FareTypeRequest = new()
                {
                    FareTypeCode = reshopResponse?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = reshopResponse?.FareTypeRequest?.FareSubTypeCode
                },
                ShoppingResponseId = reshopResponse.ShoppingResponseId,
                OrderId = reshopResponse.OrderId,
                FlightOfferResponse = new()
                {
                    Pos = reshopResponse.Pos,
                    Parties = reshopResponse.Parties,
                    DataList = reshopResponse.DataList,
                    Offers = GetReshopOffers(reshopResponse.Offers, requestData)
                },
                FormOfPayments = requestData?.FormOfPayments
            };
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferDetails> GetReshopOffers(IEnumerable<EAPI.Domain.Virgin.Models.Offer.OfferDetails> offers, ReshopRequest requestData)
        {
            foreach (var offerService in offers)
            {
                offerService.OrderItemServices = GetServiceOrderItemList(requestData);               
            }
            return offers;
        }

        private static IEnumerable<OrderItemServices> GetServiceOrderItemList(ReshopRequest reshopRequest)
        {
            return reshopRequest?.ReshopOrder?.Select(item => new OrderItemServices
            {
                OrderItemId = item?.OrderItemId,
                RetainServices = item?.RetainServices?.ToArray(),
                DeleteServices = item?.DeleteServices?.ToArray(),
                ServiceBeneficiaries = item?.ServiceBeneficiaries?.ToArray(),
                OrderProductType = item?.OrderProductType
            });

        }
    }
}
