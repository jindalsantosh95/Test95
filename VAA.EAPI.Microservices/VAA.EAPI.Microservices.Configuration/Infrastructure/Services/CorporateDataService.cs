using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Services
{
    public class CorporateDataService : ICorporateData
    {
        private readonly IAzureBlobService _azureBlobService;
        private readonly ICacheService _cacheService;

        public CorporateDataService(IAzureBlobService azureBlobService, ICacheService cacheService)
        {
            _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        public async Task<IEnumerable<CorporateDbEntity>> GetCorporateData(CancellationToken cancellationToken)
        {
            var cached = await _cacheService.GetCacheValue<IEnumerable<CorporateDbEntity>>(Constants.Application.CLIDCacheKey);

            if (cached != null) return cached;

            // Cache miss -> fetch from blob
            var blobName = ConfigReader.Get(Constants.Environment.CLIDBlobName);
            var blobStorageUri = ConfigReader.Get(Constants.Environment.BlobStorageUri);
            var containerName = ConfigReader.Get(Constants.Environment.ContainerName);
            var sasKey = ConfigReader.Get(Constants.Environment.SasKey);

            if (string.IsNullOrWhiteSpace(blobName))
                throw new InvalidOperationException("CLID blob name configuration is missing.");

            if (string.IsNullOrWhiteSpace(blobStorageUri) || string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(sasKey))
                throw new InvalidOperationException("Blob storage configuration is missing.");

            // Initialize/download blob so BlobStringAsync can read it
            await _azureBlobService.DownloadBlobAsync(blobStorageUri, containerName, blobName, sasKey, cancellationToken);

            var raw = await _azureBlobService.BlobStringAsync(blobName, cancellationToken).ConfigureAwait(false);

            var data = ParseCsvToEntities(raw) ?? Array.Empty<CorporateDbEntity>();

            await _cacheService.SetCacheValue(Constants.Application.CLIDCacheKey, data, true);

            return data;
        }

        public async Task RefreshCorporateCache(string blobContent, CancellationToken cancellationToken)
        {
            var data = ParseCsvToEntities(blobContent);

            await _cacheService.SetCacheValue(Constants.Application.CLIDCacheKey, data,true);
        }       

        private static IEnumerable<CorporateDbEntity> ParseCsvToEntities(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return Array.Empty<CorporateDbEntity>();

            using var reader = new StringReader(csv);
            using var parser = new TextFieldParser(reader)
            {
                TextFieldType = FieldType.Delimited
            };
            parser.SetDelimiters(",");

            var list = new List<CorporateDbEntity>();
            string[]? headers = parser.ReadFields();

            while (!parser.EndOfData)
            {
                var cols = parser.ReadFields();
                if (cols == null || cols.Length != headers?.Length) continue;

                var entity = new CorporateDbEntity
                {
                    CbaClid = GetValue(headers, cols, "CBA_CLID"),
                    CbiTicketDesignator = GetValue(headers, cols, "CBI_Ticket_Designator"),
                    AccCorporateName = GetValue(headers, cols, "ACC_Corporate_Name"),
                    CbaVsActiveContract = ParseBool(GetValue(headers, cols, "CBA_VS_Active_Contract")),
                    CbiPartnerCarrierCode = GetValue(headers, cols, "CBI_Partner_Carrier_Code")
                };

                if (!string.IsNullOrWhiteSpace(entity.CbaClid))
                    list.Add(entity);
            }

            return list;
        }

        private static string GetValue(string[] headers, string[] cols, string name)
        {
            var idx = Array.FindIndex(headers, h => h.Equals(name, StringComparison.OrdinalIgnoreCase));
            return idx >= 0 && idx < cols.Length ? cols[idx].Trim('"') : string.Empty;
        }

        private static bool ParseBool(string val)
        {
            if (bool.TryParse(val, out var b)) return b;
            if (int.TryParse(val, out var n)) return n != 0;
            return val.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                   val.Equals("YES", StringComparison.OrdinalIgnoreCase) ||
                   val.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
