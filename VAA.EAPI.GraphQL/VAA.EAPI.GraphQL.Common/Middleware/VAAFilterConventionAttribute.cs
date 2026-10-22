using System;

namespace VAA.EAPI.GraphQL.Common.Middleware;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class VAAFilterConventionAttribute : Attribute
{
    public string FilterName { get; }

    public VAAFilterConventionAttribute(string filterName)
    {
        FilterName = filterName;
    }
}