using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Services.FlightServicesMS;

namespace VAA.EAPI.NDC.Offer.Mapper.FlightServiceMS
{
    public static class FligthServiceRQmapper
    {
        public static FlightServicesMSCommand FlightServiceRequestMapper(Application.Query.ServiceList.ServiceListQuery request, OrderDocument orderDocument, Infrastructure.Model.OfferMS.OfferResponse offerDocument)
        {

            var corporateAccount = new CorporateAccount();

            if (orderDocument?.Parties != null)
            {
                corporateAccount.AccountName = orderDocument.Parties.CorporateAccount?.AccountName;
                corporateAccount.AgencyID = orderDocument.Parties.CorporateAccount?.AgencyID;
                corporateAccount.CorporateCode = orderDocument.Parties.CorporateAccount?.CorporateCode;
            }

            if (offerDocument?.Offers?.ToList().Count > 0)
            {
                corporateAccount = offerDocument?.Parties.CorporateAccount;
            }

            return new FlightServicesMSCommand()
            {
                Parties = GetParties(request.ServiceListRQ.DistributionChain, offerDocument?.Parties?.CorporateAccount),
                OrderId = orderDocument?.OrderNumber?.OrderId ?? null,
                OfferId = offerDocument?.Offers?.Count() > 0 ? offerDocument?.Offers.First(offer => offer.Selected).OfferId : null
            };
        }

        public static Parties GetParties(DistributionChainLinkType[] distributionChains, CorporateAccount corporateAccount)
        {
            var sortedDistributionChain = distributionChains.OrderBy(w => Convert.ToInt32(w?.Ordinal)).ToArray();
            if (sortedDistributionChain.Length == 1)
            {
                IEnumerable<Participant> participantList = new[] {
                new Participant()
                {
                    AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                            Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                            PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), sortedDistributionChain?.FirstOrDefault().OrgRole.ToString(), true),
                            Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal
                }
                };

                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAccount
                };
            }
            else if (sortedDistributionChain.Length > 1)
            {
                IEnumerable<Participant> participantList = new[] {
                new Participant()
                {
                      AgencyID = sortedDistributionChain?.Last()?.ParticipatingOrg?.OrgID,
                      Name = sortedDistributionChain?.Last()?.ParticipatingOrg?.Name,
                      PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), sortedDistributionChain?.Last().OrgRole.ToString(), true),
                      Ordinal = sortedDistributionChain?.Last()?.Ordinal

                },
                new Participant()
                {
                      AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                      Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                      PartyType = (Participant.PartyTypeEnum ? ) Enum.Parse(typeof (Participant.PartyTypeEnum), sortedDistributionChain?.FirstOrDefault().OrgRole.ToString(), true),
                      Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal
                }
                };
                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAccount
                };
            }
            return null;
        }
    }
}
