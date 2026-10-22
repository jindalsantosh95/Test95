namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google
{
    public partial class GoogleService
    {
        private sealed class GoogleDistanceMatrixRow
        {
            public List<GoogleDistanceMatrixElement> Elements { get; set; } = new();
        }
    }
}