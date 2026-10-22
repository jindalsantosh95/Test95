using System.Linq;
using System.Xml.Linq;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Helper
{
    public static class XmlParse
    {
        public static string Parse(string faultX, string attribute)
        {
            XElement errorAttributes = XElement.Parse(faultX);
            var errorMessageAttr = errorAttributes.Attributes(attribute);
            return errorMessageAttr.First().Value;
        }
    }
}
