namespace VAA.EAPI.Infrastructure.Graphql
{
    public class ServiceRequest<T>
    {
        public string Query { get; set; }
        public T Variables { get; set; }
    }
}
