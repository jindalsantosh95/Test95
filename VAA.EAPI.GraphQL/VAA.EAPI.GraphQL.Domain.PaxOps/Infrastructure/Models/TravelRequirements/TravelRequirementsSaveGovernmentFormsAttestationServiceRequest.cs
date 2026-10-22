using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public sealed class TravelRequirementsSaveGovernmentFormsAttestationServiceRequest
    {
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public string SliceId { get; set; }
        public string Id { get; set; }
        public string LinkId { get; set; }
        public string Text { get; set; }
        public string Href { get; set; }

        public static async Task<TravelRequirementsSaveGovernmentFormsAttestationServiceRequest> CreateRequestAsync(
            string travelRequirementsId,
            string passengerId,
            string sliceId,
            string governmentFormId,
            string linkId,
            string text,
            string href)
        {
            return new TravelRequirementsSaveGovernmentFormsAttestationServiceRequest
            {
                RequirementsId = await TravelRequirementsIdHelper.GetRequirementsId(travelRequirementsId),
                PassengerId = passengerId,
                SliceId = sliceId,
                Id = governmentFormId,
                LinkId = linkId,
                Text = text,
                Href = href
            };
        }
    }
}
