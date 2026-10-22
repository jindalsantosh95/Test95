using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this Enum source, NamingStrategy namingStrategy = null)
        {
            if (namingStrategy.IsNullOrDefault())
                namingStrategy = new DefaultNamingStrategy();

            return source.GetType()
                         .GetMember(source.ToString())
                         .First()
                         .ToDisplayName(namingStrategy);
        }

        public static string ToDisplayName(this MemberInfo element, NamingStrategy namingStrategy = null)
        {
            ArgumentNullException.ThrowIfNull(element);

            if (namingStrategy.IsNullOrDefault())
                namingStrategy = new DefaultNamingStrategy();

            DisplayAttribute customAttribute = element.GetCustomAttribute<DisplayAttribute>(inherit: false);
            EnumMemberAttribute customAttribute2 = element.GetCustomAttribute<EnumMemberAttribute>(inherit: false);

            string name = ((!customAttribute2.IsNullOrDefault()) ? customAttribute2.Value : ((!customAttribute.IsNullOrDefault()) ? customAttribute.Name : element.Name));

            return namingStrategy.GetPropertyName(name, hasSpecifiedName: false);
        }
    }
}
