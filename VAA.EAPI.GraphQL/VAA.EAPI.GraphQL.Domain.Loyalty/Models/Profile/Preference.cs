using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Profile
{
    public class Preference
    {
        public string Active { get; set; }
        public string Persona { get; set; }
        public PreferenceTypes? PreferenceType { get; set; }
        public string PreferenceSubType { get; set; }
        public string PreferenceValue { get; set; }
        public string CaptureCannel { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string PreferenceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
