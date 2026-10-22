using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Validation;

namespace VAA.EAPI.GraphQL.Domain.Search.Validation
{

    public sealed class RewardSeatOperationValidationRule : IDocumentValidatorRule
    {
        private const string RewardField = "rewardSeatAvailability";
        private const int MaximumInspectionSteps = 1024;

        // This is a document contract, independent of request variables and safe to cache.
        public bool IsCacheable => true;

        public void Validate(IDocumentValidatorContext context, DocumentNode document)
        {
            if (!context.Schema.QueryType.Fields.ContainsField(RewardField)) return;

            var fragments = new Dictionary<string, FragmentDefinitionNode>(StringComparer.Ordinal);
            foreach (var definition in document.Definitions)
                if (definition is FragmentDefinitionNode fragment)
                    fragments.TryAdd(fragment.Name.Value, fragment);

            foreach (var definition in document.Definitions)
            {
                if (definition is not OperationDefinitionNode { Operation: OperationType.Query } operation) continue;
                var error = Inspect(operation.SelectionSet, fragments);
                if (error is null) continue;
                context.ReportError(error);
                return;
            }
        }

        private static IError? Inspect(SelectionSetNode root, Dictionary<string, FragmentDefinitionNode> fragments)
        {
            var pending = new Stack<SelectionSetNode>();
            var visited = new HashSet<SelectionSetNode>(ReferenceEqualityComparer.Instance);
            // Two different response names are sufficient to reject a reward operation.
            var responseNames = new HashSet<string>(StringComparer.Ordinal);
            var hasReward = false;
            var hasDeferredRoot = false;
            var steps = 0;
            pending.Push(root);
            while (pending.TryPop(out var selections))
            {
                if (!visited.Add(selections)) continue;
                foreach (var selection in selections.Selections)
                {
                    steps++;
                    switch (selection)
                    {
                        case FieldNode field when !IsSkipped(field.Directives):
                            if (responseNames.Count < 2) responseNames.Add(field.Alias?.Value ?? field.Name.Value);
                            hasReward |= field.Name.Value == RewardField;
                            break;
                        case InlineFragmentNode inline when !IsSkipped(inline.Directives):
                            hasDeferredRoot |= IsDeferred(inline.Directives);
                            pending.Push(inline.SelectionSet);
                            break;
                        case FragmentSpreadNode spread when !IsSkipped(spread.Directives):
                            hasDeferredRoot |= IsDeferred(spread.Directives);
                            if (fragments.TryGetValue(spread.Name.Value, out var fragment))
                                pending.Push(fragment.SelectionSet);
                            break;
                    }

                    // Unrelated operations never receive a reward-specific error or limit. Until
                    // reward is found, discovery visits each root selection set once, without expansion.
                    if (!hasReward) continue;
                    if (steps > MaximumInspectionSteps)
                        return Error("REWARD_SEAT_INSPECTION_LIMIT", "The operation exceeds the reward availability inspection limit.");
                    if (responseNames.Count > 1 || hasDeferredRoot)
                        return Error("REWARD_SEAT_OPERATION_LIMIT", "Reward seat availability must be the only root field and cannot use deferred root fragments.");
                }
            }
            return null;
        }

        private static bool IsSkipped(IEnumerable<DirectiveNode> directives) => directives.Any(directive =>
            directive.Arguments.Any(argument => argument.Name.Value == "if" && argument.Value is BooleanValueNode value &&
                ((directive.Name.Value == "skip" && value.Value) || (directive.Name.Value == "include" && !value.Value))));

        private static bool IsDeferred(IEnumerable<DirectiveNode> directives) => directives.Any(directive =>
            directive.Name.Value == "defer" && !directive.Arguments.Any(argument => argument.Name.Value == "if" &&
                argument.Value is BooleanValueNode { Value: false }));

        private static IError Error(string code, string message) => ErrorBuilder.New().SetCode(code).SetMessage(message).Build();
    }
}
