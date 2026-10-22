using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayLocation")]
    public class Location
    {
        public string Code { get; set; }    
        public string Type { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public string ResortCode { get; set; }
        public string ResortName { get; set; }
        public string RegionCode { get; set; }
        public string RegionName { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }        
        public Address Address { get; set; }
        public string LocationAddress { get; set; }
        public GeoCoordinates GeoLocation { get; set; }

        internal Location Clone()
        {
            return new Location
            {
                Code = this.Code,
                Type = this.Type,
                CityCode = this.CityCode,
                CityName = this.CityName,
                CountryCode = this.CountryCode,
                CountryName = this.CountryName,
                StateCode = this.StateCode,
                StateName = this.StateName,
                ResortCode = this.ResortCode,
                ResortName = this.ResortName,
                RegionCode = this.RegionCode,
                RegionName = this.RegionName,
                AirportCode = this.AirportCode,
                AirportName = this.AirportName,
                Address = this.Address?.Clone(),
                LocationAddress = this.LocationAddress,
                GeoLocation = this.GeoLocation?.Clone()
            };
        }

        public static explicit operator Location(Infrastructure.Models.HolidaysMS.Location source)
        {
           
            if (source == null)
                return null;

            return new()
            {
                Code = source?.Code,
                Type = source?.Type,
                CityName = source?.CityName,
                CityCode = source?.CityCode,
                CountryCode = source?.CountryCode,
                CountryName = source?.CountryName,
                AirportName = source?.AirportName,
                StateCode = source?.StateCode,
                StateName = source?.StateName,
                ResortCode = source?.ResortCode,
                ResortName = source?.ResortName,
                RegionCode = source?.RegionCode,
                RegionName = source?.RegionName,
                AirportCode = source?.AirportCode,
                LocationAddress = source?.Address,
            };
        }

        public static explicit operator Location((Infrastructure.Models.HolidaysMS.Location location, string address) source)
        {
            var (location, address) = source;
            if (location == null && address == null)
                return null;

            return new()
            {
                Code = location?.Code,
                Type = location?.Type,
                CityName = location?.CityName,
                CityCode = location?.CityCode,
                CountryCode = location?.CountryCode,
                CountryName = location?.CountryName,
                AirportName = location?.AirportName,
                StateCode = location?.StateCode,
                StateName = location?.StateName,
                ResortCode = location?.ResortCode,
                ResortName = location?.ResortName,
                RegionCode = location?.RegionCode,
                RegionName = location?.RegionName,
                AirportCode = location?.AirportCode,
                LocationAddress = location?.Address ?? address,
            };
        }
    }
}
