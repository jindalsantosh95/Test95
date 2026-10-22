using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Location
{
    public class LocationMapDbEntity : TableEntity
    {
        public string IATACountryCode { get; set; }
        public string CountryName { get; set; }
        public string IATACityCode { get; set; }
        public string CityName { get; set; }
        public string CurrencyCode { get; set; }
        public string LocationCode { get; set; }
        public int ID { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            this.IATACountryCode = properties["IATA_COUNTRY_CODE"].StringValue;
            this.CountryName = properties["COUNTRY_NAME"].StringValue;
            this.IATACityCode = properties["IATA_CITY_CODE"].StringValue;
            this.CityName = properties["CITY_NAME"].StringValue;
            this.CurrencyCode = properties["CURRENCY_CODE"].StringValue;
            this.LocationCode = properties["LOCATION_CODE"].StringValue;
            this.ID = ID;
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = new Dictionary<string, EntityProperty>();
            properties.Add("IATA_COUNTRY_CODE", new EntityProperty(this.IATACountryCode));
            properties.Add("COUNTRY_NAME", new EntityProperty(this.CountryName));
            properties.Add("IATA_CITY_CODE", new EntityProperty(this.IATACityCode));
            properties.Add("CITY_NAME", new EntityProperty(this.CityName));
            properties.Add("CURRENCY_CODE", new EntityProperty(this.CurrencyCode));
            properties.Add("LOCATION_CODE", new EntityProperty(this.LocationCode));
            properties.Add("ID", new EntityProperty(this.ID));
            return properties;
        }
        
    }
}
