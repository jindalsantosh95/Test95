using System;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Domain;

namespace VAA.EAPI.Microservices.Retail.Basket.Validation
{
    public class ValidateBasket
    {
        public static UpdatePassengerCommand ValidatePassengerDetails(UpdatePassengerCommand updatePassenger, Domain.Basket basket)
        {

            updatePassenger.People.Validation = Constants.Application.ValidStatus;
            #region Validate First Name

            updatePassenger.People.FirstName = CheckNameValidation(Constants.Application.FirstNameRegex, updatePassenger.People.FirstName.Trim(), out bool firstNamestatus);
            if (!firstNamestatus)
            {
                updatePassenger.People.Validation = Constants.Application.InvalidStatus;
                return updatePassenger;
            }
            #endregion

            #region Validate Last Name

            updatePassenger.People.LastName = CheckNameValidation(Constants.Application.LastNameRegex, updatePassenger.People.LastName.Trim(), out bool lastNamestatus);
            if (!lastNamestatus)
            {
                updatePassenger.People.Validation = Constants.Application.InvalidStatus;
                return updatePassenger;
            }
            #endregion

            #region Validate no of passengers

            if (basket.People.Count() <= Convert.ToInt32(updatePassenger.People.Id))
            {
                updatePassenger.People.Validation = Constants.Application.InvalidStatus;

                return updatePassenger;
            }
            #endregion

            #region Validate duplicate passengers

            foreach (var people in basket.People)
            {
                if (updatePassenger.People.Id == people.Id) continue;

                if (IsDuplicatePassenger(updatePassenger.People, people))
                    updatePassenger.People.Validation = Constants.Application.InvalidStatus;

                return updatePassenger;
            }
            #endregion            
            return updatePassenger;
        }

        public static UpdateContactDetailCommand ValidateContactDetail(UpdateContactDetailCommand updateContact, Domain.Basket basket)
        {
            updateContact.ContactDetails.Validation = Constants.Application.ValidStatus;
            updateContact.ContactDetails.PhoneNumber = updateContact.ContactDetails.PhoneNumber.IsNullOrEmpty() ? null : RegExHelper.RemoveWhiteSpace(updateContact.ContactDetails.PhoneNumber.Trim());

            if (string.IsNullOrWhiteSpace(updateContact.ContactDetails.EmailAddress) || !RegexCompiled.IsMatch(updateContact.ContactDetails.EmailAddress, Constants.Application.EmailRegex, RegexOptions.IgnoreCase))
            {
                updateContact.ContactDetails.Validation = Constants.Application.InvalidStatus;
                return updateContact;
            }

            if (!updateContact.ContactDetails.PeopleId.IsNullOrEmpty() && !basket.People.Any(p => p.Id == updateContact.ContactDetails.PeopleId))
            {
                updateContact.ContactDetails.Validation = Constants.Application.InvalidStatus;
            }

            if (string.IsNullOrWhiteSpace(updateContact.ContactDetails.PhoneNumber) || !RegexCompiled.IsMatch(updateContact.ContactDetails.PhoneNumber, Constants.Application.PhoneRegex))
            {
                updateContact.ContactDetails.Validation = Constants.Application.InvalidStatus;
            }

            if (string.IsNullOrWhiteSpace(updateContact.ContactDetails.CountryCode))
            {
                updateContact.ContactDetails.Validation = Constants.Application.InvalidStatus;
            }


            return updateContact;
        }

        public static UpdateTnCCommand ValidateTnC(UpdateTnCCommand updateTnC)
        {
            updateTnC.TermsAndConditions.Validation = !updateTnC.TermsAndConditions.Accepted ? Constants.Application.InvalidStatus : Constants.Application.ValidStatus;

            return updateTnC;
        }


        public static Domain.Basket ValidateBasketStatus(Domain.Basket updatedBasket)
        {
            updatedBasket.Validation = updatedBasket.ContactDetails.Validation == Constants.Application.ValidStatus && updatedBasket.People.Any() && updatedBasket.People.Select(x => x.Validation).All(x => x == Constants.Application.ValidStatus) && updatedBasket.TermsAndConditions.Validation == Constants.Application.ValidStatus ? Constants.Application.ValidStatus : Constants.Application.InvalidStatus;

            updatedBasket.Validation = updatedBasket.InsuranceQuotes.IsNullOrEmpty() ? updatedBasket.Validation : updatedBasket.InsuranceQuotes.Any() && updatedBasket.IsInsuranceAdded == null ? Constants.Application.InvalidStatus : updatedBasket.Validation ;

            return updatedBasket;
        }

        private static bool IsDuplicatePassenger(Person newPassenger, Person existingPassenger)
        {
            return newPassenger.FirstName.Equals(existingPassenger.FirstName) && newPassenger.LastName.Equals(existingPassenger.LastName)
                                                                              && newPassenger.DateOfBirth.Equals(existingPassenger.DateOfBirth) && newPassenger.Gender.Equals(existingPassenger.Gender)
                                                                              && newPassenger.Title.Equals(existingPassenger.Title);
        }
        public static string CheckNameValidation(string NameRegex, string Name, out bool Status)
        {
            Status = true;
            Name = RegExHelper.RemoveSpecificCharacter(Name);
            Name = RegExHelper.RemoveSpaceMorethanOnce(Name);
            if (string.IsNullOrWhiteSpace(Name) || !RegexCompiled.IsMatch(Name, NameRegex))
                Status = false;
            return Name.Trim();
        }
    }
}

