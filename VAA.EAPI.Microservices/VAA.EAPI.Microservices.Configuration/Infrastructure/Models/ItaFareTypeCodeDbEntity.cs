using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class ItaFareTypeCodeDbEntity : TableEntity
    {
        public string FareType { get; set; }
        public string FareTypeCode { get; set; }
        public string FareSubTypeCode { get; set; }
        public string AccountCode { get; set; }
        public string NdcPaxType { get; set; }
        public string ItaPaxType { get; set; }
    }
}
