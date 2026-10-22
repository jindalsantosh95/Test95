using HotChocolate;
using HotChocolate.Types;

namespace VAA.EAPI.GraphQL.FunctionHost
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class VersionQuery
    {
        [GraphQLName("gqlVersion")]
        [GraphQLDescription("Default query to return GraphQL version.")]
        public Version GQLVersion() => (Version)typeof(GraphQLNameAttribute).Assembly.GetName().Version;
    }
}
