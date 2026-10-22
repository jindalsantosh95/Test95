using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount.Blob;
using VAA.EAPI.Automation.MobileApp.Constants;

namespace VAA.EAPI.Automation.MobileApp.Tests.SchemaValidator
{
    public class SchemaValidation
    {
        private readonly AzureBlobService _azureBlobService;
        private readonly string sasKey = Vault.Default.GetSecretValue("AzureBlobStorageSchemasSasKey");
        private readonly string url = Vault.Default.GetSecretValue("AzureBlobStorageUrl");
        private readonly string environment = Config.Environment.ToLowerInvariant();

        public SchemaValidation()
        {
            _azureBlobService = new AzureBlobService();
        }

        [Fact]
        [Trait("Category", Traits.SchemaValidation)]
        public async Task SchemaValidationMobile()
        {
            string lastFileStored = string.Empty;

            try
            {
                lastFileStored = (await _azureBlobService.FindBlobsAsync(url, "schemas", sasKey, $"MobileGQLSchemaTest_{environment}", CancellationToken.None)).First();
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"No file found using MobileGQLSchemaTest_{environment}", exception);
            }

            await _azureBlobService.DownloadBlobAsync(url, "schemas", lastFileStored, sasKey, CancellationToken.None);

            string sourceSchema = await _azureBlobService.BlobStringAsync(lastFileStored, CancellationToken.None);

            //GQL calls
            HttpResponseMessage gqlAirportDataResponse = Requests.Endpoints.GraphQLEndpoint.Introspection().EnsureGQLResponseSuccess();

            string currentSchema = gqlAirportDataResponse.ReadResponseAsString();

            if (sourceSchema == currentSchema)
            {
                await _azureBlobService.UploadBlobAsync(url, "schemas", sasKey, $"MobileGQLSchemaTest_{environment}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.json", currentSchema, BlobType.Json, CancellationToken.None);
                return;
            }

            JToken source = JToken.Parse(sourceSchema);
            JToken current = JToken.Parse(currentSchema);

            await _azureBlobService.UploadBlobAsync(url, "schemas", sasKey, $"MobileGQLSchemaTest_{environment}_{DateTime.UtcNow:yyyyMMddHHmmssfff}.json", currentSchema, BlobType.Json, CancellationToken.None);

            List<string> differences = VAA.EAPI.Automation.Core.Helpers.SchemaValidator.CompareJson(source, current);

            Assert.True(differences.Count == 0, "Schema differences found:\n" + string.Join("\n", differences));
        }
    }
}
