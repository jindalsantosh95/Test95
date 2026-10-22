namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SecureFlightInfo;

public sealed class UpdateSecureFlightInfoRequestInput
{
    public string secureFlightReservationId { get; set; } = string.Empty;

    public string passengerId { get; set; } = string.Empty;

    public string passengerFirstNin { get; set; } = string.Empty;

    public string passengerLastNin { get; set; } = string.Empty;

    public List<SecureFlightDetailsInput> secureFlightDetails { get; set; } = [];
}
