using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class FareFamilyListDbEntity : TableEntity
    {
        public string CabinCode { get; set; }

        public string ServiceTaxonomy { get; set; }

        public string FareTypeCode { get; set; }

        public string FareFamily { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            this.CabinCode = properties["CabinCode"].StringValue;
            this.ServiceTaxonomy = properties["ServiceTaxonomy"].StringValue;
            this.FareTypeCode = properties["FareTypeCode"].StringValue;
            this.FareFamily = properties["FareFamily"].StringValue;
            
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = new Dictionary<string, EntityProperty>();
            properties.Add("CabinCode", new EntityProperty(this.CabinCode));
            properties.Add("ServiceTaxonomy", new EntityProperty(this.ServiceTaxonomy));
            properties.Add("FareTypeCode", new EntityProperty(this.FareTypeCode));
            properties.Add("FareFamily", new EntityProperty(this.FareFamily));
            return properties;
        }
    }
}
