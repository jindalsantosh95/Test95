using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
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

        public static explicit operator Remark(Common.Remark source)
        {
            if (source == null) return null;

            return new Remark
            {
                ItemType = (RemarksType)source.ItemType,
                Code = source.Code,
                Text = source.Text
            };
        }
    }
}