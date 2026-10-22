using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;

namespace VAA.EAPI.Web.Common.Infrastructure.Exceptions
{
    public class GraphQLException : Exception
    {
        public static explicit operator GraphQLException(GraphQLResponse source) => new(source.Errors);

        public IEnumerable<GraphQLError> Errors { get; }

        public GraphQLException(IEnumerable<GraphQLError> errors)
        {
            Errors = errors;
        }
    }
}
