using System;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Enums;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{
    public class NDCParty
    {
        public string PartyType { get; set; }
        public string PartyAirlineCode { get; set; }
        public string IATANumber { get; set; }
        public string PartyLegalName { get; set; }
        public string AgencyID { get; set; }
        public string PseudoCityID { get; set; }
        public string Ordinal { get; set; }

        public static explicit operator NDCParty(DistributionChainLinkType source)
        {
            return new NDCParty()
            {
                AgencyID = source?.ParticipatingOrg?.Name,
                IATANumber = source?.ParticipatingOrg?.OrgID,
                PartyLegalName = source?.ParticipatingOrg?.Name,
                Ordinal = source?.Ordinal,
                PartyType = source?.OrgRole != null ? Enum.Parse(typeof(DistributionChainRoleCodeEnum), (source.OrgRole).ToString(), true).ToString() : null
            };
        }
    }
}
