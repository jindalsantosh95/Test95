using System.Xml;
using VAA.EAPI.Automation.Core;

namespace VAA.EAPI.Automation.Heimdall.Requests.Endpoints
{
    public static class EieEndpoints
    {

        public static HttpResponseMessage SeatV8Facade(string departureDate, string configEndpoint)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\SeatV8.xml"), ConfigReader.Get("SeatV8EieAccessToken"), departureDate);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "RetrievePassengerMap");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            return WebRequest.Xml.Post(($"{ConfigReader.Get(configEndpoint)}/VAA.EIE.AIR4Facade.SeatV8/facade.svc"), xmlDocument.OuterXml, headers);
        }
    }
}
