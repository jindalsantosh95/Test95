using HotChocolate;
using HotChocolate.Resolvers;
using System.Reflection;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class ResolverContextProxy : DispatchProxy
    {
        public List<IError> Errors { get; } = [];

        public static ResolverContextProxy Create(out IResolverContext context)
        {
            context = DispatchProxy.Create<IResolverContext, ResolverContextProxy>();

            return (ResolverContextProxy)(object)context;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(IResolverContext.ReportError) && args?.Length > 0)
            {
                if (args[0] is IError error)
                {
                    Errors.Add(error);
                }
                else if (args[0] is string message)
                {
                    Errors.Add(ErrorBuilder.New().SetMessage(message).Build());
                }

                return null;
            }

            throw new NotSupportedException($"{nameof(ResolverContextProxy)} only supports {nameof(IResolverContext.ReportError)}.");
        }
    }
}
