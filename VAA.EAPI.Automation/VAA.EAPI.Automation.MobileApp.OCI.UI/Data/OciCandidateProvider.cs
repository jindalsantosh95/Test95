using System.Globalization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.OCI.UI.Models;
using VAA.EAPI.Automation.OCI.Requests.Endpoints;
using VAA.EAPI.Automation.OCI.Tests.GraphQL;
using VAA.EAPI.Infrastructure.AzureStorage;
using CoreConfigReader = VAA.EAPI.Automation.Core.Helpers.ConfigReader;

namespace VAA.EAPI.Automation.MobileApp.OCI.UI.Data;

public sealed class OciCandidateProvider
{
    private static readonly string[] SourceTables = ["OnlineCheckInMobileAppData", "OCIPNRs"];
    private static readonly HashSet<string> UsaAirports =
        new(StringComparer.OrdinalIgnoreCase) { "ATL", "JFK", "LAS", "LAX", "MCO", "MIA", "SEA", "SFO", "TPA" };
    private readonly IConfiguration _config;

    public OciCandidateProvider(IConfiguration config)
    {
        _config = config;
    }

    public OciCandidate FindPreparedSingleAdult(int maximumApiAttempts = 8)
    {
        ConfigureCore();
        List<OciCandidate> candidates = ReadCandidates()
            .OrderBy(GetRoutePriority)
            .ThenBy(candidate => candidate.DepartureUtc)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                "No single-adult OCI candidates were available 80 minutes to 24 hours before departure.");
        }

        List<string> failures = [];
        foreach (OciCandidate candidate in candidates.Take(maximumApiAttempts))
        {
            try
            {
                ValidateTripsBookingEligibility(candidate);
                PreparePassport(candidate);
                return candidate;
            }
            catch (OciGraphQlContractException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Add($"{candidate.Pnr}: {exception.Message}");
            }
        }

        throw new InvalidOperationException(
            $"No OCI candidate passed backend preparation after {Math.Min(maximumApiAttempts, candidates.Count)} attempts. " +
            string.Join(" | ", failures));
    }

    private void ConfigureCore()
    {
        SetCoreValue("KeyVaultUrl", RequiredSetting("Core:KeyVaultUrl"));
        SetCoreValue("KeyVaultTenantId", _config["Core:KeyVaultTenantId"]);
        SetCoreValue("KeyVaultClientId", _config["Core:KeyVaultClientId"]);
        SetCoreValue("KeyVaultClientSecret", _config["Core:KeyVaultClientSecret"]);
        SetCoreValue("env", RequiredSetting("OCI:Environment"));
        SetCoreValue("BaseUrl", RequiredSetting("OCI:BackendBaseUrl"));
        SetCoreValue("graphQlCheckInMobile", RequiredSetting("OCI:GraphQlCheckInPath"));
    }

    private string RequiredSetting(string key)
    {
        string value = (_config[key] ?? string.Empty).Trim();
        if (value.IsNullOrWhiteSpace() || value.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Local mobile setting '{key}' is required for OCI data preparation.");
        }

        return value;
    }

    private static void SetCoreValue(string key, string? value)
        => CoreConfigReader.Set(key.ToLowerInvariant(), (value ?? string.Empty).Trim());

    private static List<OciCandidate> ReadCandidates()
    {
        string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");
        DateTime now = DateTime.UtcNow;
        DateTime earliest = now.AddMinutes(80);
        DateTime latest = now.AddHours(24);
        List<OciCandidate> candidates = [];

        foreach (string tableName in SourceTables)
        {
            TableStorageService<CreatedPnrsTableEntity> table = new()
            {
                TableName = tableName,
                ConnectionString = connectionString,
            };

            foreach (CreatedPnrsTableEntity row in table.RetrieveItemsAsync(default).GetAwaiter().GetResult())
            {
                if (!IsSingleAdultCandidate(row, earliest, latest, out DateTime departureUtc))
                {
                    continue;
                }

                candidates.Add(new OciCandidate(
                    row.RowKey,
                    row.FirstName,
                    row.LastName,
                    row.Origin,
                    row.Destination,
                    departureUtc,
                    tableName));
            }
        }

        return candidates
            .GroupBy(candidate => candidate.Pnr, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static bool IsSingleAdultCandidate(
        CreatedPnrsTableEntity row,
        DateTime earliest,
        DateTime latest,
        out DateTime departureUtc)
    {
        bool parsed = DateTime.TryParseExact(
            row.DepartureDate,
            "yyyy-MM-ddTHH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out departureUtc);

        return parsed &&
               departureUtc >= earliest &&
               departureUtc <= latest &&
               row.PassengersCount.Is("1") &&
               !row.RowKey.IsNullOrWhiteSpace() &&
               !row.FirstName.IsNullOrWhiteSpace() &&
               !row.LastName.IsNullOrWhiteSpace() &&
               !row.Origin.IsNullOrWhiteSpace() &&
               !row.Destination.IsNullOrWhiteSpace();
    }

    private static int GetRoutePriority(OciCandidate candidate)
        => candidate.Destination.Is("LHR")
            ? 0
            : UsaAirports.Contains(candidate.Destination)
                ? 1
                : 2;

    private void ValidateTripsBookingEligibility(OciCandidate candidate)
    {
        const string query = """
            query Booking($tripDetailsRequest: TripsDetailsRequestInput!) {
              Booking(tripDetailsRequest: $tripDetailsRequest) {
                id
                product { checkInEligible slices { checkInEligible segments { operationalStatus { type subType } } } }
              }
            }
            """;
        JObject request = new()
        {
            ["operationName"] = "Booking",
            ["query"] = query,
            ["variables"] = new JObject
            {
                ["tripDetailsRequest"] = new JObject
                {
                    ["using"] = "PNR",
                    ["confirmationNum"] = candidate.Pnr,
                    ["givenNames"] = candidate.FirstName,
                    ["surname"] = candidate.LastName,
                },
            },
        };

        string baseUrl = RequiredSetting("OCI:BackendBaseUrl").TrimEnd('/');
        string environment = RequiredSetting("OCI:Environment").Trim('/');
        string path = RequiredSetting("OCI:GraphQlCheckInPath").TrimStart('/');
        using HttpResponseMessage response = WebRequest.Json.Post(
            $"{baseUrl}/{environment}/{path}",
            request.ToString()).EnsureGQLResponseSuccess();
        JToken result = JToken.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        string bookingId = result.SelectToken("$.data.Booking.id")?.ToString() ?? string.Empty;
        List<bool> eligibility = result.SelectTokens("$.data.Booking.product[*]..checkInEligible")
            .Select(token => token.Value<bool>())
            .ToList();
        if (bookingId.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Trips Booking returned no usable booking.");
        }

        if (eligibility.Any(value => !value))
        {
            string statuses = string.Join(", ", result
                .SelectTokens("$.data.Booking.product[*]..operationalStatus")
                .Select(status => $"{status["type"]}:{status["subType"]}"));
            throw new InvalidOperationException(
                $"Trips Booking marked check-in ineligible{(statuses.IsNullOrWhiteSpace() ? string.Empty : $" ({statuses})")}.");
        }
    }

    private static void PreparePassport(OciCandidate candidate)
    {
        string request = GraphQLRequestBuilder.CheckInGraphQLRequest(
            candidate.Pnr,
            candidate.FirstName,
            candidate.LastName,
            candidate.Origin);
        using HttpResponseMessage response = GraphQLEndpoint.GraphQLCheckInMobile(request).EnsureGQLResponseSuccess();
        JToken result = JToken.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

        bool available = result.SelectToken("$..onlineCheckInStatus.available")?.Value<bool>() ?? false;
        string checkInId = result.SelectToken("$..checkInId")?.ToString() ?? string.Empty;
        JToken? passenger = result.SelectTokens("$..passengers[*]").FirstOrDefault();
        string passengerId = passenger?["passengerId"]?.ToString() ?? string.Empty;
        string dateOfBirth = passenger?["dateOfBirth"]?.ToString() ?? string.Empty;
        if (!available || checkInId.IsNullOrWhiteSpace() || passengerId.IsNullOrWhiteSpace() || dateOfBirth.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("GetBoardingPass did not return one check-in-eligible passenger.");
        }

        string saveRequest = BuildSavePassportRequest(
            checkInId,
            passengerId,
            passenger?["givenNames"]?.ToString() ?? candidate.FirstName,
            passenger?["surname"]?.ToString() ?? candidate.LastName,
            dateOfBirth);
        using HttpResponseMessage saveResponse = GraphQLEndpoint.GraphQLCheckInMobile(saveRequest).EnsureGQLResponseSuccess();
        JToken saveResult = JToken.Parse(saveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        List<string> graphQlErrors = saveResult.SelectTokens("$.errors[*].message")
            .Select(token => token.ToString())
            .Where(message => !message.IsNullOrWhiteSpace())
            .ToList();
        if (graphQlErrors.Count > 0)
        {
            throw new OciGraphQlContractException(
                "SavePassport GraphQL contract failed: " + string.Join(" | ", graphQlErrors));
        }

        JToken? savedCheckIn = saveResult.SelectToken("$.data.SavePassport");
        bool explicitlyUnavailable = savedCheckIn?["onlineCheckInStatus"]?["available"]?.Value<bool>() == false;
        bool returnedPassenger = (savedCheckIn?["passengers"]?.Count() ?? 0) > 0;
        if (explicitlyUnavailable || savedCheckIn?["checkInId"]?.ToString().IsNullOrWhiteSpace() != false || !returnedPassenger)
        {
            string dataFields = saveResult["data"] is JObject data
                ? string.Join(",", data.Properties().Select(property => property.Name))
                : "none";
            string saveFields = savedCheckIn is JObject saved
                ? string.Join(",", saved.Properties().Select(property => property.Name))
                : "none";
            throw new InvalidOperationException(
                $"SavePassport returned no usable payload. dataFields={dataFields}; saveFields={saveFields}; " +
                $"checkInIdPresent={savedCheckIn?["checkInId"]?.ToString().IsNullOrWhiteSpace() == false}; " +
                $"passengerCount={savedCheckIn?["passengers"]?.Count() ?? 0}; returnedPassenger={returnedPassenger}; " +
                $"explicitlyUnavailable={explicitlyUnavailable}; " +
                $"statusCode={savedCheckIn?["onlineCheckInStatus"]?["code"]}; " +
                $"statusTitle={savedCheckIn?["onlineCheckInStatus"]?["title"]}; " +
                $"statusMessage={savedCheckIn?["onlineCheckInStatus"]?["message"]}.");
        }
    }

    private static string BuildSavePassportRequest(
        string checkInId,
        string passengerId,
        string givenNames,
        string surname,
        string birthDate)
    {
        const string mutation = """
            mutation SavePassport($savePassportRequest: SavePassportRequestInput) {
              SavePassport(savePassportRequest: $savePassportRequest) {
                checkInId
                passengers { passengerId givenNames surname dateOfBirth }
                onlineCheckInStatus { available title message code }
              }
            }
            """;

        JObject request = new()
        {
            ["query"] = mutation,
            ["variables"] = new JObject
            {
                ["savePassportRequest"] = new JObject
                {
                    ["checkInId"] = checkInId,
                    ["passengerId"] = passengerId,
                    ["document"] = new JObject
                    {
                        ["documentType"] = "PASSPORT",
                        ["documentNumber"] = BuildDeterministicPassportNumber($"{checkInId}:{passengerId}"),
                        ["givenNames"] = givenNames,
                        ["surname"] = surname,
                        ["gender"] = new JObject { ["code"] = "M" },
                        ["issuingCountry"] = new JObject { ["code"] = "GBR" },
                        ["nationality"] = new JObject { ["code"] = "GBR" },
                        ["countryOfResidence"] = new JObject { ["code"] = "GBR" },
                        ["birthDate"] = birthDate,
                        ["expiryDate"] = DateTime.UtcNow.AddYears(7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    },
                    ["mRZLine1"] = string.Empty,
                    ["mRZLine2"] = string.Empty,
                    ["isScanned"] = false,
                },
            },
        };

        return request.ToString(Newtonsoft.Json.Formatting.None);
    }

    private static string BuildDeterministicPassportNumber(string seed)
    {
        uint hash = 0;
        foreach (char character in seed.Trim().ToUpperInvariant())
        {
            hash = unchecked((hash * 31) + character);
        }

        return $"A{hash % 100_000_000:D8}";
    }

    private sealed class OciGraphQlContractException : Exception
    {
        public OciGraphQlContractException(string message)
            : base(message)
        {
        }
    }
}
