namespace VAA.EAPI.Automation.MobileApp.OCI.UI.Models;

public sealed record OciCandidate(
    string Pnr,
    string FirstName,
    string LastName,
    string Origin,
    string Destination,
    DateTime DepartureUtc,
    string SourceTable);
