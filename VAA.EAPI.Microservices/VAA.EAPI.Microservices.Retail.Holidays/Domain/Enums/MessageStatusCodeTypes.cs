using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageStatusCodeTypes
    {
        [Description("Not started")]
        NOTSTARTED,
        [Description("In progress")]
        INPROGRESS,
        [Description("Completed")]
        COMPLETED
    }
}
