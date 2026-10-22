using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Common.Helper
{
    public class EnumOperations
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo == null)
            {
                throw new InvalidOperationException();
            }

            EnumMemberAttribute[] attributes = (EnumMemberAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), false);
            return attributes.Length > 0 ? attributes[0].Value : value.ToString();
        }
    }
}
