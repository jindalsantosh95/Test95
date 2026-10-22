namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion
{
    public class AddMealRequest
    {
        public string OrderId { get;set; }
        public string RecordLocator { get; set; }
        public IEnumerable<FlightSegment> Segments { get; set; }
    }
}
