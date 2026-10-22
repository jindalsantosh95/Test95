using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class OrderReshopNameCorrectionResponseMapper
    {
        public static IATA_OrderReshopRS MapReshopResponse(OrderDocument orderDocument, OfferResponse OfferResponse, OrderReshopRequest orderReshopRQ , [Optional] System.Collections.Generic.List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings)
        {
            string paxRefId = orderReshopRQ?.PaxNameChangeDetails?.PaxRefID;
            string currencyCode = orderDocument?.PointOfSale?.CurrencyCode;

            List<ServiceDefinitionType> servDefinitionList = new();
            return new()
            {
                Items = new OrderReshopResponseType[1]
                {
                    new()
                    {
                        Order = new OrderKeyType
                        {
                            OrderID = OfferResponse.OrderId,
                            OwnerCode = Constants.Application.OWNERCODE
                        },
                        ReshopResults = new ReshopResultsType
                        {
                            Item = new ReshopOffersType
                            {
                                Offer = GetReshopOffers(OfferResponse, paxRefId, currencyCode)
                            }
                        },

                        DataLists = new()
                        {
                           ServiceDefinitionList = new ServiceDefinitionType[]
                             {
                                new ServiceDefinitionType
                                    {
                                         Name = Constants.Application.NameChangeServiceDefinitionType,
                                         OwnerCode = Constants.Application.OWNERCODE,
                                         ServiceDefinitionID = Constants.Application.NameChangeServiceDefinitionId

                                    }
                              }
                        },
                        Warning = (nDCWarnings?.Any() ?? false) ? GetWarnings(nDCWarnings) : null
                    }
                }
            };
        }
        private static WarningType[] GetWarnings(List<NDCWarning> nDCWarnings)
        {
            return nDCWarnings.Select(x => new WarningType()
            {
                Code = Convert.ToString(x.Code),
                DescText = x.ShortText,
                LangCode = Constants.Application.LangCode,
                TypeCode = x.Type,
                TagText = x.TagText
            }).ToArray();
        }

        private static ChangedOfferType[] GetReshopOffers(OfferResponse offerResponse, string paxRefId, string currencyCode)
        {
            return offerResponse.Offers.Select(x => new ChangedOfferType
            {
                OfferExpirationTimeLimitDateTime = Convert.ToDateTime(x.OfferExpirationTimeLimit).ToUniversalTime(),
                OfferExpirationTimeLimitDateTimeSpecified = true,
                OfferID = x.OfferId,
                OfferItem = GetOfferItem(offerResponse, paxRefId, currencyCode),
                OwnerCode = Constants.Application.OWNERCODE

            }).ToArray();
        }

        private static NameChangeOfferItemType[] GetOfferItem(OfferResponse offerResponse, string paxRefId, string currencyCode)
        {
            var response = offerResponse.Offers.FirstOrDefault().OfferItems.First();
            var nameChangeOfferItems = new List<NameChangeOfferItemType>();

            var nameChangeOfferItem = new NameChangeOfferItemType
            {               
                MandatoryInd = true,
                MandatoryIndSpecified = true,
                NameChangeService = new NameChangeServiceType[]
                {
                     new NameChangeServiceType
                        {
                             PaxRefID = paxRefId,
                             ServiceDefinitionRefID = Constants.Application.NameChangeServiceDefinitionId,
                             ServiceID = Constants.Application.NameChangeServiceId,
                        }
                },
                OfferItemID = response.OfferItemId,
                PaxNameChange = new PaxNameChangeType
                {
                    Surname = response.PaxNameChangeDetails.Surname,
                    PaxRefID = response.PaxNameChangeDetails.PaxRefID,
                    GivenName = new string[] { response.PaxNameChangeDetails.GivenName }
                },
                Price = new PriceType
                {
                    TotalAmount = new AmountType
                    {
                        Value = Constants.Application.NameChangeTotalAmount,
                        CurCode = currencyCode
                    }
                }
            };

            nameChangeOfferItems.Add(nameChangeOfferItem);
            return nameChangeOfferItems.ToArray();
        }
    }
}