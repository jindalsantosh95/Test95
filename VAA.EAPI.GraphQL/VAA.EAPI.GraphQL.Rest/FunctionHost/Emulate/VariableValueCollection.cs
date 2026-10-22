using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.Types;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal sealed class VariableValueCollection : IVariableValueCollection
    {
        private readonly Dictionary<string, object> _variables = [];

        public IEnumerator<VariableValue> GetEnumerator() => _variables.Select(kvp => new VariableValue(kvp.Key, new InputType(TypeKind.Object, typeof(object)), new ValueNode(SyntaxKind.Field, kvp.Value))).GetEnumerator();

        public T? GetVariable<T>(NameString name) => _variables.TryGetValue(name, out var value) && value is T typedValue ? typedValue : default;

        public bool TryGetVariable(string name, out object value) => _variables.TryGetValue(name, out value);

        public bool TryGetVariable<T>(NameString name, out T? value) => _variables.TryGetValue(name, out var objValue) && objValue is T typedValue ? (value = typedValue) != null : (value = default) != null;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
