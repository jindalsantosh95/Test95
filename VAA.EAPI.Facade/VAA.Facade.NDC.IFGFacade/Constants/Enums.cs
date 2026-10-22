using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.Facade.NDC.IFGFacade.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductType
    {
        [EnumMember(Value = "Ticket")]
        Ticket = 100,
        [EnumMember(Value = "Seat")]
        Seat = 200
    }
}
