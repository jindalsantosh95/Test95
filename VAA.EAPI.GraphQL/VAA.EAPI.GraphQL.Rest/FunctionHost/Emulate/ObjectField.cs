using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal sealed class ObjectField(int index, string name, string description, IObjectType declaringType, Type runtimeType) : IObjectField
    {
        public IObjectType DeclaringType { get; } = declaringType;

        public bool IsParallelExecutable { get; } = default;

        public FieldDelegate Middleware { get; } = default!;

        public FieldResolverDelegate Resolver { get; } = default!;

        public PureFieldDelegate PureResolver { get; } = default!;

        public SubscribeResolverDelegate SubscribeResolver { get; } = default!;

        public IReadOnlyList<IDirective> ExecutableDirectives { get; } = default!;

        public MemberInfo Member { get; } = default!;

        public MemberInfo ResolverMember { get; } = default!;

        public bool MaybeStream { get; } = default;

        public bool IsIntrospectionField { get; } = default;

        public bool IsDeprecated { get; } = default;

        public string DeprecationReason { get; } = default!;

        public int Index { get; } = index;

        public string Description { get; } = description;

        public IDirectiveCollection Directives { get; } = default!;

        public ISyntaxNode SyntaxNode { get; } = default!;

        public IReadOnlyDictionary<string, object> ContextData { get; } = default!;

        public IOutputType Type { get; } = default!;

        public IFieldCollection<IInputField> Arguments { get; } = default!;

        public NameString Name { get; } = new NameString(name);

        public FieldCoordinate Coordinate { get; } = default!;

        public Type RuntimeType { get; } = runtimeType;

        IComplexOutputType IOutputField.DeclaringType => DeclaringType;

        ITypeSystemObject IField.DeclaringType { get; } = default!;
    }
}
