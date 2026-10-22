using System.Linq;
using System.Xml.Linq;

namespace VAA.EAPI.Common.Helpers
{
    public static class XmlParse
    {
        public static string GetAttributesValue(string faultX, string attribute)
        {
            XElement errorAttributes = XElement.Parse(faultX);
            var errorMessageAttr = errorAttributes.Attributes(attribute);
            return errorMessageAttr.First().Value;
        }
    }
}
