using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Application.Query.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderQuoteRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderQuote;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using static VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote.Products;

namespace VAA.EAPI.NDC.Offer.Mapper.OrderQuote
{
    public class OrderQuoteRQMapper
    {
        public static Parties GetParties(IATA_OrderQuoteRQ orderQuoteRQ)
        {
            var distributionChain = orderQuoteRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal)).Where(x=>x.OrgRole != DistributionChainRoleCodeType.Carrier);

            if (distributionChain.Count() == 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = distributionChain.First()?.ParticipatingOrg?.OrgID,
                            Name = distributionChain.First()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.First().OrgRole.ToString(), true),
                            Ordinal = distributionChain.First()?.Ordinal
                        }
                    }
                };
            }
            else if (distributionChain.Count() > 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = distributionChain.Last().ParticipatingOrg?.OrgID,
                            Name = distributionChain.Last().ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.Last().OrgRole.ToString(), true),
                            Ordinal = distributionChain.Last().Ordinal
                        },
                        new()
                        {
                            AgencyID = distributionChain.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = distributionChain.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), distributionChain.FirstOrDefault()?.OrgRole.ToString(), true),
                            Ordinal = distributionChain.FirstOrDefault()?.Ordinal
                        }
                    }
                };
            }

            return null;
        }

        public static OrderRepriceQuery MapOrderQuoteRequestToVAJson(IATA_OrderQuoteRQ orderQuoteRQ, bool isCreditCardAccepted, OfferResponse offerResponse)
        {
            var orderQuoteRequest = new OrderRepriceQuery();
            orderQuoteRequest.Parties = MapOrderQuoteNDCParties(orderQuoteRQ?.DistributionChain , isCreditCardAccepted, null,null,null);

            if (orderQuoteRQ?.Payload?.SelectedOffers == null)
            {
                orderQuoteRequest.IsHeldBooking = true;
            }
            else
            {
                orderQuoteRequest.PricedOffer = new Domain.Model.VAA.OrderQuote.PricedOffer()
                {
                    SelectedOffer = GetPricedOffer(orderQuoteRQ?.Payload?.SelectedOffers)
                };
                orderQuoteRequest.CustomerDetails = GetCustomerDetails(orderQuoteRQ, offerResponse);
            }
            orderQuoteRequest.OrderId = orderQuoteRequest.IsHeldBooking ? orderQuoteRQ?.Payload?.RepricedOrder?.OrderID : orderQuoteRQ?.Payload?.ExistingOrder?.OrderID;
            orderQuoteRequest.OwnerCode = orderQuoteRequest.IsHeldBooking ? orderQuoteRQ?.Payload?.RepricedOrder?.OwnerCode : orderQuoteRQ?.Payload?.ExistingOrder?.OwnerCode;

            return orderQuoteRequest;
        }

        public static OrderQuoteRepriceQuery MapOrderQuoteRq(OrderQuoteQuery orderQuoteRQ, OrderRepriceQuery orderRepriceQuery, OfferResponse offerResponse, IEnumerable<UpsellOffersData> upsellOffersConfigResponse, bool isCreditCardAccepted, IGenericDataMapper genericDataMapper,string accountCode , CorporateAccount corporateAccount, bool isReshopDisrupted, OrderDocument orderDocument = null, bool isHoldBookingReshop = false)
        {
            OrderQuoteRepriceQuery eapiOrderQuote = new OrderQuoteRepriceQuery();

            var rePrice = new RepriceQueries();
            rePrice.SearchCriteria = new SearchCriteria()
            {
                CheckInBaggageAllowance = true,
                CarryOnBaggageAllowance = true,
                FareFamilies = GetFareFamiliesForQuote(genericDataMapper, orderRepriceQuery.PricedOffer.SelectedOffer.OfferRefID, offerResponse, upsellOffersConfigResponse)
            };
            eapiOrderQuote.RepriceQueries = rePrice;
            eapiOrderQuote.Pos = new Pos()
            {
                CityCode = offerResponse?.Pos?.CityCode,
                CountryCode = offerResponse?.Pos?.CountryCode,
                CurrencyCode = offerResponse?.Pos?.CurrencyCode,
            };
            eapiOrderQuote.IsReshopDisruption = isReshopDisrupted;
            eapiOrderQuote.PricedOffer = new Domain.Model.VAA.OrderQuote.PricedOffer()
            {
                SelectedOffer = new Domain.Model.VAA.OrderQuote.SelectedOffer()
                {
                    ShoppingRefID = offerResponse.ShoppingResponseId
                }
            };
            eapiOrderQuote.CustomerDetails = GetQuoteCustomerDetails(orderQuoteRQ.OrderQuoteRQ,offerResponse);
            eapiOrderQuote.Parties = MapOrderQuoteNDCParties(orderQuoteRQ?.OrderQuoteRQ.DistributionChain , isCreditCardAccepted, offerResponse.Parties.Participants.First().PccCode,accountCode,corporateAccount);
            eapiOrderQuote.SegmentDetails = orderDocument.DataList.SegmentDetails;
            eapiOrderQuote.AccountableDocumentRefs = orderDocument.OrderItems.Where(x => x.AccountableDocumentRefs != null && x.Services.Any(srv => srv.Product.ProductType == Product.ProductTypes.FlightEnum)).SelectMany(x => x.AccountableDocumentRefs);
            eapiOrderQuote.IsReshopHoldBooking = isHoldBookingReshop;
            var isOfferValid = offerResponse.Offers.Any(offers => offers.OfferId == orderRepriceQuery.PricedOffer?.SelectedOffer?.OfferRefID);

            if (isOfferValid)
            {
                var offerList = offerResponse.Offers.Where(offers => offers.OfferId == orderRepriceQuery.PricedOffer?.SelectedOffer?.OfferRefID);
                foreach (var offers in offerList)
                {
                    eapiOrderQuote.PricedOffer.SelectedOffer.OfferRefID = offers.OfferId;
                    var repricedquery = offers.OfferItems.Join(orderRepriceQuery.PricedOffer.SelectedOffer.OfferItems,
                                                    oI => oI.OfferItemId,
                                                    oIRed => oIRed.OfferItemId,
                                                    (oI, oIRed) => new { oI, oIRed });

                    var pricedOfferItems = repricedquery.Select(x => new Domain.Model.VAA.OrderQuote.PricedOfferItem()
                    {
                        OfferItemId = x.oI.OfferItemId,
                        ServiceBeneficiaries = x.oI.Services.FirstOrDefault()?.ServiceBeneficiaries,
                        Product = new Products()
                        {
                            ProductType = (ProductTypeEnum)x.oI.Services.FirstOrDefault()?.Product.ProductType.Value,
                            ProductName = offerResponse.DataList.PriceClass.LastOrDefault(s => s.Id == x.oI.Services.LastOrDefault()?.PriceClassRefIds.LastOrDefault())?.ProductName,
                            ProductOwner = x.oI.Services.FirstOrDefault()?.Product.ProductOwner,
                            ProductCode = x.oI.Services.FirstOrDefault()?.Product.ProductCode
                        }
                    });

                    eapiOrderQuote.PricedOffer.SelectedOffer.OfferItems = pricedOfferItems;

                    if (orderRepriceQuery.PricedOffer.SelectedOffer.OfferItems.FirstOrDefault().Product?.ProductName != null)
                    {
                        eapiOrderQuote.PricedOffer.SelectedOffer.FareFamily = genericDataMapper.GetITABrandName(orderRepriceQuery.PricedOffer.SelectedOffer.OfferItems.First().Product?.ProductName);
                    }
                }
            }

            return eapiOrderQuote;
        }

        private static Domain.Model.VAA.OrderQuote.SelectedOffer GetPricedOffer(SelectedOfferType[] selectedOfferList)
        {
            var sf = selectedOfferList?.First();

            var selectedOffer = new Domain.Model.VAA.OrderQuote.SelectedOffer
            {
                OfferRefID = sf.OfferRefID,
                OwnerCode = sf.OwnerCode,
                OfferItems = sf.SelectedOfferItem
                   .Select(item => new Domain.Model.VAA.OrderQuote.PricedOfferItem
                   {
                       OfferItemId = item.OfferItemRefID,
                       ServiceBeneficiaries = item.PaxRefID
                   })
            };

            return selectedOffer;

        }

        private static IEnumerable<CustomerDetail> GetCustomerDetails(IATA_OrderQuoteRQ orderQuoteRQ, OfferResponse offerResponse)
        {
            return orderQuoteRQ.Payload.PaxList.Select(pax => new CustomerDetail
            {
                CustId = pax.PaxID,
                Ptc = pax.PTC == Domain.Model.VAA.Constants.ChildPTCCode ? Domain.Model.VAA.Constants.CNNPTCCode : pax.PTC
            });
        }

        private static IEnumerable<VAA.EAPI.Domain.Virgin.Models.Customer.Customer> GetQuoteCustomerDetails(IATA_OrderQuoteRQ orderQuoteRQ, OfferResponse offerResponse)
        {
            var associatedAdults = new Dictionary<string, string>();
            var adults = orderQuoteRQ.Payload.PaxList.Where(x => x.PTC == Constants.Application.AdultPTCCode).ToList();

            if (orderQuoteRQ.Payload.PaxList.Any(x => x.PTC == Constants.Application.InfantPTCCode))
            {
                foreach (var inf in orderQuoteRQ.Payload.PaxList.Where(x => x.PTC == Constants.Application.InfantPTCCode))
                {
                    associatedAdults.Add($"{adults.First().PaxID}", inf.PaxID);
                    adults.RemoveAt(0);
                }
            }
            return orderQuoteRQ.Payload.PaxList.Select(pax =>
            {
                var matchedCustomers = offerResponse.DataList.CustomerDetails.Where(x => x.CustId == pax.PaxID);
                var itaPtcs = matchedCustomers.Any(c => c.ItaPtc == null) ? null: string.Join(",", matchedCustomers.Select(c => c.ItaPtc));

                return new VAA.EAPI.Domain.Virgin.Models.Customer.Customer
                {
                    CustId = pax.PaxID,
                    Ptc = pax.PTC == Domain.Model.VAA.Constants.ChildPTCCode ? Domain.Model.VAA.Constants.CNNPTCCode : pax.PTC,
                    ItaPtc = itaPtcs,
                    LinkedInfantId = pax.PTC == Constants.Application.AdultPTCCode ? associatedAdults.FirstOrDefault(x => x.Key == pax.PaxID).Value : null
                };
            });
        }


        private static Parties MapOrderQuoteNDCParties(DistributionChainLinkType[] distributionChain, bool isCreditCardAccepted, string pcc,string accountCode , CorporateAccount corporateAccount)
        {
            var distributionChainLink = JsonConvert.SerializeObject(distributionChain);
            DistributionChainLinkType[] distributionChainObj = JsonConvert.DeserializeObject<DistributionChainLinkType[]>(distributionChainLink);

            var sortedDistributionChain = distributionChainObj.OrderBy(w => Convert.ToInt32(w?.Ordinal)).ToArray().Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier); 
            if (sortedDistributionChain.Count() == 1)
            {

                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (sortedDistributionChain?.FirstOrDefault().OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                            Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal,
                            IsCreditCardAccepted = isCreditCardAccepted,
                            PccCode = pcc,
                            AccountCode = accountCode
                        }
                    },
                    CorporateAccount = corporateAccount
                };
            }
            else if (sortedDistributionChain.Count() > 1)
            {

                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                             AgencyID = sortedDistributionChain?.Last().ParticipatingOrg?.OrgID,
                             Name = sortedDistributionChain?.Last().ParticipatingOrg?.Name,
                             PartyType = (sortedDistributionChain?.Last().OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                             Ordinal = sortedDistributionChain?.Last().Ordinal,
                             PccCode = pcc,
                             IsCreditCardAccepted = isCreditCardAccepted,
                             AccountCode= accountCode
                        },
                        new()
                        {
                             AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                             Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                             PartyType = (sortedDistributionChain?.FirstOrDefault().OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                             Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal,
                             IsCreditCardAccepted = isCreditCardAccepted,
                             AccountCode= accountCode
                        }
                    },
                    CorporateAccount = corporateAccount
                };
            }

            return null;
        }

        private static IEnumerable<string> GetFareFamiliesForQuote(IGenericDataMapper genericDataMapper, string selectedOfferId, OfferResponse orderDoc, IEnumerable<UpsellOffersData> upsellOffersConfigRS)
        {
            if (upsellOffersConfigRS != null)
            {
                return from offerItem in orderDoc.Offers.First(offer => offer.OfferId == selectedOfferId).OfferItems
                       from service in offerItem.Services
                       from priceClassRefId in service.PriceClassRefIds
                       from priceclass in orderDoc.DataList.PriceClass.Where(x => x.Id == priceClassRefId)
                       from upsell in upsellOffersConfigRS.Where(x => x.ChosenBrand == priceclass.ProductName && x.EligibleUpsell.Any())
                       from eligibleUpsell in upsell.EligibleUpsell
                       where eligibleUpsell.Brand != null || upsell.ChosenBrand == Constants.Application.FareFamilyDictionary.Keys.Last()
                       select genericDataMapper.GetITABrandName(eligibleUpsell.Brand ?? upsell.ChosenBrand);
            }

            return Enumerable.Empty<string>();
        }
    }
}
