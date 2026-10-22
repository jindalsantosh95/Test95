using NodaTime.Text;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.TravelRequirements
{
    public class SavePassportServiceRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public Document Document { get; set; }
        public string MRZLine1 { get; set; }
        public string MRZLine2 { get; set; }
        public bool IsScanned { get; set; }

        public static async Task<SavePassportServiceRequest> CreateRequestAsync(TravelRequirementsSavePassportRequest source)
        {
            var model = new SavePassportServiceRequest
            {
                RequirementsId = await TravelRequirementsIdHelper.GetRequirementsId(source.TravelRequirementsId),
                PassengerId = source.PassengerId,
                Document = (Document)source.Document,
                MRZLine1 = source.MRZLine1,
                MRZLine2 = source.MRZLine2,
                IsScanned = source.IsScanned
            };
            var pattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");
            if (source.Document.BirthDate != null) model.Document.DateOfBirth = pattern.Format(source.Document.BirthDate.Value);
            if (source.Document.ExpiryDate != null) model.Document.ExpirationDate = pattern.Format(source.Document.ExpiryDate.Value);

            return model;
        }
    }
}
