using HotChocolate;
using System;
using System.Linq;
using System.Reflection;

namespace VAA.EAPI.GraphQL.Middleware;

internal class GraphQLNameMap
{
    public static explicit operator GraphQLNameMap(MethodInfo source) => new GraphQLNameMap()
    {
        GraphQLAttr = source.GetCustomAttributesData().First(a => a.AttributeType == typeof(GraphQLNameAttribute)),
        Class = source.DeclaringType,
        MethodInfo = source
    };

    public CustomAttributeData GraphQLAttr { get; private set; }
    public Type Class { get; private set; }
    public MethodInfo MethodInfo { get; private set; }

    public override string ToString() => $"{Class.FullName}+{MethodInfo.Name}";
}