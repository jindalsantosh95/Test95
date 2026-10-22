using Newtonsoft.Json.Linq;
using System;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public sealed class CarbonFootprint
    {
        public static explicit operator CarbonFootprint(JToken source)
        {
            if (source["response"].HasValues)
            {

                return new()
                {
                    Origin = source["request"]["origin"].ToString(),
                    Destination = source["request"]["destination"].ToString(),
                    FlightNumber = source["request"]["flightNumber"]?.ToString(),
                    Date = source["request"]["date"].HasValues ? source["request"]["date"].ToString().Parse<DateTime>().ToString(Constants.Application.FlightDateFormat) : null,
                    RequestId = source["request"]["requestId"]?.ToString(),
                    SourceMethodology = source["response"]["SourceMethodology"]?.ToString(),
                    SourceQuality = source["response"]["SourceQuality"]?.ToString(),
                    KilosCo2 = !source["response"]["KilosCo2"].ToString().IsNullOrEmpty() ? source["response"]["KilosCo2"].ToString().Parse<decimal>() : default,
                    KilosCo2e = !source["response"]["KilosCo2e"].ToString().IsNullOrEmpty() ? source["response"]["KilosCo2e"].ToString().Parse<decimal>() : default,
                    LbsCo2 = !source["response"]["LbsCo2"].ToString().IsNullOrEmpty() ? source["response"]["LbsCo2"].ToString().Parse<decimal>() : default,
                    LbsCo2e = !source["response"]["LbsCo2e"].ToString().IsNullOrEmpty() ? source["response"]["LbsCo2e"].ToString().Parse<decimal>() : default,
                    Source = source["response"]["Source"]?.ToString(),
                    StatusCode = source["status"]?.ToString(),
                    TravelClass = source["request"]["travelClass"]?.ToString()
                };
            }
            else
            {
                return new()
                {
                    Origin = source["request"]["origin"].ToString(),
                    Destination = source["request"]["destination"].ToString(),
                    FlightNumber = source["request"]["flightNumber"]?.ToString(),
                    Date = source["request"]["date"].HasValues ? source["request"]["date"].ToString().Parse<DateTime>().ToString(Constants.Application.FlightDateFormat) : null,
                    RequestId = source["request"]["requestId"]?.ToString(),
                    StatusCode = source["status"].ToString(),
                    ErrorMessage = source["error"]?.ToString(),
                    TravelClass = source["request"]["travelClass"].ToString()
                };
            }
        }

        public string Origin { get; set; }
        public string Destination { get; set; }
        public string FlightNumber { get; set; }
        public string Date { get; set; }
        public string RequestId { get; set; }
        public string SourceMethodology { get; set; }
        public string SourceQuality { get; set; }
        public decimal KilosCo2 { get; set; }
        public decimal KilosCo2e { get; set; }
        public decimal LbsCo2 { get; set; }
        public decimal LbsCo2e { get; set; }
        public string Source { get; set; }
        public string StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string TravelClass { get; set; }
    }
}
