namespace VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability
{

    public sealed class RewardSeatAvailabilityValidationResult
    {
        public RewardSeatAvailabilityValidationResult(string? error)
        {
            Error = error;
        }

        public string? Error { get; }

        public bool IsValid => Error is null;
    }
}
