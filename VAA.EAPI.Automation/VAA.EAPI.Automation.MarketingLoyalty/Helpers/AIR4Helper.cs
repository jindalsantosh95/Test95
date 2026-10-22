using System.Security.Cryptography.X509Certificates;
using System.Xml;
using VAA.EAPI.Automation.Core.Delta.Endpoint;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MarketingLoyalty.Helpers
{
    public static class AIR4Helper
    {
        public static XmlDocument SFPDUpdate(X509Certificate2 dlCertificate, string airlineCode, string dateOfBirth, string firstName, string lastName, string gender, string fcNumber, out string requestSFPDUpdate)
        {
            HttpResponseMessage sfpdUpdate = Soa.SFPDUpdate(dlCertificate, airlineCode, dateOfBirth, firstName, lastName, gender, fcNumber, out requestSFPDUpdate).EnsureIsSuccessStatusCode();

            string result = sfpdUpdate.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument sfpdUpdateResponseDocument = new();

            sfpdUpdateResponseDocument.LoadXml(result);

            return sfpdUpdateResponseDocument;
        }

        public static XmlDocument SFPDRetrieve(X509Certificate2 dlCertificate, string airlineCode, string transactionId, string fcNumber, out string requestSFPDRetrieve)
        {
            HttpResponseMessage sfpdRetrieve = Soa.SFPDRetrieve(dlCertificate, airlineCode, transactionId, fcNumber, out requestSFPDRetrieve).EnsureIsSuccessStatusCode();

            string result = sfpdRetrieve.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument sfpdRetrieveResponseDocument = new();

            sfpdRetrieveResponseDocument.LoadXml(result);

            return sfpdRetrieveResponseDocument;
        }
    }
}
