using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using VAA.EAPI.GraphQL.Stitch.Core.Extensions;
using VAA.EAPI.GraphQL.Stitch.Core.Helpers;

namespace VAA.EAPI.GraphQL.Stitch.Core.Middleware
{
    internal sealed class OperationLimiterMiddleware
    {
        private const string OperationCountLimitConfiguration = "OperationLimiterMiddleware:OperationCountLimit";

        private readonly int _limit = ConfigReader.Try(OperationCountLimitConfiguration, 5);
        private readonly RequestDelegate _next;
        private readonly IQueryResult _limitResponse;

        public OperationLimiterMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _limitResponse = QueryResultBuilder.CreateError(ErrorBuilder.New()
                                                                        .SetMessage($"Only {_limit} operation(s) per request is allowed.")
                                                                        .SetCode("400")
                                                                        .Build());
        }

        public async ValueTask InvokeAsync(IRequestContext context)
        {
            var document = context.Document;

            if (document != null)
            {
                var operations = document.Definitions.Where(x => x is OperationDefinitionNode)
                                                     .Select(x => (OperationDefinitionNode)x);

                int operationCount = operations.Count();

                if (operationCount > _limit)
                {
                    context.Result = _limitResponse;

                    return;
                }

                var queries = ((HotChocolate.Types.ObjectType)context.Schema.Types.FirstOrDefault(x => x.Name.Is("Query")))?.Fields.ToArray() ?? [];
                var mutations = ((HotChocolate.Types.ObjectType)context.Schema.Types.FirstOrDefault(x => x.Name.Is("Mutation")))?.Fields.ToArray() ?? [];

                operationCount = operations.SelectMany(operation => operation.SelectionSet.Selections.Where(x => x is FieldNode).Select(x => (FieldNode)x))
                                           .Where(x => queries.Any(q => q.Name == x.Name.Value) || mutations.Any(q => q.Name == x.Name.Value))
                                           .Count();

                if (operationCount > _limit)
                {
                    context.Result = _limitResponse;

                    return;
                }
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}