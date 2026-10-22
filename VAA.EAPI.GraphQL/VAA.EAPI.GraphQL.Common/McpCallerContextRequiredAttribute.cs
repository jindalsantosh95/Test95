using System;

namespace VAA.EAPI.GraphQL.Common
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class McpCallerContextRequiredAttribute : Attribute;
}
