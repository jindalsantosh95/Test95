using System;
using System.Reflection;

namespace VAA.EAPI.GraphQL.Rest.FunctionHost
{
    internal sealed class ResolverType
    {
        public Type QueryMutationType { get; init; }
        public Type ServiceType { get; init; }
        public Type For { get; init; }
        public MethodInfo ConfigureMethod { get; init; }
    }
}
