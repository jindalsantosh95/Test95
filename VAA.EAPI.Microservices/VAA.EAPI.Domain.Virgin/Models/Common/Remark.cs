using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class Remark
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum RemarksType
        {
            [EnumMember(Value = "SSR")]
            SSR = 0,
            [EnumMember(Value = "OSI")]
            OSI = 1
        }
        public RemarksType ItemType { get; set; }
        public string Code { get; set; }
        public string Text { get; set; }

        public static explicit operator Remark(JObject source)
        {
            return new Remark()
            {
                ItemType = (RemarksType)Enum.Parse(typeof(RemarksType), source["ItemType"]?.ToString()),
                Code = source["Code"]?.ToString(),
                Text = source["Text"]?.ToString(),
            };
        }

    }
}
