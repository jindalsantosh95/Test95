namespace VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails
{
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public string ErrorMessage { get; init; }

        public static ValidationResult Valid() => new() { IsValid = true };
        public static ValidationResult Invalid(string errorMessage) => new() { IsValid = false, ErrorMessage = errorMessage };
    }
}
