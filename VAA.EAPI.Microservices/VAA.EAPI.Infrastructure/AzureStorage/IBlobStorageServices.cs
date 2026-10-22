using System;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.AzureStorage
{
    [Obsolete("Please use VAA.EAPI.Infrastructure.Services.AzureBlob.IAzureBlobService")]
    public interface IBlobStorageServices
    {
        Task<T> Deserialize<T>();
    }
}