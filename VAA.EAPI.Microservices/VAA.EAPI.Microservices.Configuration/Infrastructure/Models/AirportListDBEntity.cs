using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class AirportListDBEntity : TableEntity
    {
        public int? ID { get; set; }
        public string AirportCode { get; set; }
        public string AirportName { get; set; }
        public string LocationMappingID { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            this.AirportCode = properties["IATA_AIRPORT_CODE"].StringValue;
            this.AirportName = properties["AIRPORT_NAME"].StringValue;
            this.LocationMappingID = properties["LOCATION_MAPPINGID"].StringValue;
            this.ID = properties["ID"].Int32Value;
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = new Dictionary<string, EntityProperty>();

            properties.Add("IATA_AIRPORT_CODE", new EntityProperty(this.AirportCode));
            properties.Add("AIRPORT_NAME", new EntityProperty(this.AirportName));
            properties.Add("LOCATION_MAPPINGID", new EntityProperty(this.LocationMappingID));
            properties.Add("ID", new EntityProperty(this.ID));

            return properties;
        }
    }
}
