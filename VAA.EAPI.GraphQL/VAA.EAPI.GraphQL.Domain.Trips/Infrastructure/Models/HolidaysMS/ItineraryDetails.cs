namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class ItineraryDetails
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<ItineraryItemsDetails> ItineraryItems { get; set; }

    }
}