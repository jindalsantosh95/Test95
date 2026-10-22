using HotChocolate;
using HotChocolate.Types;
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.RefData.Models
{
    public sealed class MobileFeatures
    {
        [JsonProperty("features", Required = Required.Always)]
        [GraphQLIgnore]
        public Dictionary<string, string> Features { get; set; } = new();

        [GraphQLName("features")]
        [GraphQLType(typeof(NonNullType<AnyType>))]
        public IReadOnlyDictionary<string, object> GetFeatures() =>
            Features.ToDictionary(feature => feature.Key, feature => (object)feature.Value);
    }
}
