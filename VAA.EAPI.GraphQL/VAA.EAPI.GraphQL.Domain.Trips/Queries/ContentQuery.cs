using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Content;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ContentQuery
    {
        private IContentServiceProvider _contentServiceProvider;

        public ContentQuery(IContentServiceProvider contentServiceProvider)
        {
            _contentServiceProvider = contentServiceProvider;
        }

      
        [GraphQLName("GetContentFragment")]
        public async Task<IEnumerable<string>> GetContentFragment(string? contentFragmentId, ContentRequest? contentRequest = null)
        {
            return await _contentServiceProvider.GetContentDetails(contentFragmentId, contentRequest);
        }
    }
}
