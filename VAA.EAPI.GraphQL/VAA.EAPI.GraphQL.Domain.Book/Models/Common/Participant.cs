using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class Participant
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PartyTypeEnum
        {
            [EnumMember(Value = "Aggregator")]
            Aggregator = 0,
            [EnumMember(Value = "TMC")]
            TMC = 1,
            [EnumMember(Value = "OTA")]
            OTA = 2,
            [EnumMember(Value = "Meta")]
            Meta = 3,
            [EnumMember(Value = "GSA")]
            GSA = 4,
            [EnumMember(Value = "Corporate")]
            Corporate = 5,
            [EnumMember(Value = "SME")]
            SME = 6,
            [EnumMember(Value = "GDS")]
            GDS = 7,
            [EnumMember(Value = "Retail")]
            Retail = 8,
            [EnumMember(Value = "System")]
            System = 9,
            [EnumMember(Value = "ORA")]
            ORA = 10,
            [EnumMember(Value = "POA")]
            POA = 11,
            [EnumMember(Value = "OperatingCarrier")]
            OperatingCarrier = 12,
            [EnumMember(Value = "MarketingCarrier")]
            MarketingCarrier = 13,
            [EnumMember(Value = "Person")]
            Person = 14,
            [EnumMember(Value = "Seller")]
            Seller = 15,
            [EnumMember(Value = "Distributor")]
            Distributor = 16,
            [EnumMember(Value = "Carrier")]
            Carrier = 17,
        }
        public PartyTypeEnum? PartyType { get; set; }
        public string Name { get; set; }
        public string AgencyID { get; set; }
    }

}