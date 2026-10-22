using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public class TravelRequirementsSaveContactTracingServiceRequest
    {
        public string RequirementsId { get; set; }
        public string SliceId { get; set; }
        public IEnumerable<ContactTracing> ContactTracing { get; set; }

        public static async Task<TravelRequirementsSaveContactTracingServiceRequest> CreateRequestAsync(string travelRequirementsId, string sliceId, IEnumerable<ContactTracing> contactTracing)
        {
            return new TravelRequirementsSaveContactTracingServiceRequest
            {
                RequirementsId = await TravelRequirementsIdHelper.GetRequirementsId(travelRequirementsId),
                SliceId = sliceId,
                ContactTracing = contactTracing
            };
        }
    }
}
