using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class BspMarketCurrencyData
    {
        public string BSPMarket { get; set; }
        public string CountryCode { get; set; }
        public string LocalCurrencyCode { get; set; }
        public string DualCurrencyCode { get; set; }

        public static explicit operator BspMarketCurrencyData(BspMarketCurrencyDbEntity bspMarketCurrencyListDbEntity)
        {
            return new BspMarketCurrencyData()
            {
                BSPMarket = bspMarketCurrencyListDbEntity.BSPMarket,
                CountryCode = bspMarketCurrencyListDbEntity.CountryCode,
                LocalCurrencyCode = bspMarketCurrencyListDbEntity.LocalCurrencyCode,
                DualCurrencyCode = bspMarketCurrencyListDbEntity.DualCurrencyCode
            };
        }
    }
}
