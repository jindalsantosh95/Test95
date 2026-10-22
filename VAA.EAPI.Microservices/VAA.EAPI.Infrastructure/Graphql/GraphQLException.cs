using System;

namespace VAA.EAPI.Infrastructure.Graphql
{
    public class GraphQLException : ApplicationException
    {
        public GraphQLException(string? message) : base(message) { }
    }
}
