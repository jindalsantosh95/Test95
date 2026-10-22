using HotChocolate.Types;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost.Emulate
{
    internal interface IObjectTypeDescriptorFields
    {
        IReadOnlyCollection<IObjectFieldDescriptor> FieldDescriptors { get; }
    }
}
