
using System;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;

namespace VAA.EAPI.NDC.Offer.Mapper.OrderReshopNameCorrection
{
    public static class OrderReshopNameCorrectionRequestMapper
    {
         public static OrderReshopRequest MapNameCorrectionRq(IATA_OrderReshopRQ orderReshopRQ)
        {
            return new OrderReshopRequest()
            {
                BookingReferences = new EAPI.Domain.Virgin.Models.Order.BookingReference { RLOC = orderReshopRQ?.Request?.OrderRefID != null && orderReshopRQ.Request.OrderRefID.StartsWith(Constants.Application.Vs932TicketPrefix) ? orderReshopRQ.Request.OrderRefID.Substring(5, 6) : null },
                OrderId = orderReshopRQ?.Request?.OrderRefID,
                Parties = GetParties(orderReshopRQ),
                PaxNameChangeDetails = GetPaxNameChange(orderReshopRQ)
            };
        }
        public static Parties GetParties(IATA_OrderReshopRQ orderReshopRQ)
        {
            var distributionChain = (orderReshopRQ.DistributionChain.OrderBy(x => int.Parse(x.Ordinal))).Where(x => x.OrgRole != DistributionChainRoleCodeType.Carrier);

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

        private static PaxNameChange GetPaxNameChange(IATA_OrderReshopRQ orderReshopRQ)
        {
            var reshopOrder = ((orderReshopRQ.Request.UpdateOrder?.Item) as ReshopOrderType);
            var isNameCorrectionOrder = reshopOrder?.ReshopOrderChoice?.Item as UpdatePaxNameType;

            return new PaxNameChange()
            {
                GivenName = isNameCorrectionOrder?.GivenName.First(),
                PaxRefID = isNameCorrectionOrder?.PaxRefID.First(),
                Surname = isNameCorrectionOrder?.Surname
            };
        }
    }
}
