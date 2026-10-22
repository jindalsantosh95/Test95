using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.FareFamily
{
    public class FareFamilyListDbEntity : TableEntity
    {
        public string CabinCode { get; set; }
        public string ServiceTaxonomy { get; set; }
        public string FareTypeCode { get; set; }
        public string FareFamily { get; set; }
        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            CabinCode = properties["CabinCode"].StringValue;
            ServiceTaxonomy = properties["ServiceTaxonomy"].StringValue;
            FareTypeCode = properties["FareTypeCode"].StringValue;
            FareFamily = properties["FareFamily"].StringValue;

        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = new Dictionary<string, EntityProperty>();
            properties.Add("CabinCode", new EntityProperty(CabinCode));
            properties.Add("ServiceTaxonomy", new EntityProperty(ServiceTaxonomy));
            properties.Add("FareTypeCode", new EntityProperty(FareTypeCode));
            properties.Add("FareFamily", new EntityProperty(FareFamily));
            return properties;
        }
    }
}
