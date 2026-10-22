using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public class CheckInRequirements
    {
        [GraphQLDescription("Slice the requirement state applies to. Correlates to slices[].sliceId. Not used for checkin, because you can only check in for the currently active slice.")]
        public string? SliceId { get; set; }

        [GraphQLDescription("Whether this passenger and slice have applicable pre-travel document requirements.")]
        public bool PassportRequired { get; set; }

        [GraphQLDescription("Whether passport data has been provided for this passenger and slice.")]
        public bool PassportProvided { get; set; }

        [GraphQLDescription("Whether passport data has been captured by a reliable source, such as a mobile phone camera or airport kiosk scanner. If we don't have a verified passport then a member of staff will need to inspect the document at the airport.")]
        public bool PassportVerified { get; set; }

        [GraphQLDescription("Whether baggage declaration is complete. This can only happen in the 24 hours before travel, it will be false before then.")]
        public bool BaggageDeclared { get; set; }

        [GraphQLDescription("Contact tracing requirement state.")]
        public ContactTracingStatus ContactTracing { get; set; }

        [GraphQLDescription("UTC timestamp from which contact tracing can be completed, when it is not yet available.")]
        public DateTime? ContactTracingAvailableFromUtc { get; set; }

        [GraphQLDescription("Human-readable description of when contact tracing becomes available, derived from the acceptance window.")]
        public string? ContactTracingAvailableFromDescription { get; set; }

        [GraphQLDescription("Whether check-in is complete for this passenger.")]
        public bool CheckedIn { get; set; }

        [GraphQLDescription("Overall completion state for this passenger.")]
        public OverallCheckInStatus Overall { get; set; }

        [GraphQLDescription("Enum to show if passenger needs to provide a return date.")]
        public ReturnDateStatus ReturnDateStatus { get; set; }

        [GraphQLDescription("Whether the passenger declined to provide a return date.")]
        public bool IsReturnDateDeclined { get; set; }

        [GraphQLDescription("Return date in ISO yyyy-MM-dd format when provided.")]
        public LocalDate? ReturnDate { get; set; }

        [GraphQLDescription("Whether this passenger/slice is eligible for check-in.")]
        public bool CheckinEligible { get; set; }
        public bool UnhappyPassport { get; set; }

        [GraphQLDescription("User-safe passport or secondary-document errors for the requirement scope.")]
        public IEnumerable<CheckInError> PassportErrors { get; set; } = [];

        [GraphQLDescription("User-safe configured travel-requirement errors handled separately from passportErrors.")]
        public IEnumerable<CheckInError> TravelRequirementErrors { get; set; } = [];

        [GraphQLDescription("Secondary document requirement state.")]
        public SecondaryDocumentStatus SecondaryDocumentStatus { get; set; }

        [GraphQLDescription("Applicable secondary-document requirements for this passenger and slice. Empty when none apply or passport prerequisites are not met.")]
        public TravelRequirements.SecondaryDocumentRequirement[] SecondaryDocuments { get; set; } = [];

        [GraphQLDescription("Government forms for this passenger and slice. Null when no government forms apply.")]
        public IEnumerable<TravelRequirements.GovernmentForm>? GovernmentForms { get; set; }

        public static explicit operator CheckInRequirements(Infrastructure.Models.CheckInMS.CheckInRequirements source) => new()
        {
            SliceId = source.SliceId,
            PassportRequired = source.PassportRequired,
            PassportProvided = source.PassportProvided,
            PassportVerified = source.PassportVerified,
            BaggageDeclared = source.BaggageDeclared,
            ContactTracing = source.ContactTracing,
            ContactTracingAvailableFromUtc = source.ContactTracingAvailableFromUtc,
            ContactTracingAvailableFromDescription = source.ContactTracingAvailableFromDescription,
            CheckedIn = source.CheckedIn,
            Overall = source.Overall,
            ReturnDateStatus = source.ReturnDateStatus,
            ReturnDate = source.ReturnDate,
            IsReturnDateDeclined = source.IsReturnDateDeclined,
            CheckinEligible = source.CheckinEligible,
            UnhappyPassport = source.UnhappyPassport,
            PassportErrors = source.PassportErrors,
            TravelRequirementErrors = source.TravelRequirementErrors,
            SecondaryDocumentStatus = source.SecondaryDocumentStatus,
            SecondaryDocuments = (source.SecondaryDocuments ?? [])
                .Select(document => (TravelRequirements.SecondaryDocumentRequirement)document)
                .ToArray(),
            GovernmentForms = source.GovernmentForms?.Select(form => (TravelRequirements.GovernmentForm)form)
        };
    }
}
