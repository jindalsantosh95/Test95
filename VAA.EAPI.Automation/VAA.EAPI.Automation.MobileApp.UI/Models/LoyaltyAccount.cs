namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record LoyaltyAccount(
    string LoginIdentifier,
    string FlyingClubNumber,
    string FirstName,
    string LastName,
    string Tier,
    int Points);
