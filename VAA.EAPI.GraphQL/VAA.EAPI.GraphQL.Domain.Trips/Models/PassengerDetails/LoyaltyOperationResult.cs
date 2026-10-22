namespace VAA.EAPI.GraphQL.Domain.Trips.Models.PassengerDetails
{
    public class LoyaltyOperationResult
    {
        public bool IsSuccess { get; init; }
        public string Status { get; init; }
        public string ErrorMessage { get; init; }
        public string CorrelationId { get; init; }

        public static LoyaltyOperationResult Success(string status, string correlationId = null) =>
            new() { IsSuccess = true, Status = status, CorrelationId = correlationId };

        public static LoyaltyOperationResult Failure(string errorMessage, string correlationId = null) =>
            new() { IsSuccess = false, Status = "Failed", ErrorMessage = errorMessage, CorrelationId = correlationId };
    }
}
