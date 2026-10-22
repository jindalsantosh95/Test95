using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Assertions;

public static class GuestJourneyAssertions
{
    public static void AssertConfirmation(
        GuestJourneyScenario scenario,
        LoyaltyAccount? loyaltyAccount,
        string selectedFare,
        IReadOnlyList<string> selectedSeats,
        ConfirmationDetails confirmation)
    {
        string expectedSurname = loyaltyAccount?.LastName ?? "AutomationOne";
        if (!confirmation.Surname.Equals(expectedSurname, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Confirmation surname was '{confirmation.Surname}', expected '{expectedSurname}'.");
        }

        AssertPassengerCount(confirmation, scenario.Adults, "Adult");
        AssertPassengerCount(confirmation, scenario.Teens, "Teen");
        AssertPassengerCount(confirmation, scenario.Children, "Child");
        AssertPassengerCount(confirmation, scenario.Infants, "Infant");

        if (!Regex.IsMatch(confirmation.PaymentMethod, "(?i)(card|visa)"))
        {
            throw new InvalidOperationException(
                $"Confirmation payment method was not Card/Visa. Actual: '{confirmation.PaymentMethod}'.");
        }

        int expectedFlights = scenario.IsReturn ? 2 : 1;
        if (confirmation.FlightCount < expectedFlights)
        {
            throw new InvalidOperationException(
                $"Confirmation rendered {confirmation.FlightCount} flight(s), expected {expectedFlights}. " +
                confirmation.BodySummary);
        }

        if (!scenario.SelectAnyAvailableFare)
        {
            string expectedCabin = scenario.FareName.StartsWith("Economy", StringComparison.OrdinalIgnoreCase)
                ? "Economy"
                : scenario.FareName;
            if (!confirmation.BodySummary.Contains(expectedCabin, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Confirmation did not retain the selected cabin '{expectedCabin}'. " +
                    $"Selected fare: '{selectedFare}'. {confirmation.BodySummary}");
            }
        }

        foreach (string seat in selectedSeats)
        {
            string seatNumber = seat.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
            if (!confirmation.BodySummary.Contains(seatNumber, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Confirmation did not contain selected seat '{seatNumber}' " +
                    $"(selection details: '{seat}'). {confirmation.BodySummary}");
            }
        }
    }

    public static void AssertPaymentPrice(
        PriceSummarySnapshot initial,
        PriceSummarySnapshot payment,
        SeatSelectionMode seatSelection,
        bool insuranceSelected,
        IReadOnlyList<string> selectedSeats)
    {
        bool paidSeatSelected = seatSelection == SeatSelectionMode.SelectPaid && selectedSeats.Count > 0;
        if (!paidSeatSelected && !insuranceSelected)
        {
            try
            {
                BookingPriceAssertions.AssertConsistent(initial, payment, "Payment");
            }
            catch (InvalidOperationException exception) when (seatSelection == SeatSelectionMode.SelectAvailable)
            {
                throw new InvalidOperationException(
                    $"Selecting an included seat changed the booking total. Selected seats: {string.Join(", ", selectedSeats)}. {exception.Message}",
                    exception);
            }
            return;
        }

        BookingPriceAssertions.AssertValid(payment, "Payment with paid seat");
        if (!BookingPriceAssertions.CurrenciesAreEquivalent(initial.Currency, payment.Currency))
        {
            throw new InvalidOperationException(
                $"Payment currency changed after seat selection from {initial.Currency} to {payment.Currency}.");
        }

        decimal initialTotal = decimal.Parse(initial.Total, NumberStyles.Number, CultureInfo.InvariantCulture);
        decimal paymentTotal = decimal.Parse(payment.Total, NumberStyles.Number, CultureInfo.InvariantCulture);
        if (paymentTotal <= initialTotal)
        {
            throw new InvalidOperationException(
                $"Journey extras did not increase the total: initial {initial}, payment {payment}.");
        }
    }

    public static void AssertExpectedCurrency(PriceSummarySnapshot price, string? expectedCurrency)
    {
        if (expectedCurrency.IsNullOrWhiteSpace())
        {
            return;
        }

        bool matches = expectedCurrency.ToUpperInvariant() switch
        {
            "GBP" => price.Currency.Contains('£'),
            "EUR" => price.Currency.Contains('€'),
            "CAD" => price.Currency.Contains("CA$", StringComparison.OrdinalIgnoreCase) ||
                     price.Currency.Contains("CAD", StringComparison.OrdinalIgnoreCase),
            "USD" => price.Currency.Contains("US$", StringComparison.OrdinalIgnoreCase) ||
                     price.Currency == "$",
            _ => price.Currency.Contains(expectedCurrency, StringComparison.OrdinalIgnoreCase)
        };

        if (!matches)
        {
            throw new InvalidOperationException(
                $"Expected {expectedCurrency} pricing, but the visible price summary used '{price.Currency}'.");
        }
    }

    private static void AssertPassengerCount(
        ConfirmationDetails confirmation,
        int count,
        string passengerType)
    {
        if (count == 0)
        {
            return;
        }

        string plural = passengerType == "Child" ? "Children" : $"{passengerType}s";
        string expected = count == 1 ? $"1 {passengerType}" : $"{count} {plural}";
        if (!confirmation.PassengerDetails.Contains(expected, StringComparison.OrdinalIgnoreCase) &&
            !confirmation.BodySummary.Contains(expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Confirmation did not contain passenger count '{expected}'. " +
                $"Passenger details: '{confirmation.PassengerDetails}'.");
        }
    }
}
