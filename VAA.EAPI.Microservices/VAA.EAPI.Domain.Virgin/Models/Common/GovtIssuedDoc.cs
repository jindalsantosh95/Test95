using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class GovtIssuedDoc
    {
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string CountryOfIssue { get; set; }
        public string CountryOfResidence { get; set; }
        public string ExpirationDate { get; set; }
        public string IssueDate { get; set; }
        public string RegionOfIssuance { get; set; }

        public static explicit operator GovtIssuedDoc(JObject source)
        {
            return new GovtIssuedDoc()
            {
                DocumentNumber = source["DocumentNumber"]?.ToString(),
                DocumentType = source["DocumentType"]?.ToString(),
                CountryOfIssue = source["CountryOfIssue"]?.ToString(),
                CountryOfResidence = source["CountryOfResidence"]?.ToString(),
                ExpirationDate = source["ExpirationDate"]?.ToString(),
                IssueDate = source["IssueDate"]?.ToString(),
                RegionOfIssuance = source["RegionOfIssuance"]?.ToString()
            };
        }
    }
}
