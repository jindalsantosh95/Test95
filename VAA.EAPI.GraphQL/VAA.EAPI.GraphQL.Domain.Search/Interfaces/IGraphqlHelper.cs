using HotChocolate.Resolvers;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    public interface IGraphqlHelper
    {
        IReadOnlyList<IFieldSelection> GetSelections(IResolverContext resolverContext);
        bool FieldIsSelected(IReadOnlyList<IFieldSelection> fieldSelections, string fieldName);
    }
}
