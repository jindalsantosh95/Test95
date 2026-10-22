using Azure.Storage.Blobs;
using MediatR;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage
{
    public class AzureStorageCommandHandler : IRequestHandler<AzureStorageCommand, Unit>
    {
        public virtual async Task<Unit> Handle(AzureStorageCommand request, CancellationToken cancellationToken)
        {
            var blobClient = GetBlobClient(request.StorageConnection, request.ContainerName, request.BlobName);

            switch (request.Action)
            {
                case AzureStorageCommand.Actions.Add:
                    var data = Encoding.UTF8.GetBytes(request.FileData);
                    using (var ms = new MemoryStream(data))
                        await blobClient.UploadAsync(ms, overwrite: true);
                    break;
                case AzureStorageCommand.Actions.Delete:
                    await blobClient.DeleteIfExistsAsync();
                    break;
                case AzureStorageCommand.Actions.Copy:
                    var archiveBlobClient = GetBlobClient(request.StorageConnection, request.ArchiveContainer, request.BlobName);
                    await archiveBlobClient.StartCopyFromUriAsync(blobClient.Uri);
                    break;
            }

            return Unit.Value;
        }

        private BlobClient GetBlobClient(string storageConnection, string containerName, string blobName)
        {
            var blobContainerClient = new BlobContainerClient(storageConnection, containerName);

            blobContainerClient.CreateIfNotExistsAsync();

            return blobContainerClient.GetBlobClient(blobName);
        }
    }
}
