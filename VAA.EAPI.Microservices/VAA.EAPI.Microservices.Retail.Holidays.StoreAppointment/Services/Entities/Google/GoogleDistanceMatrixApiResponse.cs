namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google
{
    public partial class GoogleService
    {
        private sealed class GoogleDistanceMatrixApiResponse
        {
            public List<GoogleDistanceMatrixRow> Rows { get; set; } = new();
        }
    }
}