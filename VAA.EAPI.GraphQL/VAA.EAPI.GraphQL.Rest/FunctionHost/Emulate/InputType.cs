using HotChocolate.Types;
using System;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal sealed class InputType(TypeKind kind, Type runtimeType) : IInputType
    {
        public TypeKind Kind { get; } = kind;

        public Type RuntimeType { get; } = runtimeType;
    }
}
