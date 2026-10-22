using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class ItaFareTypeCodeData
    {
        public string FareType { get; set; }
        public string FareTypeCode { get; set; }
        public string FareSubTypeCode { get; set; }
        public string AccountCode { get; set; }
        public string NdcPaxType { get; set; }
        public string ItaPaxType { get; set; }

        public static explicit operator ItaFareTypeCodeData(ItaFareTypeCodeDbEntity ItaFareTypeCodeListDbEntity)
        {
            return new ItaFareTypeCodeData()
            {
                FareType = ItaFareTypeCodeListDbEntity.FareType,
                FareTypeCode = ItaFareTypeCodeListDbEntity.FareTypeCode,
                FareSubTypeCode = ItaFareTypeCodeListDbEntity.FareSubTypeCode,
                AccountCode = ItaFareTypeCodeListDbEntity.AccountCode,
                NdcPaxType = ItaFareTypeCodeListDbEntity.NdcPaxType,
                ItaPaxType = ItaFareTypeCodeListDbEntity.ItaPaxType
            };
        }
    }
}
