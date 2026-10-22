using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class CheckInRequirements
    {
        public string? SliceId { get; set; }
        public bool PassportRequired { get; set; }
        public bool PassportProvided { get; set; }
        public bool PassportVerified { get; set; }
        public bool BaggageDeclared { get; set; }
        public ContactTracingStatus ContactTracing { get; set; }
        public DateTime? ContactTracingAvailableFromUtc { get; set; }
        public string? ContactTracingAvailableFromDescription { get; set; }
        public bool CheckedIn { get; set; }
        public OverallCheckInStatus Overall { get; set; }
        public ReturnDateStatus ReturnDateStatus { get; set; }
        public bool IsReturnDateDeclined { get; set; }
        public LocalDate? ReturnDate { get; set; }
        public bool CheckinEligible { get; set; }
        public bool UnhappyPassport { get; set; }
        public IEnumerable<CheckInError> PassportErrors { get; set; } = [];
        public IEnumerable<CheckInError> TravelRequirementErrors { get; set; } = [];
        public SecondaryDocumentStatus SecondaryDocumentStatus { get; set; }
        public SecondaryDocumentRequirement[] SecondaryDocuments { get; set; } = [];
        public IEnumerable<GovernmentForm>? GovernmentForms { get; set; }
    }
}
