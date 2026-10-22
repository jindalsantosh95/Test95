using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
{
    public class AirportList
    {
        [JsonProperty(PropertyName = "ID")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "IATA_AIRPORT_CODE")]
        public string AirportCode { get; set; }
        [JsonProperty(PropertyName = "AIRPORT_NAME")]
        public string AirportName { get; set; }
        [JsonProperty(PropertyName = "LOCATION_MAPPINGID")]
        public string LocationMappingID { get; set; }

        public static explicit operator AirportList(AirportListDBEntity airportListDbEntity)
        {
            return new AirportList()
            {
                AirportCode = airportListDbEntity.AirportCode,
                AirportName = airportListDbEntity.AirportName,
                LocationMappingID = airportListDbEntity.LocationMappingID,
                ID = airportListDbEntity.ID
            };
        }

    }
}
