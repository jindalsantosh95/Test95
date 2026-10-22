using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Starlink;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Domain.Trips.Services;

public sealed class StarlinkEligibilityProvider : IStarlinkEligibilityProvider
{
    private const int StarlinkCategoryIndex = 11;
    private readonly IHttpConnector _connector;

    public StarlinkEligibilityProvider(IHttpConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public async Task<IReadOnlyDictionary<string, bool?>> GetEligibilityAsync(IEnumerable<string> tailNumbers)
    {
        var normalizedTailNumbers = (tailNumbers ?? [])
            .Where(x => !x.IsNullOrWhiteSpace())
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedTailNumbers.Length == 0)
            return new Dictionary<string, bool?>(StringComparer.OrdinalIgnoreCase);

        var headers = new Dictionary<string, string>
        {
            [CommonGraphql.Constants.Application.OcpKeyHeader] =
                ConfigReader.Get(CommonGraphql.Constants.Environment.FlightOpsOcpKey)
        };
        var request = new { TailNumbers = normalizedTailNumbers, CategoryIndex = StarlinkCategoryIndex };
        var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.FlightOpsUrl).TrimEnd('/')}/checkaircraftcategory";
        var response = (await _connector.SendAsync<CheckAircraftCategoryResponse>(url, headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        return (response?.AircraftCategoryChecks ?? [])
            .Where(x => !x.TailNumber.IsNullOrWhiteSpace())
            .ToDictionary(x => x.TailNumber.Trim(), x => (bool?)x.DoesCategoryExist, StringComparer.OrdinalIgnoreCase);
    }
}
