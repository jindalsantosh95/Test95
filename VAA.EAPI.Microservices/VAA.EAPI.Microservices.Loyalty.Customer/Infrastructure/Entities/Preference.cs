namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class Preference
    {
        public string Active { get; set; }

        public string CaptureCannel { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string Persona { get; set; }

        public string PreferenceId { get; set; }

        public string PreferenceSubType { get; set; }

        public string PreferenceType { get; set; }

        public string PreferenceValue { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public CommsType[] CommsType { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedOn { get; set; }
    }
}
