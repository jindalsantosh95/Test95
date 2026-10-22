using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
namespace VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange
{
    public class FlightItems
    {
        public string ProductCode { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ActionContexts
        {

            [EnumMember(Value = "Delete")]
            Delete = 0,

            [EnumMember(Value = "Add")]
            Add = 1,

            [EnumMember(Value = "Retain")]
            Retain = 2
        }
        [OpenApiProperty(Description = "Provides context for the change being requested")]
        public ActionContexts? ActionContext { get; set; }
        public string CurrentSeatAssignment { get; set; }
        public Segment Segments { get; set; }

    }
}
