using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
{
    public class FareFamily : IFareFamily
    {
        private readonly IAzureBlobService _blobService;

        public FareFamily(IAzureBlobService blobService)
        {
            _blobService = blobService ?? throw new ArgumentNullException(nameof(blobService));
        }

        public async Task<string> GetFareFamily(string rowKey)
        {
            var blobData = await _blobService.BlobJsonAsAsync<JToken>(ConfigReader.Get(Constants.Environment.ItaFareFamilyBlobName), CancellationToken.None);

            return blobData.SelectToken($"$.[?(@.RowKey== '{rowKey}')]")?.ToObject<ItaFareFamilyTableEntity>().ItaFareFamily;
        }

        public async Task<System.Collections.Generic.IReadOnlyDictionary<string, string>> GetFareFamilies(
            System.Collections.Generic.IReadOnlyCollection<string> rowKeys, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(rowKeys);
            var blobData = await _blobService.BlobJsonAsAsync<JToken>(
                ConfigReader.Get(Constants.Environment.ItaFareFamilyBlobName), cancellationToken);
            if (blobData is not JArray rows)
                throw new InvalidOperationException("Reward fare-family blob is not an array.");
            var requested = new System.Collections.Generic.HashSet<string>(rowKeys, StringComparer.Ordinal);
            var matching = rows.Select(row => row.ToObject<ItaFareFamilyTableEntity>())
                .Where(row => row is not null && requested.Contains(row.RowKey))
                .ToArray();
            if (matching.Length != requested.Count || matching.Select(row => row.RowKey).Distinct(StringComparer.Ordinal).Count() != requested.Count ||
                matching.Any(row => row.ItaFareFamily.IsNullOrWhiteSpace()))
                throw new InvalidOperationException("Reward fare-family blob rows are incomplete or duplicated.");
            return matching.ToDictionary(row => row.RowKey, row => row.ItaFareFamily, StringComparer.Ordinal);
        }
    }
}
