using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal class ObjectTypeDescriptor<T>(IResolverContext resolverContext) : IObjectTypeDescriptor<T>, IObjectTypeDescriptorFields
    {
        private readonly IResolverContext _resolverContext = resolverContext ?? throw new ArgumentNullException(nameof(resolverContext));

        private readonly ConcurrentBag<IObjectFieldDescriptor> _objectFieldDescriptors = [];

        public IReadOnlyCollection<IObjectFieldDescriptor> FieldDescriptors => _objectFieldDescriptors;

        public IObjectTypeDescriptor<T> BindFields(BindingBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor<T> BindFieldsExplicitly()
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> BindFieldsImplicitly()
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor<T> Description(string value)
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Directive<TDirective>(TDirective directiveInstance) where TDirective : class
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Directive<TDirective>() where TDirective : class, new()
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Directive(NameString name, params ArgumentNode[] arguments)
        {
            throw new NotImplementedException();
        }

        public IDescriptorExtension<ObjectTypeDefinition> Extend()
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor ExtendsType(Type extendsType)
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor ExtendsType<TExtendsType>()
        {
            throw new NotImplementedException();
        }

        public IObjectFieldDescriptor Field(Expression<Func<T, object>> propertyOrMethod)
        {
            var objectFieldDescriptor = new ObjectFieldDescriptor<T, object>(_resolverContext, propertyOrMethod);

            _objectFieldDescriptors.Add(objectFieldDescriptor);

            return objectFieldDescriptor;
        }
        public IObjectFieldDescriptor Field<TValue>(Expression<Func<T, TValue?>> propertyOrMethod)
        {
            var objectFieldDescriptor = new ObjectFieldDescriptor<T, TValue?>(_resolverContext, propertyOrMethod);

            _objectFieldDescriptors.Add(objectFieldDescriptor);

            return objectFieldDescriptor;
        }
        public IObjectFieldDescriptor Field(NameString name)
        {
            throw new NotImplementedException();
        }
        public IObjectFieldDescriptor Field<TResolver>(Expression<Func<TResolver, object>> propertyOrMethod)
        {
            throw new NotImplementedException();
        }
        public IObjectFieldDescriptor Field(MemberInfo propertyOrMethod)
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor<T> Implements<TInterface>() where TInterface : InterfaceType
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Implements<TInterface>(TInterface type) where TInterface : InterfaceType
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Implements(NamedTypeNode type)
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor<T> Interface<TInterface>() where TInterface : InterfaceType
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Interface<TInterface>(TInterface type) where TInterface : InterfaceType
        {
            throw new NotImplementedException();
        }
        public IObjectTypeDescriptor<T> Interface(NamedTypeNode type)
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor<T> IsOfType(IsOfType isOfType)
        {
            throw new NotImplementedException();
        }

        public IObjectTypeDescriptor<T> Name(NameString value)
        {
            throw new NotImplementedException();
        }
    }
}
