using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Domain.Virgin.Models.Customer
{
    public class Customer
    {
        public string CustId { get; set; }
        public string CustIdNdc { get; set; }
        public string Title { get; set; }
        public string Ptc { get; set; }
        public string ItaPtc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public Contacts Contacts { get; set; }
        public Address Addresses { get; set; }
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
        public Employer Employer { get; set; }
        public List<TravelDocument> TravelDocuments { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }
        public IEnumerable<GovtIssuedDoc> GovtIssuedDocs { get; set; }
        public Preferences Preferences { get; set; }
        public string LinkedInfantId { get; set; }
        public string AssociatedWithAdult { get; set; }
        public bool IsNameChanged { get; set; }
        public bool IsApisAdded { get; set; }
        public bool IsEmailUpdates { get; set; }

        public static explicit operator Customer(JObject source)
        {
            return new Customer()
            {
                CustId = source["CustId"]?.ToString(),
                CustIdNdc = source["CustIdNdc"]?.ToString(),
                Title = source["Title"]?.ToString(),
                Ptc = source["Ptc"]?.ToString(),
                FirstName = source["FirstName"]?.ToString(),
                LastName = source["LastName"]?.ToString(),
                Gender = source["Gender"]?.ToString(),
                DateOfBirth = source["DateOfBirth"]?.ToString(),
                Contacts = source["Contacts"] != null ? (Contacts)source["Contacts"] : null,
                Addresses = source["Addresses"] != null ? (Address)source["Addresses"] : null,
                LoyaltyMemberDetails = source["LoyaltyMemberDetails"] != null ? (LoyaltyMemberDetails)source["LoyaltyMemberDetails"] : null,
                TravelDocuments = source["TravelDocuments"]?.Select(a => (TravelDocument)a).ToList(),
                FirstNameNumber = source["FirstNameNumber"]?.ToString(),
                LastNameNumber = source["LastNameNumber"]?.ToString(),
                GovtIssuedDocs = source["GovtIssuedDocs"]?.Select(a => (GovtIssuedDoc)a),
                Preferences = source["Preferences"] != null ? (Preferences)source["Preferences"] : null,
                LinkedInfantId = source["LinkedInfantId"]?.ToString(),
                AssociatedWithAdult = source["AssociatedWithAdult"]?.ToString(),
                IsNameChanged = source["IsNameChanged"] != null ? (bool)source["IsNameChanged"] : false
            };
        }
    }
}
