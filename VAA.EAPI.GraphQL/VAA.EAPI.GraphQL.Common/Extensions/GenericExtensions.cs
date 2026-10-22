namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsNullOrDefault<T>(this T instance) => instance?.Equals(default(T)) ?? true;
    }
}
