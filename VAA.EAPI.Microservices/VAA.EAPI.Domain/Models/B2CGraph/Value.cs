using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.Domain.Models.B2CGraph
{
    public class Value
    {
        public bool AccountEnabled { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Department { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string JobTitle { get; set; }
        public string Mobile { get; set; }
        public string PostalCode { get; set; }
        public string PreferredLanguage { get; set; }
        public List<SignInName> SignInNames { get; set; }
        public string State { get; set; }
        public string StreetAddress { get; set; }
        public string Surname { get; set; }
        public string TelephoneNumber { get; set; }
        public string UserPrincipalName { get; set; }
        public string UserType { get; set; }
        public string Id { get; set; }
        public List<IdentityType> Identities { get; set; }
        public string[] ProxyAddresses { get; set; }
        
        [JsonExtensionData] 
        public Dictionary<string, JToken> AdditionalData { get; set; }

    }
}
