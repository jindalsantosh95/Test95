using NodaTime;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public class TravelRequirementsSaveReturnDateServiceRequest
    {
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public LocalDate? ReturnOn { get; set; }
        public bool NoReturnDate { get; set; }

        public static async Task<TravelRequirementsSaveReturnDateServiceRequest> CreateRequestAsync(TravelRequirementsSaveReturnDateRequest source)
        {
            return new TravelRequirementsSaveReturnDateServiceRequest
            {
                RequirementsId = await TravelRequirementsIdHelper.GetRequirementsId(source.TravelRequirementsId),
                PassengerId = source.PassengerId,
                ReturnOn = source.ReturnOn,
                NoReturnDate = source.NoReturnDate
            };
        }
    }
}
