using System;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.EventData
{
    public class Partner
    {
        public string PartnerId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubType { get; set; }
        public int SkyTeamPartnerFlag { get; set; }

        public static explicit operator Partner(PartnerProfileDetails partner)
        {
            if (partner == null) throw new ArgumentNullException(nameof(partner));

            return new Partner()
            {
                PartnerId = partner.RowId,
                PartnerCode = partner.PartnerCode,
                PartnerName = partner.Name,
                PartnerType = partner.PartnerType,
                PartnerSubType = partner.PartnerSubtype,
                SkyTeamPartnerFlag = partner.SkyTeamPartnerFlag
            };
        }
    }
}
