using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class SpecialServiceRequestInfo
    {
        public string AirlineCode { get; set; }
        public string AllPassengersFlag { get; set; }
        public string AllSegmentsFlag { get; set; }
        public string FreeformText { get; set; }
        public string NumberInParty { get; set; }
        public string RecordNumber { get; set; }
        public string RequestCode { get; set; }
        public string StatusCode { get; set; }
        public string FirstName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastName { get; set; }
        public string LastNameNumber { get; set; }
        public string SegmentNumber { get; set; }
        public string SSRNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ActionContexts
        {

            [EnumMember(Value = "Delete")]
            Delete = 0,

            [EnumMember(Value = "Add")]
            Add = 1,

            [EnumMember(Value = "Update")]
            Update = 2
        }
        public ActionContexts? ActionContext { get; set; }
        public string ProgramCode { get; set; }
    }
}
