using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class SpecialServiceRequestInfo
    {
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
    }
}
