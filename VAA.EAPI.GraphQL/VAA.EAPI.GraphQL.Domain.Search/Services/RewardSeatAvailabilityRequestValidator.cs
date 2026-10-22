using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Search.Models.RewardSeatAvailability;

namespace VAA.EAPI.GraphQL.Domain.Search.Services
{

    internal static class RewardSeatAvailabilityRequestValidator
    {
        internal static RewardSeatAvailabilityValidationResult Validate(RewardSeatAvailabilityRequest? request)
        {
            if (request is null)
            {
                return new RewardSeatAvailabilityValidationResult("Reward seat availability request is required.");
            }

            if (request.Years is null || request.Years.Count() != 1 || request.Months is null || request.Months.Count() != 1)
            {
                return new RewardSeatAvailabilityValidationResult("Exactly one year and month are required.");
            }

            if (!IsIataCode(request.Slice?.Origin) || !IsIataCode(request.Slice.Destination) ||
                request.Slice.Origin.Is(request.Slice.Destination))
            {
                return new RewardSeatAvailabilityValidationResult("A valid origin and destination are required.");
            }

            if (request.Slice.Departure.Year != request.Years.First() || request.Slice.Departure.Month != (int)request.Months.First())
            {
                return new RewardSeatAvailabilityValidationResult("Departure, year and month must describe the same calendar month.");
            }

            return new RewardSeatAvailabilityValidationResult(null);
        }

        private static bool IsIataCode(string? value)
        {
            return value is { Length: 3 } && value.All(char.IsAsciiLetterUpper);
        }

    }
}
