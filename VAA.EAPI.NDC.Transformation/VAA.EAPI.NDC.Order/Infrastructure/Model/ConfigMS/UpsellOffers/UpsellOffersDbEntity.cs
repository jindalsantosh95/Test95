using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.UpsellOffers
{
    public class UpsellOffersDbEntity : TableEntity
    {
        public string ChosenBrand { get; set; }
        public string EligibleUpsell { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            this.ChosenBrand = properties["Chosen_Brand"].StringValue;
            this.EligibleUpsell = properties["Eligible_Upsell"].StringValue;
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = new Dictionary<string, EntityProperty>();
            properties.Add("Chosen_Brand", new EntityProperty(this.ChosenBrand));
            properties.Add("Eligible_Upsell", new EntityProperty(this.EligibleUpsell));
            return properties;
        }
    }
}
