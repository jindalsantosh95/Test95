using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    public class EnrolCustomerDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PreferredFirstName { get; set; }
        public string PreferredLastName { get; set; }
        public string PreferredGender { get; set; }
        public string PreferredName { get; set; }
        public string PreferredSalutation { get; set; }
        public string PreferredPronouns { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string Salutation { get; set; }
        public string Nationality { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum CustomerTypeEnum
        {
            [EnumMember(Value = "Basic")]
            Basic = 0,
            [EnumMember(Value = "Loyalty")]
            Loyalty = 1
        }
        public CustomerTypeEnum? CustomerType { get; set; }
        public string DateOfBirth { get; set; }
        public string Password { get; set; }
        public IEnumerable<Address> AddressDetails { get; set; }
        public Contact ContactDetails { get; set; }
        public LoyaltyMember LoyaltyMember { get; set; }
        public TravelProfile TravelProfile { get; set; }
        public IEnumerable<Preferences> Preferences { get; set; }
        public IEnumerable<CustomerAttribute> CustomerAttribute { get; set; }

        public static explicit operator EnrolCustomerDetails(UpgradeToFlyingClubCommand req)
        {
            var addressDetails = req.CustomerDetails.AddressDetails
                .Select(addressDetail => new Address
                {
                    AddressLine1 = addressDetail.AddressLine1,
                    AddressLine2 = addressDetail.AddressLine2,
                    AddressType = (Address.AddressTypes?)addressDetail.AddressType,
                    PostalCode = addressDetail.PostalCode,
                    City = addressDetail.City
                });

            var phoneDetails = req.CustomerDetails.ContactDetails.PhoneDetails.ToList()
                .Select(phoneDetail => new Phone
                {
                    PhoneType = (Phone.PhoneTypes?)phoneDetail.PhoneType,
                    PhoneNumber = phoneDetail.PhoneNumber,
                });

            var emailDetails = req.CustomerDetails.ContactDetails.EmailDetails.ToList()
                .Select(emailDetail => new Email
                {
                    EmailId = emailDetail.EmailId,
                });

            return new EnrolCustomerDetails()
            {
                FirstName = req.CustomerDetails.FirstName,
                LastName = req.CustomerDetails.LastName,
                MiddleName = req.CustomerDetails.MiddleName,
                Title = req.CustomerDetails.Title,
                Gender = req.CustomerDetails.Gender,
                AddressDetails = addressDetails,
                CustomerType = (CustomerTypeEnum?)req.CustomerDetails.CustomerType,
                LoyaltyMember = new()
                {
                    MemberNumber = req.CustomerDetails.FcNumber,
                    EnrollmentChannel = req.CustomerDetails.EnrollmentChannel
                },
                ContactDetails = new()
                {
                    PhoneDetails = phoneDetails.ToList(),
                    EmailDetails = emailDetails.ToList()
                },
                DateOfBirth = req.CustomerDetails.DateOfBirth               
            };
        }
    }
}
