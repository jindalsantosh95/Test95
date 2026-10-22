using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.MobileApp.UI.Models;

/// <summary>
/// Prepared warning fragments, not executable proxy rules or proof of a real booking.
/// UI copy comes from the backend message; match every returned message once observable.
/// </summary>
public sealed class ConfirmationWarningCase
{
    private readonly string[] codes;

    private ConfirmationWarningCase(string name, params string[] codes)
    {
        Name = name;
        this.codes = codes;
    }

    public string Name { get; }
    public IReadOnlyList<string> ExpectedCodes => Array.AsReadOnly(codes);
    public IReadOnlyList<string> ExpectedMessages => Array.AsReadOnly(codes.Select(MessageFor).ToArray());

    public static ConfirmationWarningCase AllSeatsFailed => new("AllSeatsFailed", "W001");
    public static ConfirmationWarningCase SomeSeatsFailed => new("SomeSeatsFailed", "W002");
    public static ConfirmationWarningCase Proximity => new("Proximity", "W003");
    public static ConfirmationWarningCase AllSeatsFailedWithProximity => new("AllSeatsFailedWithProximity", "W003", "W001");
    public static ConfirmationWarningCase SomeSeatsFailedWithProximity => new("SomeSeatsFailedWithProximity", "W003", "W002");
    public static ConfirmationWarningCase NoWarnings => new("NoWarnings");

    // DeltaOrderMapper appends fulfilment warnings after proximity. UI assertions must
    // compare code sets, not assume that the application preserves response order.
    public JArray BuildWarnings() => new(codes.Select(code => new JObject
    {
        ["code"] = code,
        ["type"] = code switch
        {
            "W001" => "ALL_SEAT_FULFILMENT_FAILED",
            "W002" => "PARTIAL_SEAT_FULFILMENT",
            "W003" => "SEAT_NONCOMPLIANT_PROXIMITY",
            _ => throw new InvalidOperationException("Unsupported confirmation warning code.")
        },
        ["message"] = MessageFor(code)
    }));

    // Exact Retail.Order/Warnings/Warnings.json copy, including original spacing/spelling.
    // Do not replace backend wording with app-owned text or infer financial correctness.
    private static string MessageFor(string code) => code switch
    {
        "W001" => "We're sorry, the seats you selected have not been confirmed. Don't worry, you have not been charged. Please head to My Booking to select some new seats.",
        "W002" => "Your booking is complete, but unfortunately some of the seats you selected have not been confirmed. Please head to My Booking to select some new seats.",
        "W003" => "It’s a legal requirement for children to sit where they can be supervised by an adult or young adult (age 12 or over) travelling with them. This could be next to them, infront of, behind or across the aisle. Please ensure you  visit My booking before you travel to change your seat selection.",
        _ => throw new InvalidOperationException("Unsupported confirmation warning code.")
    };
}
