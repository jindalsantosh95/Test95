using System.ComponentModel;
using System.Reflection;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            Type enumType = value.GetType();
            FieldInfo fieldInfo = enumType.GetField(value.ToString());
            DescriptionAttribute descriptionAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return descriptionAttribute?.Description ?? value.ToString(); // Default to the enum member name if no description attribute is found.
        }
    }
}
