using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Airport
{
    public class AirportListDBEntity : TableEntity
    {
        public int? ID { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string LocationMappingID { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            AirportCode = properties["IATA_AIRPORT_CODE"].StringValue;
            AirportName = properties["AIRPORT_NAME"].StringValue;
            LocationMappingID = properties["LOCATION_MAPPINGID"].StringValue;
            ID = properties["ID"].Int32Value;
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = new Dictionary<string, EntityProperty>();

            properties.Add("IATA_AIRPORT_CODE", new EntityProperty(AirportCode));
            properties.Add("AIRPORT_NAME", new EntityProperty(AirportName));
            properties.Add("LOCATION_MAPPINGID", new EntityProperty(LocationMappingID));
            properties.Add("ID", new EntityProperty(ID));

            return properties;
        }
    }
}
