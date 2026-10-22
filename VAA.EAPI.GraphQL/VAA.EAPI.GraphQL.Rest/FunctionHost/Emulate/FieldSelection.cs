using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal sealed class FieldSelection(string responseName, IObjectField field, IType type, TypeKind typeKind, FieldNode syntaxNode, IReadOnlyList<FieldNode> syntaxNodes) : IFieldSelection
    {
        public NameString ResponseName { get; } = responseName ?? throw new ArgumentNullException(nameof(responseName));
        public IObjectField Field { get; } = field;
        public IType Type { get; } = type;
        public TypeKind TypeKind { get; } = typeKind;
        public FieldNode SyntaxNode { get; } = syntaxNode;
        public IReadOnlyList<FieldNode> SyntaxNodes { get; } = syntaxNodes;
    }
}
