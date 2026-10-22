using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class ServiceDefinition
    {
        public string ServiceDefinitionId { get; set; }
        public IEnumerable<string> Description { get; set; }
    }
}
