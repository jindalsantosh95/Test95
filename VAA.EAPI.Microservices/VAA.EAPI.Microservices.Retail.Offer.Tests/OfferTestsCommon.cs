using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{
    public class OfferTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"ApplicationId", "ND"},
                {"DebugOptions", "Y"},
                {"TestLab", "TVTH"},
                {"RFCWindowInHours", "24"},
                {"OfferDBEndpoint", "https://avadeuncmdboffer.documents.azure.com:443/"},
                {"OfferDB", "OfferDB_Dev"},
                {"OfferDBCollection", "OfferCollection"},
                {"OfferExpiryLimitInMinutes", "20" },
                {"ItaServiceApi","revenue" },
                {"ItaServiceKey","JTX0CpysDxnaKmUYOrOdcu" },
                {"ItaServiceReshopKey","6767676767676" },
                {"ItaServiceVersion","142" },
                {"ItaDataset","production" },
                {"ItaQpx","cloudprod" },
                {"DocumentSize", "2000" },
                {"DaysDifferenceDepartureAndBooking", "4:0:0:0"},
                {"ThresholdDaysBeforeDeparture", "3:0:0:0"},
                {"ThresholdDaysAfterBooking","3:0:0:0"},
                {"HoursDifferenceDepartureAndBooking","0:4:0:0"},
                {"CabinBaggageText","The baggage information displayed applies to Virgin Atlantic {flight} codeshare flights.For partner airline baggage, please visit our partners website for further information"},
                {"PieceAllowance","PieceAllowance"},
                {"CabinBagAllowance","CabinBagAllowance"},
                { "ItaReshopApi", "reshop"},
                {"ItaReshopApiKey", "rAwSUoguycqMaVm0sMyyca"},
                {"ItaReshopApiVersion", "162"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
