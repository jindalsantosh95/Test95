namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.MealServiceMutaion
{
    public class CancelMealRequest
    {
        public string OrderId { get;set; }
        public string RecordLocator { get; set; }
        public IEnumerable<FlightSegment> Segments { get; set; }
        public string CancelReason { get; set; }
    }
}
