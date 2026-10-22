using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage
{
    public class AzureStorageCommand : IRequest<Unit>
    {
        public string StorageConnection { get; set; }
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public Actions? Action { get; set; }
        public string FileData { get; set; }
        public string ArchiveContainer { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Actions
        {
            [EnumMember(Value = "Add")]
            Add = 0,
            [EnumMember(Value = "Delete")]
            Delete = 1,
            [EnumMember(Value = "Copy")]
            Copy = 2,
        }

    }
}
