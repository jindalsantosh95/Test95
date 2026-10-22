using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;

namespace VAA.EAPI.Automation.Book.Helpers
{
    public static class RetrievePnrDetails
    {
        private static readonly CertificateFixture certificateFixture = new();

        static RetrievePnrDetails()
        {
            certificateFixture.ImportCertificate("certificate", Vault.Default, "Delta-SOA-Certificate");
        }

        private static XmlDocument ReturnPnrCommonV6Response(string rloc)
        {
            HttpResponseMessage pnrCommonV6Retrieve = Core.Delta.Endpoint.Soa.PnrCommonV6Retrieve(rloc, certificateFixture.GetCertificate("certificate")).EnsureIsSuccessStatusCode();

            string result = pnrCommonV6Retrieve.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument retrievePnrCommonXmlDocument = new();

            retrievePnrCommonXmlDocument.LoadXml(result);

            return retrievePnrCommonXmlDocument;
        }

        public static string[] ReturnAssignedSeats(string rloc)
        {
            var retrievePnrCommonXmlDocument = ReturnPnrCommonV6Response(rloc);

            var xmlNamespaceManager = retrievePnrCommonXmlDocument.CreateXmlNamespaceManager();

            return retrievePnrCommonXmlDocument.SelectNodes("//pnr:SeatDl", xmlNamespaceManager).OfType<XmlNode>().Select(x => x.Attributes["CurrentSeatAssignment"].Value).ToArray();
        }

        public static int ReturnAllocatedChildMealCount(string pnr)
        {
            var pnrDetailsXml = ReturnPnrCommonV6Response(pnr);
            var xmlNamespaceManager = pnrDetailsXml.CreateXmlNamespaceManager();

            var flightNumbers = pnrDetailsXml.SelectNodes("//pnr:DtiSegmentInfo", xmlNamespaceManager).OfType<XmlNode>()
                .Where(x => x.Attributes["Carrier"]?.Value == "VS")
                .Select(x => x.Attributes["FlightNumber"].Value)
                .Distinct()
                .ToArray();

            var childPassengers = pnrDetailsXml.SelectNodes("//pnr:Passenger", xmlNamespaceManager).OfType<XmlNode>()
                .Where(x => x.Attributes["FirstName"].Value.StartsWith("CHILD"))
                .Select(x => x.Attributes["LastNameNumber"])
                .ToArray();

            XmlNode[] ssrs = null;
            List<XmlNode> childMeals = new();

            foreach (var child in childPassengers)
            {
                foreach (var flight in flightNumbers)
                {
                    ssrs = pnrDetailsXml.SelectNodes("//pnr:SpecialServiceRequest", xmlNamespaceManager).OfType<XmlNode>()
                        .Where(x => x.Attributes["RequestCode"].Value == "CHML" && x.Attributes["StatusCode"].Value == "HK" && x.Attributes["LastNameNumber"].Value == child.Value && x.Attributes["FlightNumber"].Value == flight)
                        .ToArray();

                    foreach (var ssr in ssrs)
                    {
                        childMeals.Add(ssr);
                    }
                }
            }

            return childMeals.Count;
        }

        public static int ReturnChildMealCount(string pnr)
        {
            var pnrDetailsXml = ReturnPnrCommonV6Response(pnr);
            var xmlNamespaceManager = pnrDetailsXml.CreateXmlNamespaceManager();

            var childMeals = pnrDetailsXml.SelectNodes("//pnr:SpecialServiceRequest", xmlNamespaceManager).OfType<XmlNode>()
                .Where(x => x.Attributes["RequestCode"].Value == "CHML")
                .ToArray();

            return childMeals.Length;
        }
    }
}
