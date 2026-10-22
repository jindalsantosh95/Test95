using System.Security.Cryptography.X509Certificates;
using System.Xml;
using VAA.EAPI.Automation.Core.Delta.Endpoint;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Delta.Helpers
{
    public static class AIR4Helper
    {
        public static XmlDocument LoyaltyProfileSyncAdd(X509Certificate2 dlCertificate, string memberNumber, string dateOfBirth, string firstName, string lastName, string gender, string email, out string requestLoyaltyProfileSyncAdd)
        {
            var loyaltyProfileSyncAdd = Soa.LoyaltyProfileSyncAdd(dlCertificate, memberNumber, dateOfBirth, firstName, lastName, gender, email, out requestLoyaltyProfileSyncAdd).EnsureIsSuccessStatusCode();

            string result = loyaltyProfileSyncAdd.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument loyaltyProfileSyncAddResponseDocument = new();

            loyaltyProfileSyncAddResponseDocument.LoadXml(result);

            return loyaltyProfileSyncAddResponseDocument;
        }
    }
}