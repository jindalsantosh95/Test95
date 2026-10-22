namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class APIErrorBatch
    {
        public IEnumerable<APIErrorResponse> Exceptions { get; set; }
    }
}
