using HotChocolate.Resolvers;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    public class GraphqlHelper : IGraphqlHelper
    {
        public IReadOnlyList<IFieldSelection> GetSelections(IResolverContext resolverContext)
        {
            return resolverContext.GetSelections((ObjectType)resolverContext.Selection.Type.NamedType());
        }

        public bool FieldIsSelected(IReadOnlyList<IFieldSelection> fieldSelections, string fieldName)
        {
            return fieldSelections.Any(t => t.Field.Name.Equals(fieldName));
        }
    }
}
