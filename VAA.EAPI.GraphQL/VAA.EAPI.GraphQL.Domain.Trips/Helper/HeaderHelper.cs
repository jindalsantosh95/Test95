using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public class HeaderHelper
    {
        public static string GetIPAddress()
        {
            var ipAddress = AsyncContext.Current.Context.GetHeader("X-Forwarded-For");
            return !ipAddress.IsNullOrWhiteSpace() ? ipAddress.Split(',').First() : string.Empty;
        }
    }
}
