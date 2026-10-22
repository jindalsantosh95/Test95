using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Mapper.ValidationMS
{
    public class ValidationMSRequestMapper
    {
        public static AgencyValidateRequest MapValidationRequest(Parties rqParties, bool checkTktAuthority = false)
        {
            var validationMSRequest = new AgencyValidateRequest()
            {
                Parties = new()
                {
                    CorporateAccount = rqParties.CorporateAccount,
                    Participants = GetParticipants(rqParties)
                }
            };

            validationMSRequest.Keytype = validationMSRequest.Parties?.Participants.Count() > 1 ? Constants.Application.KeyTypeIndirect : Constants.Application.KeyTypeDirect;

            if (checkTktAuthority)
            {
                validationMSRequest.CheckAgentTktAuthority = ConfigReader.Get<bool>(Constants.Application.CheckAgentTktAuthority);
            }
            return validationMSRequest;
        }

        public static IEnumerable<Participant> GetParticipants(Parties parties)
        {
            var newParties = parties.Participants.Where(x=>x.PartyType != Participant.PartyTypeEnum.Carrier);

            IEnumerable<Participant> participantList = new[] {new Participant()
                {
                    AgencyID = newParties?.Last().AgencyID,
                    Name = newParties?.Last().Name,
                    PartyType = newParties?.Last().PartyType
                }
            };

            if (newParties.Count() > 1)
            {
                participantList = participantList.Append(new Participant()
                {
                    AgencyID = newParties?.First().AgencyID,
                    Name = newParties?.First().Name,
                    PartyType = newParties?.First().PartyType
                });
            }

            return participantList;
        }

        public static Parties GetSeatParties(Domain.RequestResponseModels.SeatAvailabilityRQ.DistributionChainLinkType[] distributionChains, CorporateAccount corporateAccount)
        {
            var sortedDistributionChain = distributionChains.Where(x => x.OrgRole != Domain.RequestResponseModels.SeatAvailabilityRQ.DistributionChainRoleCodeType.Carrier).OrderBy(w => Convert.ToInt32(w?.Ordinal)).ToArray();
            if (sortedDistributionChain.Length == 1)
            {

                IEnumerable<Participant> participantList = new[] {
                new Participant()
                {
                     AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                     Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                     PartyType = (sortedDistributionChain?.FirstOrDefault().OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                     Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal
                }

                };
                var corporateAc = new CorporateAccount()
                {
                    AccountName = corporateAccount?.AccountName,
                    AgencyID = corporateAccount?.AgencyID,
                    CorporateCode = corporateAccount?.CorporateCode
                };

                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAc
                };
            }
            else if (sortedDistributionChain.Length > 1)
            {
                IEnumerable<Participant> participantList = sortedDistributionChain.Select(dC =>
                new Participant()
              {
                  AgencyID = dC.ParticipatingOrg?.OrgID,
                  Name = dC?.ParticipatingOrg?.Name,
                  PartyType = (dC.OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                  Ordinal = dC.Ordinal

              });
                var corporateAc = new CorporateAccount()
                {
                    AccountName = corporateAccount?.AccountName,
                    AgencyID = corporateAccount?.AgencyID,
                    CorporateCode = corporateAccount?.CorporateCode
                };

                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAc
                };
            }

            return null;
        }

        public static Parties GetServiceListParties(Domain.RequestResponseModels.ServiceListRQ.DistributionChainLinkType[] distributionChains, CorporateAccount corporateAccount)
        {
            var sortedDistributionChain = (distributionChains.Where(x => x.OrgRole != Domain.RequestResponseModels.ServiceListRQ.DistributionChainRoleCodeType.Carrier)).OrderBy(w => Convert.ToInt32(w?.Ordinal)).ToArray();
            if (sortedDistributionChain.Length == 1)
            {

                IEnumerable<Participant> participantList = new[] {
                new Participant()
                {
                     AgencyID = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.OrgID,
                     Name = sortedDistributionChain?.FirstOrDefault()?.ParticipatingOrg?.Name,
                     PartyType = (sortedDistributionChain?.FirstOrDefault().OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                     Ordinal = sortedDistributionChain?.FirstOrDefault()?.Ordinal
                }

                };
                var corporateAc = new CorporateAccount()
                {
                    AccountName = corporateAccount?.AccountName,
                    AgencyID = corporateAccount?.AgencyID,
                    CorporateCode = corporateAccount?.CorporateCode
                };

                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAc
                };
            }
            else if (sortedDistributionChain.Length > 1)
            {
                IEnumerable<Participant> participantList = sortedDistributionChain.Select(dC =>
                new Participant()
                {
                    AgencyID = dC.ParticipatingOrg?.OrgID,
                    Name = dC?.ParticipatingOrg?.Name,
                    PartyType = (dC.OrgRole.ToString() ?? string.Empty).Parse<Participant.PartyTypeEnum>(),
                    Ordinal = dC.Ordinal

                });
                var corporateAc = new CorporateAccount()
                {
                    AccountName = corporateAccount?.AccountName,
                    AgencyID = corporateAccount?.AgencyID,
                    CorporateCode = corporateAccount?.CorporateCode
                };

                return new Parties()
                {
                    Participants = participantList,
                    CorporateAccount = corporateAc
                };
            }

            return null;
        }

        public static AgencyValidateRequest MapValidationRQQuote(Parties offerJSON, CorporateAccount corporateAccount, bool checkTktAuthority = false)
        {
            AgencyValidateRequest validationRQ = new AgencyValidateRequest();
            Parties parties = new Parties();
            parties.Participants = GetParticipants(offerJSON);
            validationRQ.Parties = parties;
            if (corporateAccount != null)
            {
                validationRQ.Parties.CorporateAccount = new CorporateAccount()
                {
                    CorporateCode = corporateAccount?.CorporateCode,
                    AccountName = corporateAccount?.AccountName
                };
            }
            validationRQ.Keytype = validationRQ.Parties?.Participants.Count() > 1 ? Constants.Application.KeyTypeIndirect : Constants.Application.KeyTypeDirect;

            if (checkTktAuthority)
            {
                validationRQ.CheckAgentTktAuthority = ConfigReader.Get<bool>(Constants.Application.CheckAgentTktAuthority);
            }

            return validationRQ;
        }

        public static AgencyValidateRequest MapValidationRQReshop(Parties offerJSON, CorporateAccount corporateAccount)
        {
            AgencyValidateRequest validationRQ = new AgencyValidateRequest();
            Parties parties = new Parties();
            parties.Participants = GetParticipants(offerJSON);
            validationRQ.Parties = parties;

            if (corporateAccount != null)
            {
                validationRQ.Parties.CorporateAccount = new CorporateAccount()
                {
                    CorporateCode = corporateAccount?.CorporateCode,
                    AccountName = corporateAccount?.AccountName
                };
            }

            _ = validationRQ?.Parties?.Participants?.Count() > 1 ? validationRQ.Keytype = Domain.Model.VAA.Constants.KeyTypeIndirect : validationRQ.Keytype = Domain.Model.VAA.Constants.KeyTypeDirect;

            return validationRQ;
        }
    }
}
