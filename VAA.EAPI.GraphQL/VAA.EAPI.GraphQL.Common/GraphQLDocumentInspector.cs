using HotChocolate.Language;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.GraphQL.Common
{
    public static class GraphQLDocumentInspector
    {
        public static bool ContainsField(DocumentNode document, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

            var fragments = document.Definitions
                .OfType<FragmentDefinitionNode>()
                .GroupBy(fragment => fragment.Name.Value, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            return document.Definitions
                .OfType<OperationDefinitionNode>()
                .Any(operation => ContainsField(operation.SelectionSet, fieldName, fragments, new HashSet<string>(StringComparer.Ordinal)));
        }

        private static bool ContainsField(
            SelectionSetNode selectionSet,
            string fieldName,
            IReadOnlyDictionary<string, FragmentDefinitionNode> fragments,
            ISet<string> visitedFragments) => selectionSet.Selections.Any(selection => selection switch
            {
                FieldNode field when string.Equals(field.Name.Value, fieldName, StringComparison.Ordinal) => true,
                FieldNode field when field.SelectionSet is not null => ContainsField(field.SelectionSet, fieldName, fragments, visitedFragments),
                InlineFragmentNode inlineFragment => ContainsField(inlineFragment.SelectionSet, fieldName, fragments, visitedFragments),
                FragmentSpreadNode fragmentSpread => ContainsField(fragmentSpread, fieldName, fragments, visitedFragments),
                _ => false
            });

        private static bool ContainsField(
            FragmentSpreadNode fragmentSpread,
            string fieldName,
            IReadOnlyDictionary<string, FragmentDefinitionNode> fragments,
            ISet<string> visitedFragments)
        {
            var name = fragmentSpread.Name.Value;
            return visitedFragments.Add(name) && fragments.TryGetValue(name, out var fragment) &&
                   ContainsField(fragment.SelectionSet, fieldName, fragments, visitedFragments);
        }
    }
}
