using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;

namespace VAA.EAPI.Domain.Virgin.Models.Common
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
        public string Ordinal { get; set; }
        public string AccountCode { get; set; }
        public string PccCode { get; set; }
        public Contacts Contacts { get; set; }
        public bool IsCreditCardAccepted { get; set; }
        public string RequestAgentType { get; set; }

        public static explicit operator Participant(JObject source)
        {
            return new Participant()
            {
                AccountCode = source["AccountCode"]?.ToString(),
                PccCode = source["PccCode"]?.ToString(),
                AgencyID = source["AgencyID"]?.ToString(),
                Contacts = source["Contacts"] != null ? (Contacts)source["Contacts"] : null,
                Name = source["Name"]?.ToString(),
                Ordinal = source["Ordinal"]?.ToString(),
                PartyType = source["PartyType"] != null ? (PartyTypeEnum)Enum.Parse(typeof(PartyTypeEnum), source["PartyType"].ToString()) : null,
                IsCreditCardAccepted = source["IsCreditCardAccepted"] != null ? (bool)source["IsCreditCardAccepted"] : false,
                RequestAgentType = source["RequestAgentType"]?.ToString(),
            };
        }
    }
}