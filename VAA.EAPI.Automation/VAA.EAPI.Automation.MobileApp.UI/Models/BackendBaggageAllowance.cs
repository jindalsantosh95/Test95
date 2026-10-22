namespace VAA.EAPI.Automation.MobileApp.UI.Models;

public sealed record BackendBaggageAllowance(
    string Category, int Count, string? Weight, string? Dimensions);
