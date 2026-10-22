using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public sealed class TravelRequirementsSaveSecondaryDocumentAttestationServiceRequest
    {
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public string SliceId { get; set; }
        public string SecondaryDocumentId { get; set; }
        public string CountryCode { get; set; }

        public static async Task<TravelRequirementsSaveSecondaryDocumentAttestationServiceRequest> CreateRequestAsync(
            string travelRequirementsId,
            string passengerId,
            string sliceId,
            string secondaryDocumentId,
            string countryCode)
        {
            return new TravelRequirementsSaveSecondaryDocumentAttestationServiceRequest
            {
                RequirementsId = await TravelRequirementsIdHelper.GetRequirementsId(travelRequirementsId),
                PassengerId = passengerId,
                SliceId = sliceId,
                SecondaryDocumentId = secondaryDocumentId,
                CountryCode = countryCode
            };
        }
    }
}
