using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Mapper
{
    public static class CabinMapper
    {
        private static readonly char[] Separator = [','];

        private static readonly HashSet<string> PremiumEconomyCabins;
        private static readonly HashSet<string> UpperClassConfigCabins;
        private static readonly HashSet<string> EconomyCompleteCabins;

        static CabinMapper()
        {
            PremiumEconomyCabins = ParseConfigToSet("PremiumEconomyConfigKey");
            UpperClassConfigCabins = ParseConfigToSet("UpperClassConfigKey");
            EconomyCompleteCabins = ParseConfigToSet("EconomyCompleteKey");
        }

        private static HashSet<string> ParseConfigToSet(string key)
        {
            string configValue = ConfigReader.Get(key);

            if (string.IsNullOrWhiteSpace(configValue))
                return null;

            // Split, trim, filter, and use case-insensitive comparer
            var tokens = configValue
                .Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));

            return new HashSet<string>(tokens, StringComparer.OrdinalIgnoreCase);
        }

        public static string MapToCabin(string bookingClass)
        {
            string cabin = bookingClass.Trim();

            if (PremiumEconomyCabins!=null && PremiumEconomyCabins.Contains(cabin)) return "W";
            if (UpperClassConfigCabins!=null && UpperClassConfigCabins.Contains(cabin)) return "J";
            if (EconomyCompleteCabins!=null && EconomyCompleteCabins.Contains(cabin)) return "Y";

            return cabin.ToUpper();
        }
    }
}


