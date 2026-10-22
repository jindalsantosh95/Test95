using HotChocolate;
using HotChocolate.Resolvers;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public static class ErrorHelper
    {
        public static T Fail<T>(IResolverContext context, string message, string title, string code)
        {
            context.ReportError(
                ErrorBuilder.New()
                    .SetMessage(message)
                    .SetExtension("title", title)
                    .SetExtension("code", code)
                    .Build());

            return default;
        }
    }
}
