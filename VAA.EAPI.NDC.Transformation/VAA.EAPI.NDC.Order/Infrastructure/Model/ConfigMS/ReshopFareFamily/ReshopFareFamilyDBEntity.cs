using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.ReshopFareFamily
{
    public class ReshopFareFamilyDBEntity : TableEntity
    {
        public string CABINCODE { get; set; }
        public string EXISTING_FAREFAMILY { get; set; }
        public string ITA_FAREFAMILIES { get; set; }
    }
}
