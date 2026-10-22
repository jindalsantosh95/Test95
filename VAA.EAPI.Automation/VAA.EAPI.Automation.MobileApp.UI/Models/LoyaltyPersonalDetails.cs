namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record LoyaltyPersonalDetails(
    string MemberName,
    bool EditDetailsAvailable,
    bool DeleteAccountAvailable);
