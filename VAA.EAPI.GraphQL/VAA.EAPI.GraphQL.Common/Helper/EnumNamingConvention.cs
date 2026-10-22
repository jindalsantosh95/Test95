using HotChocolate;
using HotChocolate.Types.Descriptors;
using System;

namespace VAA.EAPI.GraphQL.Common.Helper
{
    public sealed class EnumNamingConvention : DefaultNamingConventions
    {
        public override NameString GetEnumValueName(object value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            return value.ToString();
        }
    }
}
