using HotChocolate.Language;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal sealed class ValueNode(SyntaxKind kind, object value) : IValueNode
    {
        public object Value { get; } = value;

        public SyntaxKind Kind { get; } = kind;

        public Location Location { get; } = default!;

        public bool Equals(IValueNode other) => Value.Equals(other.Value) && Kind == other.Kind;

        public IEnumerable<ISyntaxNode> GetNodes() => throw new NotImplementedException();

        public string ToString(bool indented) => throw new NotImplementedException();
    }
}
