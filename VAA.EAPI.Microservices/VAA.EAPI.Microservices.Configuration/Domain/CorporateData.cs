using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class CorporateData
    {
        public string CbaClid { get; set; }
        public string CbiTicketDesignator { get; set; }
        public bool CbaVsActiveContract { get; set; }
        public string AccCorporateName { get; set; }
        public string CbiPartnerCarrierCode { get; set; }

        public static explicit operator CorporateData(CorporateDbEntity corporateDbEntity)
        {
            if (corporateDbEntity == null)
            {
                return null;
            }

            return new CorporateData
            {
                CbaClid = corporateDbEntity.CbaClid?.ToString(),
                CbiTicketDesignator = corporateDbEntity.CbiTicketDesignator,
                CbaVsActiveContract = corporateDbEntity.CbaVsActiveContract,
                AccCorporateName = corporateDbEntity.AccCorporateName,
                CbiPartnerCarrierCode = corporateDbEntity.CbiPartnerCarrierCode
            };
        }
    }
}