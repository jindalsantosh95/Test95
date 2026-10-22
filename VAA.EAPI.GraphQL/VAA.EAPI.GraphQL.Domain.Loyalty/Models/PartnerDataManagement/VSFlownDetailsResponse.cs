using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement
{
    public class VSFlownDetailsResponse
    {
        public IEnumerable<VSRetroFlownDetail> VSRetroFlownDetails { get; set; } = new List<VSRetroFlownDetail>();
    }
}
