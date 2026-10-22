namespace VAA.EAPI.Infrastructure.Graphql
{
    public class GraphQLError
    {
        public string Message { get; set; }
        public GraphQLErrorExtension Extensions { get; set; }
    }

}
