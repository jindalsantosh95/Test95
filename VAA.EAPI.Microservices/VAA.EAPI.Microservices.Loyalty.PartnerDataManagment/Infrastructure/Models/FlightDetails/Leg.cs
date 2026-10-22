using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails
{
    public class Leg
    {
        [JsonProperty("Leg_Id")]
        public string LegId { get; set; }

        [JsonProperty("Origin_Airport_Code")]
        public string OriginAirportCode { get; set; }

        [JsonProperty("Origin_Airport_Name")]
        public string OriginAirportName { get; set; }

        [JsonProperty("Origin_X_Coordinate")]
        public string OriginXCoordinate { get; set; }

        [JsonProperty("Origin_Y_Coordinate")]
        public string OriginYCoordinate { get; set; }

        [JsonProperty("Origin_City_IATACode")]
        public string OriginCityIATACode { get; set; }

        [JsonProperty("Origin_City_Name")]
        public string OriginCityName { get; set; }

        [JsonProperty("Origin_Country_IATACode")]
        public string OriginCountryIATACode { get; set; }

        [JsonProperty("Origin_Country_Name")]
        public string OriginCountryName { get; set; }

        [JsonProperty("Destination_Airport_Code")]
        public string DestinationAirportCode { get; set; }

        [JsonProperty("Destination_Airport_Name")]
        public string DestinationAirportName { get; set; }

        [JsonProperty("Destination_X_Coordinate")]
        public string DestinationXCoordinate { get; set; }

        [JsonProperty("Destination_Y_Coordinate")]
        public string DestinationYCoordinate { get; set; }

        [JsonProperty("Destination_City_IATACode")]
        public string DestinationCityIATACode { get; set; }

        [JsonProperty("Destination_City_Name")]
        public string DestinationCityName { get; set; }

        [JsonProperty("Destination_Country_IATACode")]
        public string DestinationCountryIATACode { get; set; }

        [JsonProperty("Destination_Country_Name ")]
        public string DestinationCountryName { get; set; }

        [JsonProperty("PD_STUS_SET_DTM")]
        public string PDSTUSSETDTM { get; set; }

        [JsonProperty("Departure_Status")]
        public string DepartureStatus { get; set; }

        [JsonProperty("Arrival_Status")]
        public string ArrivalStatus { get; set; }

        [JsonProperty("Flight_Cancelled")]
        public string FlightCancelled { get; set; }

        [JsonProperty("Flight_Deleted")]
        public string FlightDeleted { get; set; }

        [JsonProperty("Flight_Diverted")]
        public string FlightDiverted { get; set; }

        [JsonProperty("Flight_Created ")]
        public string FlightCreated { get; set; }

        [JsonProperty("Wheels_Up_Status")]
        public string WheelsUpStatus { get; set; }

        [JsonProperty("Wheels_Down_Status")]
        public string WheelsDownStatus { get; set; }
    }
}
