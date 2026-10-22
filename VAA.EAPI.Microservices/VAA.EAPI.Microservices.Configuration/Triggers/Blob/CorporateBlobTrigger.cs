using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Triggers.Blob
{
    public class CorporateBlobTrigger
    {
        private readonly ILogger<CorporateBlobTrigger> _logger;
        private readonly ICorporateData _corporateData;

        public CorporateBlobTrigger(ILogger<CorporateBlobTrigger> logger, ICorporateData corporateData)
        {
            _logger = logger;
            _corporateData = corporateData;
        }

        [Function("CorporateBlobTrigger")]
        public async Task Run([Microsoft.Azure.Functions.Worker.BlobTrigger("%ContainerName%" + "/{name}")] string blobContent, CancellationToken cancellationToken)
        {
            await _corporateData.RefreshCorporateCache( blobContent,cancellationToken);
        }
    }
}