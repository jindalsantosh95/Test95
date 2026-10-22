namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Interface
{
    public interface ICheckAircraftCategoryDbService
    {
        Task<IReadOnlyCollection<string>> GetTailNumbersWithCategoryAsync(IEnumerable<string> tailNumbers, int categoryIndex, CancellationToken cancellationToken);
    }
}
