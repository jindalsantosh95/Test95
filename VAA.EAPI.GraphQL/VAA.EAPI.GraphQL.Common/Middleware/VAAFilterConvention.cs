using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.Filters;
using System;
using System.Linq;

namespace VAA.EAPI.GraphQL.Common.Middleware
{
    public class VAAFilterConvention : FilterConvention
    {
        public override NameString GetTypeName(Type type)
        {
            NameString result = base.GetTypeName(type);

            var attr = type.GetCustomAttributes(false).FirstOrDefault(attr => attr is VAAFilterConventionAttribute);
            if (attr is not null)
            {
                result = ((VAAFilterConventionAttribute)attr).FilterName;
            }
            return result;
        }

        protected override void Configure(IFilterConventionDescriptor descriptor)
        {
            descriptor.AddDefaults();
        }
    }
}