using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class Customer
    {
        public Customer(string firstName = default, string lastName = default, string middleName = default, DateTime? dateOfBirth = default, string email = default, string gender = default, string title = default, string preferredSalutation = default, string salutation = default, string nationality = default, string customerType = default, string telephonePassword = default, string pnrName = default, string envelopeSalutation = default, string nameChangeReason = default, string amendedIn30Days = default, Contact contactDetails = default, List<Address> addressDetails = default, LoyaltyMember loyaltyMember = default, TravelProfile travelProfile = default, List<CustomerAttribute> customerAttribute = default, ContactNote contactNote = default, MemberAttribute memberAttribute = default, ChildMember childMember = default, List<Preferences> preferences = default)
        {
            FirstName = firstName;
            LastName = lastName;
            MiddleName = middleName;
            DateOfBirth = dateOfBirth;
            Email = email;
            Gender = gender;
            Title = title;
            PreferredSalutation = preferredSalutation;
            Salutation = salutation;
            Nationality = nationality;
            CustomerType = customerType;
            TelephonePassword = telephonePassword;
            PnrName = pnrName;
            EnvelopeSalutation = envelopeSalutation;
            NameChangeReason = nameChangeReason;
            AmendedIn30Days = amendedIn30Days;
            ContactDetails = contactDetails;
            AddressDetails = addressDetails;
            LoyaltyMember = loyaltyMember;
            TravelProfile = travelProfile;
            CustomerAttribute = customerAttribute;
            ContactNote = contactNote;
            MemberAttribute = memberAttribute;
            ChildMember = childMember;
            Preferences = preferences;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string PreferredSalutation { get; set; }
        public string Salutation { get; set; }
        public string Nationality { get; set; }
        public string CustomerType { get; set; }
        public string TelephonePassword { get; set; }
        public string PnrName { get; set; }
        public string EnvelopeSalutation { get; set; }
        public string NameChangeReason { get; set; }
        public string AmendedIn30Days { get; set; }
        public Contact ContactDetails { get; set; }
        public List<Address> AddressDetails { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public TravelProfile TravelProfile { get; set; }
        public List<CustomerAttribute> CustomerAttribute { get; set; }
        public ContactNote ContactNote { get; set; }
        public MemberAttribute MemberAttribute { get; set; }
        public ChildMember ChildMember { get; set; }
        public List<Preferences> Preferences { get; set; }
    }
}
