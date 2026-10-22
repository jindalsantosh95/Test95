using System;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using AirShoppingRQ = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.AirShoppingRQ;
using SeatAvailabilityRQ = VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public class OfferRequestMapper
    {
        public static Parties GetParties(AirShoppingRQ.IATA_AirShoppingRQ airShoppingRQ)
        {
            var distributionChain = airShoppingRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal)).Where(x => x.OrgRole != AirShoppingRQ.DistributionChainRoleCodeType.Carrier);
            
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
                    },
                    CorporateAccount = GetCorporateAccount(airShoppingRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner)
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
                    },
                    CorporateAccount = GetCorporateAccount(airShoppingRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner)
                };
            }
            return null;
        }

        public static Parties GetPartiesFromSeatReq(SeatAvailabilityRQ.IATA_SeatAvailabilityRQ seatAvailabilityRQ)
        {
            var distributionChain = (seatAvailabilityRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal))).Where(x=> x.OrgRole != DistributionChainRoleCodeType.Carrier);

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
                    },
                    CorporateAccount = GetCorporateAccountSeatRQ(seatAvailabilityRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner)
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
                    },
                    CorporateAccount = GetCorporateAccountSeatRQ(seatAvailabilityRQ.Request?.OfferCriteria?.ProgramCriteria?.FirstOrDefault()?.ProgramOwner)
                };
            }
            return null;
        }

        private static CorporateAccount GetCorporateAccount(AirShoppingRQ.ProgramOwnerType programOwner)
        {
            return programOwner == null ?
              null : new CorporateAccount()
              {
                  AccountName = (programOwner.Item as AirShoppingRQ.OrgType).Name,
                  CorporateCode = (programOwner.Item as AirShoppingRQ.OrgType).OrgID
              };
        }

        private static CorporateAccount GetCorporateAccountSeatRQ(SeatAvailabilityRQ.ProgramOwnerType programOwner)
        {
            return programOwner == null ?
              null : new CorporateAccount()
              {
                  AccountName = (programOwner.Item as SeatAvailabilityRQ.OrgType).Name,
                  CorporateCode = (programOwner.Item as SeatAvailabilityRQ.OrgType).OrgID
              };
        }
    }
}
