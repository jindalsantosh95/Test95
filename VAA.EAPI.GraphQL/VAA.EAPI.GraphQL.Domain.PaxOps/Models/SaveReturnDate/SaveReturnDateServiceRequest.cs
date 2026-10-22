using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate
{
    public class SaveReturnDateServiceRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public string PassengerId { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool NoReturnDate { get; set; }
        public IEnumerable<string> SelectedPassengersIds { get; set; }

        public static async Task<SaveReturnDateServiceRequest> CreateRequestAsync(SaveReturnDateRequest source)
        {
            return new SaveReturnDateServiceRequest
            {
                TravelPartyId = await CheckInIdHelper.GetTravelPartyId(source.CheckInId),
                RequirementsId = await CheckInIdHelper.GetRequirementsId(source.CheckInId),
                PassengerId = source.PassengerId,
                ReturnDate = source.ReturnOn?.AtMidnight().ToDateTimeUnspecified() ?? source.ReturnDate,
                NoReturnDate = source.NoReturnDate,
                SelectedPassengersIds = await CheckInIdHelper.GetSelectedPassengers(source.CheckInId)
            };
        }
    }
}
