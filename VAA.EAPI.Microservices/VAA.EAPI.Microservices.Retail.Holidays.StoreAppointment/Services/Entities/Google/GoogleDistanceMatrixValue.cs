namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google
{
    public partial class GoogleService
    {
        private sealed class GoogleDistanceMatrixValue
        {
            public string Text { get; set; } = string.Empty;

            public long Value { get; set; }
        }
    }
}