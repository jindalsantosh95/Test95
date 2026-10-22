namespace VAA.EAPI.GraphQL.Domain.Operations.Interfaces
{
    public interface IPassengerManifestProvider
    {
        Task<Models.PassengerManifest.PassengerManifestResponse> Query(Models.PassengerManifest.PassengerManifestRequest request);
    }
}
