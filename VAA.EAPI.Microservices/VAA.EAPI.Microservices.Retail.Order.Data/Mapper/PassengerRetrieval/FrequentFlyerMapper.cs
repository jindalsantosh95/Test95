using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper.PassengerRetrieval
{
    public static class FrequentFlyerMapper
    {
        public static FrequentFlyerDetails Resolve(IEnumerable<SpecialServiceRequest>? specialServiceRequests, CustomerDetail passenger, Ticket? ticket)
        {
            var fqtvDetails = GetFqtvDetails(specialServiceRequests, passenger, ticket);

            return new FrequentFlyerDetails(
                FirstNonEmpty(fqtvDetails?.AirlineCode, passenger.FrequentFlyer?.AirlineCode),
                FirstNonEmpty(fqtvDetails?.MemberNumber, passenger.FrequentFlyer?.AccountNumber));
        }

        private static FqtvDetails? GetFqtvDetails(IEnumerable<SpecialServiceRequest>? specialServiceRequests, CustomerDetail passenger, Ticket? ticket)
        {
            var fqtvRequest = specialServiceRequests?
                .Where(request => string.Equals(request.RequestCode, "FQTV", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(request => FqtvMatchesPassenger(request, passenger, ticket));

            if (string.IsNullOrWhiteSpace(fqtvRequest?.Remarks)) return null;

            var loyaltyDetails = fqtvRequest.Remarks.Trim().TrimStart('/');
            var passengerSeparator = loyaltyDetails.IndexOf('-');
            var carrierAndMemberNumber = passengerSeparator >= 0
                ? loyaltyDetails[..passengerSeparator]
                : loyaltyDetails;

            if (carrierAndMemberNumber.Length <= 2) return null;

            var airlineCode = carrierAndMemberNumber[..2];
            var memberNumber = carrierAndMemberNumber[2..];

            return memberNumber.All(char.IsDigit)
                ? new FqtvDetails(airlineCode, memberNumber)
                : null;
        }

        private static bool FqtvMatchesPassenger(SpecialServiceRequest request, CustomerDetail passenger, Ticket? ticket)
        {
            var hasFirstNameNumber = !string.IsNullOrWhiteSpace(request.FirstNameNumber);
            var hasLastNameNumber = !string.IsNullOrWhiteSpace(request.LastNameNumber);

            if (hasFirstNameNumber || hasLastNameNumber)
            {
                var firstNameMatches = !hasFirstNameNumber || string.Equals(request.FirstNameNumber, passenger.FirstNameNumber, StringComparison.OrdinalIgnoreCase);
                var lastNameMatches = !hasLastNameNumber || string.Equals(request.LastNameNumber, passenger.LastNameNumber, StringComparison.OrdinalIgnoreCase);

                return firstNameMatches && lastNameMatches;
            }

            var passengerSeparator = request.Remarks?.IndexOf('-') ?? -1;
            var fqtvPassengerName = passengerSeparator >= 0 ? request.Remarks[(passengerSeparator + 1)..] : null;

            return !string.IsNullOrWhiteSpace(fqtvPassengerName)
                && string.Equals(fqtvPassengerName, ticket?.PassengerName, StringComparison.OrdinalIgnoreCase);
        }

        private static string? FirstNonEmpty(string? primaryValue, string? fallbackValue) =>
            !string.IsNullOrWhiteSpace(primaryValue) ? primaryValue : fallbackValue;

        private sealed record FqtvDetails(string AirlineCode, string MemberNumber);
    }

    public sealed record FrequentFlyerDetails(string? AirlineCode, string? MemberNumber);
}
