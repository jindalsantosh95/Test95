using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.NDC.Offer.Application.Query.OfferPrice;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.OfferPriceMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;

namespace VAA.EAPI.NDC.Offer.Mapper.OfferPrice
{
    public class OfferPriceRQMapper
    {
        public static OfferPriceMicroserviceCommand MapOfferPriceRequest(OfferPriceQuery offerPriceQuery, SelectedOfferListType pricedOffer, bool isCreditCardAccepted, IGenericDataMapper genericDataMapper, OfferResponse orderDoc, IEnumerable<UpsellOffersData> upsellOffersConfigRS, string accountCode)
        {
            if (orderDoc.Parties != null)
            {
                orderDoc.Parties.Participants.First().IsCreditCardAccepted = isCreditCardAccepted;
                orderDoc.Parties.Participants.First().AccountCode = accountCode;
            }

            OfferPriceMicroserviceCommand offerPriceMicroserviceCommand = new()
            {
                Pos = orderDoc?.Pos,
                Parties = orderDoc?.Parties ?? GetParties(offerPriceQuery.OfferPriceRQ?.DistributionChain, orderDoc.Parties.CorporateAccount, isCreditCardAccepted, accountCode),
                RepriceQueries = new()
                {
                    SearchCriteria = new()
                    {
                        CheckInBaggageAllowance = true,
                        CarryOnBaggageAllowance = true,
                        FareFamilies = GetFareFamilies(genericDataMapper, pricedOffer?.SelectedOffer?.First().OfferRefID, orderDoc, upsellOffersConfigRS)
                    }
                },
                PricedOffer = new()
                {
                    SelectedOffer = GetSelectedOffer(pricedOffer?.SelectedOffer, orderDoc)
                },
                CustomerDetails = offerPriceQuery.OfferPriceRQ?.Request?.DataLists?.PaxList.Select(pax => new Customer()
                {
                    Ptc = pax?.PTC == Constants.Application.ChildPTCCode ? Constants.Application.CNNPTCCode : pax?.PTC,
                    ItaPtc = orderDoc.DataList.CustomerDetails.Where(c => c.Ptc == (pax?.PTC == Constants.Application.ChildPTCCode? Constants.Application.CNNPTCCode: pax?.PTC)).Select(c => c.ItaPtc ?? c.Ptc).FirstOrDefault(),
                    CustId = pax.PaxID
                })
            };

            return offerPriceMicroserviceCommand;
        }

        public static Parties GetParties(DistributionChainLinkType[] distributionChains, CorporateAccount corporateAccount, bool isCreditCardAccepted, string accountCode)
        {
            var sortedDistributionChain = (distributionChains.OrderBy(w => Convert.ToInt32(w?.Ordinal)).ToArray()).Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier);
            
            if (sortedDistributionChain.Count() == 1)
            {
                return new Parties()
                {
                    Participants = new Participant[]
                    {
                        new()
                        {
                            AgencyID = sortedDistributionChain.First()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain.First()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), sortedDistributionChain.First().OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain.First()?.Ordinal,
                            IsCreditCardAccepted = isCreditCardAccepted,
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
                            AgencyID = sortedDistributionChain.Last().ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain.Last().ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), sortedDistributionChain.Last().OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain.Last().Ordinal
                        },
                        new()
                        {
                            AgencyID = sortedDistributionChain.First()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain.First()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), sortedDistributionChain.First()?.OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain.First()?.Ordinal
                        }
                    },
                    CorporateAccount = corporateAccount
                };
            }
            return null;
        }

        private static SelectedOffer GetSelectedOffer(SelectedOfferType[] selectedOfferList, OfferResponse offerDoc)
        {
            SelectedOffer selectedOffer = new();
            List<PricedOfferItem> pricedOfferItems = new();
            foreach (var offer in selectedOfferList)
            {
                selectedOffer.OfferRefID = offer.OfferRefID;
                selectedOffer.ShoppingRefID = offerDoc.ShoppingResponseId;
                foreach (var offerItem in offer.SelectedOfferItem)
                {
                    PricedOfferItem pricedOfferItem = new()
                    {
                        OfferItemId = offerItem.OfferItemRefID
                    };
                    if (offerItem?.PaxRefID != null)
                    {
                        pricedOfferItem.ServiceBeneficiaries = offerItem.PaxRefID;
                    }
                    pricedOfferItems.Add(pricedOfferItem);
                }
                selectedOffer.OfferItems = pricedOfferItems;
            }
            return selectedOffer;
        }

        private static IEnumerable<string> GetFareFamilies(IGenericDataMapper genericDataMapper, string selectedOfferId, OfferResponse orderDoc, IEnumerable<UpsellOffersData> upsellOffersConfigRS)
        {
            if (upsellOffersConfigRS is null) return Enumerable.Empty<string>();

            return from offerItem in orderDoc.Offers.First(offer => offer.OfferId == selectedOfferId).OfferItems
                   from service in offerItem.Services
                   from priceClassRefId in service.PriceClassRefIds
                   from priceclass in orderDoc.DataList.PriceClass.Where(x => x.Id == priceClassRefId)
                   from upsell in upsellOffersConfigRS.Where(x => x.ChosenBrand == priceclass.ProductName && x.EligibleUpsell.Any())
                   from eligibleUpsell in upsell.EligibleUpsell
                   where eligibleUpsell.Brand != null
                   select genericDataMapper.GetITABrandName(eligibleUpsell.Brand);
        }
    }
}
