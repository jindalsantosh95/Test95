using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper
{
    public class GenericDataMapper : IGenericDataMapper
    {
        private enum PTC
        {
            ADT, CNN, GBE, INF
        }

        private readonly Dictionary<string, string> brandNameMap = new()
        {
            { "Economy Delight", "ECONOMY-DELIGHT" },
            { "Economy Delight Flex", "ECON-DELIGHT-FLEXIBLE" },
            { "Economy Delight Light", "ECONOMY-DELIGHT-LIGHT" },
            { "Economy Light", "ECONOMY-LIGHT" },
            { "Economy Classic", "ECON" },
            { "Economy Classic Flex", "ECON-CLASSIC-FLEXIBLE" },
            { "Premium", "COMFORT-PLUS-PREMIUM-ECON" },
            { "Premium Flex", "COMFORT-PLUS-PREMIUM-FLEXIBLE" },
            { "Premium Light", "COMFORT-PLUS-PREMIUM-LIGHT" },
            { "Upper Class", "BUS-FIRST" },
            { "Upper Class Flex", "BUS-FIRST-FLEXIBLE" },
            { "Upper Class Light", "BUS-FIRST-LIGHT" }
        };

        private readonly Dictionary<string, string> operatingCarriersMap = new()
        {
            { "VS", "Virgin Atlantic Airways" },
            { "DL", "Delta Air Lines" },
            { "CA", "Air China"},
            { "SQ", "Singapore Airlines"},
            { "NZ", "Air New Zealand"},
            { "VA", "Virgin Australia" },
            { "AM", "Aeromexico"},
            { "AF", "Air France"},
            { "KL", "KLM"},
            { "WS", "WestJet"}
        };

        public string GetITABrandName(string vaaFareFamily)
        {
            string itaBrand = brandNameMap.ContainsKey(vaaFareFamily) ? brandNameMap[vaaFareFamily] : string.Empty;

            return itaBrand;
        }

        public string GetOperatingCarrierNameByCode(string carrierCode)
        {
            string operatingCarrierName = operatingCarriersMap.ContainsKey(carrierCode) ? operatingCarriersMap[carrierCode] : null;

            return operatingCarrierName;
        }

        public string ConvertNdcPtcToItaPtc(string ndcptc)
        {
            return ndcptc switch
            {
                "ADT" => nameof(PTC.ADT),
                "CHD" => nameof(PTC.CNN), // Convert NdcPtc To ItaPtc
                "INF" => nameof(PTC.INF),
                "GBE" => nameof(PTC.GBE),
                _=> ndcptc
            };
        }
    }
}
