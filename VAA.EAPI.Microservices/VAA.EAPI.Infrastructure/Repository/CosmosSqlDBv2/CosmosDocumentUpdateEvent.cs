using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Entities.Enums;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2
{
    public class CosmosDocumentUpdateEvent : CosmosDocumentEvent
    {
        internal List<PatchOperation> _operations = new();
        public CosmosDocumentUpdateEvent()
        {
        }

        public CosmosDocumentUpdateEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {

        }

        public void AddField(string path, object value)
        {
            _operations.Add(PatchOperation.Add(path, value));
        }

        public void RemoveField(string path)
        {
            _operations.Add(PatchOperation.Remove(path));
        }

        public void UpdateField(string path, object value)
        {
            _operations.Add(PatchOperation.Set(path, value));
        }
    }
}
