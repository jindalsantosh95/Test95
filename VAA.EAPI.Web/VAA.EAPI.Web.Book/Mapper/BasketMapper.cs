using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Book.Mapper
{
    public static class BasketMapper
    {
        private static IEnumerable<string> _tncText;

        public static YourDetailsResponse YourDetailsMapper(YourDetailsResponse yourDetailsResponse)
        {
            return new()
            {
                Link = ConfigReader.Get(Constants.Environment.BasketLink),
                Validation = yourDetailsResponse.Validation,
                AddressDetails = new(),
                ConfigurationDetails = new(),
                MembershipDetails = new(),
                ContactDetails = ContactMapper(yourDetailsResponse.ContactDetails),
                People = PeopleMapper(yourDetailsResponse.People),
                TermsAndConditions = TncMapper(yourDetailsResponse.TermsAndConditions),
                VoucherDetails = new(),
                MilesDetails = new(),
                PaymentDetails = new(),
                AffinityPartnerDetail = new(),
                PromotionalCodeDetails = new(),
                UpsellOptions = new(),
                Summary = yourDetailsResponse.Summary,
                ResourceType = Constants.Application.Booking,
                Errata = RetrieveTnCText(_tncText),
                PaymentLink = ConfigReader.Get(Constants.Environment.PaymentLink),
                CustomerProfile = yourDetailsResponse.CustomerProfile,
                EarnPoints = yourDetailsResponse.EarnPoints,
                IsInsuranceAdded = yourDetailsResponse.IsInsuranceAdded,
                PointOfSale = yourDetailsResponse.PointOfSale,
                IsCoBrandEligible = yourDetailsResponse.IsCoBrandEligible,
                CoBrandStatus = yourDetailsResponse.CoBrandStatus,
                FcAutoMatched = yourDetailsResponse.FcAutoMatched
            };
        }

        public static IEnumerable<Person> PeopleMapper(IEnumerable<Person> people)
        {
            foreach (var item in people ?? Enumerable.Empty<Person>())
            {
                item.Link = $"{ConfigReader.Get(Constants.Environment.PassengerLink)}{item.Id}";
                item.Age = null;
                item.ProfilePassenger = item.ProfilePassenger ?? false;
                item.DateOfBirth = item.DateOfBirth != null ? DateTime.ParseExact(item.DateOfBirth, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") : null;
                item.MembershipDetails.Link = item.Link;
            }

            return people;
        }

        public static ContactDetails ContactMapper(ContactDetails contactDetails)
        {
            if (contactDetails != null)
                contactDetails.Link = ConfigReader.Get(Constants.Environment.ContactLink);

            return contactDetails;
        }

        public static TermsAndConditions TncMapper(TermsAndConditions termsAndConditions)
        {
            if (termsAndConditions != null)
                termsAndConditions.Link = ConfigReader.Get(Constants.Environment.TermsLink);

            return termsAndConditions;
        }

        public static IEnumerable<string> RetrieveTnCText(IEnumerable<string> value)
        {
            if (value.IsNullOrEmpty())
                _tncText = File.ReadAllLines("GraphQlMutation/Errata.txt");

            return _tncText;
        }
    }
}
