using System.Reflection;
using System.Text.Json;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    internal static class IropContentCatalogue
    {
        private const string ResourceName = "VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass.IropContent.json";

        private static readonly Lazy<IReadOnlyDictionary<string, IropInformation>> InformationBySubType = new(Load);

        public static IropInformation? GetInformation(string? subType)
        {
            if (subType.IsNullOrWhiteSpace())
                return null;

            return InformationBySubType.Value.TryGetValue(subType, out var content)
                ? content
                : null;
        }

        private static IReadOnlyDictionary<string, IropInformation> Load()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded IROP content resource '{ResourceName}' was not found.");

            var catalogue = JsonSerializer.Deserialize<IropContentCatalogueDocument>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (catalogue?.Statuses ?? [])
                .Where(status => status.Information is not null)
                .SelectMany(status => (status.SubTypes ?? [])
                    .Where(subType => !subType.IsNullOrWhiteSpace())
                    .Select(subType => new { SubType = subType, status.Information }))
                .ToDictionary(status => status.SubType, status => status.Information!, StringComparer.OrdinalIgnoreCase);
        }

        private sealed class IropContentCatalogueDocument
        {
            public IEnumerable<IropContentDefinition>? Statuses { get; set; }
        }

        private sealed class IropContentDefinition
        {
            public IEnumerable<string>? SubTypes { get; set; }
            public IropInformation? Information { get; set; }
        }
    }
}
