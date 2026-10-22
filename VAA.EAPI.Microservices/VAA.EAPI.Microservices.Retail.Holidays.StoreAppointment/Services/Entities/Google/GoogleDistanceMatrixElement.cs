namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google
{
    public partial class GoogleService
    {
        private sealed class GoogleDistanceMatrixElement
        {
            public string Status { get; set; } = string.Empty;

            public GoogleDistanceMatrixValue? Distance { get; set; }

            public GoogleDistanceMatrixValue? Duration { get; set; }
        }
    }
}