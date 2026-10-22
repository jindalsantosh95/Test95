using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal interface IObjectFieldDescriptorInvoke<T, TValue>
    {
        Expression<Func<T, TValue?>> PropertyOrMethod { get; }
        object Apply(Type resultType);
    }

    internal class ObjectFieldDescriptor<T, TValue>(IResolverContext resolverContext, Expression<Func<T, TValue?>> propertyOrMethod) : IObjectFieldDescriptor, IObjectFieldDescriptorInvoke<T, TValue>
    {
        private readonly IResolverContext _resolverContext = resolverContext ?? throw new ArgumentNullException(nameof(resolverContext));

        private readonly ConcurrentDictionary<Type, FieldResolverDelegate> _fieldResolvers = [];

        public Expression<Func<T, TValue?>> PropertyOrMethod => propertyOrMethod ?? throw new ArgumentNullException(nameof(propertyOrMethod));

        public object Apply(Type resultType)
        {
            if (_fieldResolvers.TryGetValue(resultType, out var resolver))
                return resolver.Invoke(_resolverContext);

            return default;
        }

        public IObjectFieldDescriptor Argument(NameString argumentName, Action<IArgumentDescriptor> argumentDescriptor)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Deprecated(string reason)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Deprecated()
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor DeprecationReason(string reason)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Description(string value)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Directive<T>(T directiveInstance) where T : class
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Directive<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Directive(NameString name, params ArgumentNode[] arguments)
        {
            throw new NotImplementedException();
        }

        public IDescriptorExtension<ObjectFieldDefinition> Extend()
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Ignore(bool ignore = true)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Name(NameString value)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Resolve(FieldResolverDelegate fieldResolver)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Resolve(FieldResolverDelegate fieldResolver, Type resultType)
        {
            _fieldResolvers.TryAdd(resultType.IsGenericType ? resultType.GenericTypeArguments[0] : resultType, fieldResolver);

            return this;
        }

        public IObjectFieldDescriptor Resolver(FieldResolverDelegate fieldResolver)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Resolver(FieldResolverDelegate fieldResolver, Type resultType)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor ResolveWith<TResolver>(Expression<Func<TResolver, object>> propertyOrMethod)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor ResolveWith(MemberInfo propertyOrMethod)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Subscribe(SubscribeResolverDelegate subscribeResolver)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor SyntaxNode(FieldDefinitionNode fieldDefinition)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Type(ITypeNode typeNode)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Type(Type type)
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Use(FieldMiddleware middleware)
        {
            throw new NotImplementedException();
        }

        IObjectFieldDescriptor IObjectFieldDescriptor.Type<TOutputType>()
        {
            throw new NotImplementedException();
        }

        IObjectFieldDescriptor IObjectFieldDescriptor.Type<TOutputType>(TOutputType outputType)
        {
            throw new NotImplementedException();
        }
    }
}
