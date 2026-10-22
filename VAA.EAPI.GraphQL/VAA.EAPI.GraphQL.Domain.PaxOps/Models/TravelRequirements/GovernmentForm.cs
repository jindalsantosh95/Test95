using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    [GraphQLDescription("Government form that applies to one passenger and slice. Completion is self-attested; links are not click-tracked or validated by the backend.")]
    public sealed class GovernmentForm
    {
        [GraphQLDescription("Self-attestation state for this government form. Government forms use REQUIRED and COMPLETED only.")]
        public GovernmentFormsStatus Status { get; set; }

        [GraphQLDescription("Downstream link identifier, when supplied.")]
        public string? Id { get; set; }

        [GraphQLDescription("TRM link identifier required when submitting completion.")]
        public string? LinkId { get; set; }

        [GraphQLDescription("Display title for the required government form.")]
        public string Title { get; set; }

        [GraphQLDescription("Approved external web page for completing the government form.")]
        public string Url { get; set; }

        [GraphQLDescription("UTC date and time from which the government form can be completed. Null when the form is actionable or the window is unsupported.")]
        public DateTime? AvailableFromUtc { get; set; }

        [GraphQLDescription("Human-readable description of when the government form becomes available. Null when the form is actionable or the window is unsupported.")]
        public string? AvailableFromDescription { get; set; }

        [GraphQLDescription("Indicates whether the government form is currently available to complete.")]
        public bool IsAvailable { get; set; }

        public static explicit operator GovernmentForm(Infrastructure.Models.CheckInMS.GovernmentForm source) => new()
        {
            Status = source.Status,
            Id = source.Id,
            LinkId = source.LinkId,
            Title = source.Title,
            Url = source.Url,
            AvailableFromUtc = source.AvailableFromUtc,
            AvailableFromDescription = source.AvailableFromDescription,
            IsAvailable = source.IsAvailable
        };
    }
}
