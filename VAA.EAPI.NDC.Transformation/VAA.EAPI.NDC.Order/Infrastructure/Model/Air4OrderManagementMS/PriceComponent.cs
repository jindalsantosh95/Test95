using System.Runtime.Serialization;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS
{
    public class PriceComponent
    {
        public enum PriceComponentTypeEnum
        {
            [EnumMember(Value = "BasePrice")]
            BasePrice = 1,            
            [EnumMember(Value = "BaseFare")]
            BaseFare = 2,            
            [EnumMember(Value = "Tax")]
            Tax = 3,           
            [EnumMember(Value = "Fee")]
            Fee = 4,            
            [EnumMember(Value = "Surcharge")]
            Surcharge = 5
        }
        public PriceComponentTypeEnum? PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public CurrencyAmount Amount { get; set; }
    }
}
