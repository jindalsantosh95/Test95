using Newtonsoft.Json.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class TravelDocument
    {
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string DateOfBirth { get; set; }
        public string CountryOfIssue { get; set; }
        public string CountryOfResidence { get; set; }
        public PersonName PersonName { get; set; }
        public string ExpirationDate { get; set; }
        public string Gender { get; set; }
        public string IssueDate { get; set; }
        public bool Verified { get; set; }
        public string RegionOfIssuance { get; set; }
        public string Nationality { get; set; }

        public static explicit operator TravelDocument(JObject source)
        {
            return new TravelDocument()
            {
                DocumentNumber = source["DocumentNumber"]?.ToString(),
                DocumentType = source["DocumentType"]?.ToString(),
                DateOfBirth = source["DateOfBirth"]?.ToString(),
                CountryOfIssue = source["CountryOfIssue"]?.ToString(),
                CountryOfResidence = source["CountryOfResidence"]?.ToString(),
                PersonName = source["PersonName"] != null ? (PersonName)source["PersonName"] : null,
                ExpirationDate = source["ExpirationDate"]?.ToString(),
                Gender = source["Gender"]?.ToString(),
                IssueDate = source["IssueDate"]?.ToString(),
                Verified = source["Verified"] != null ? (bool)source["Verified"] : false,
                RegionOfIssuance = source["RegionOfIssuance"]?.ToString(),
                Nationality = source["Nationality"]?.ToString()
            };
        }
    }
}
