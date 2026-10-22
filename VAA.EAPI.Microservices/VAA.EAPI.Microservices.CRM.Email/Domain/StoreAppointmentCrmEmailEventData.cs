namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class StoreAppointmentCrmEmailEventData
    {
        public string AppointmentReferenceId { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string EmailDataXml { get; set; } = string.Empty;
    }
}
