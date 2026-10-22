using VAA.EAPI.Domain.Models.CosmosGraph;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CosmosGraphModel
{
    public class FlightSegment : GraphObject
    {
        public string MarketingCarrier { get; set; }

        public string MarketingFlightNumber { get; set; }
        public string MarketingFlightSuffix { get; set; }
        public string MarketingAirlineName { get; set; }

        public string OperatingCarrier { get; set; }
        public string OperatingFlightSuffix { get; set; }
        public string OperatingAirlineName { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string DepartureAirportCodeIATA { get; set; }
        public string DepartureAirportCodeICAO { get; set; }
        public string DepartureAirportName { get; set; }
        public string DepartureCityCode { get; set; }
        public string DepartureCityName { get; set; }
        public string DepartureCountryCode { get; set; }
        public string DepartureCountryName { get; set; }
        public string ArrivalAirportCodeIATA { get; set; }
        public string ArrivalAirportCodeICAO { get; set; }
        public string ArrivalAirportName { get; set; }
        public string ArrivalCityCode { get; set; }
        public string ArrivalCityName { get; set; }
        public string ArrivalCountryCode { get; set; }
        public string ArrivalCountryName { get; set; }
        public string ScheduledDepartureLocalDateTime { get; set; }
        public string ScheduledDepartureUtcDateTime { get; set; }
        public string ScheduledArrivalLocalDateTime { get; set; }
        public string ScheduledArrivalUtcDateTime { get; set; }


        public override PropertyValue GetProperties()
        {
            PropertyValue propertyValue = new PropertyValue();
            propertyValue.Properties.Add("marketingCarrier", MarketingCarrier);
            propertyValue.Properties.Add("marketingFlightNumber", MarketingFlightNumber);
            propertyValue.Properties.Add("marketingFlightSuffix", MarketingFlightSuffix);
            propertyValue.Properties.Add("marketingAirlineName", MarketingAirlineName);
            propertyValue.Properties.Add("operatingCarrier", OperatingCarrier);
            propertyValue.Properties.Add("operatingFlightSuffix", OperatingFlightSuffix);
            propertyValue.Properties.Add("operatingAirlineName", OperatingAirlineName);
            propertyValue.Properties.Add("operatingFlightNumber", OperatingFlightNumber);
            propertyValue.Properties.Add("departureAirportCodeIATA", DepartureAirportCodeIATA);
            propertyValue.Properties.Add("departureAirportCodeICAO", DepartureAirportCodeICAO);
            propertyValue.Properties.Add("departureAirportName", DepartureAirportName);
            propertyValue.Properties.Add("departureCityCode", DepartureCityCode);
            propertyValue.Properties.Add("departureCityName", DepartureCityName);
            propertyValue.Properties.Add("departureCountryCode", DepartureCountryCode);
            propertyValue.Properties.Add("departureCountryName", DepartureCountryName);
            propertyValue.Properties.Add("arrivalAirportCodeIATA", ArrivalAirportCodeIATA);
            propertyValue.Properties.Add("arrivalAirportCodeICAO", ArrivalAirportCodeICAO);
            propertyValue.Properties.Add("arrivalAirportName", ArrivalAirportName);
            propertyValue.Properties.Add("arrivalCityCode", ArrivalCityCode);
            propertyValue.Properties.Add("arrivalCityName", ArrivalCityName);
            propertyValue.Properties.Add("arrivalCountryCode", ArrivalCountryCode);
            propertyValue.Properties.Add("arrivalCountryName", ArrivalCountryName);
            propertyValue.Properties.Add("scheduledDepartureLocalDateTime", ScheduledDepartureLocalDateTime);
            propertyValue.Properties.Add("scheduledDepartureUtcDateTime", ScheduledDepartureUtcDateTime);
            propertyValue.Properties.Add("scheduledArrivalLocalDateTime", ScheduledArrivalLocalDateTime);
            propertyValue.Properties.Add("scheduledArrivalUtcDateTime", ScheduledArrivalUtcDateTime);
            propertyValue.Properties.Add("documentType", "FlightSegment");
            propertyValue.Properties.Add("documentVersion", "1.0");
            return propertyValue;
        }
    }
}
