using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Enum;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public class VacUpdatedAttrubute
    {
        public static List<VacManageAccountUpdateAttributes> FindAttribute(Customers existingDoc, CustomerPersonalInfoUpdateCommand request)
        {
            var changedAttributes = new List<VacManageAccountUpdateAttributes>();

            var existingPhoneNumber = existingDoc.ContactDetails.FirstOrDefault(x => x.ContactType.Is(Constants.Application.PhoneContactType) && x.Primary == "Y")?.ContactValue;

            var existingCountryCode = existingDoc.ContactDetails.FirstOrDefault(x => x.ContactType == Constants.Application.PhoneContactType && x.Primary == "Y")?.CountryCode;

            var existingEmail = existingDoc.ContactDetails.FirstOrDefault(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y")?.ContactValue;

            if (!existingDoc.FirstName.Trim().Is(request.FirstName.Trim()) || !existingDoc.LastName.Trim().Is(request.LastName.Trim()))
            {
                changedAttributes.Add(VacManageAccountUpdateAttributes.Name);
            }

            if (!existingPhoneNumber.Trim().Is(request.PhoneNumber.Trim()) || !existingCountryCode.Trim().Is(request.CountryCode.Trim()))
            {
                changedAttributes.Add(VacManageAccountUpdateAttributes.PhoneNumber);
            }

            if (!DateTime.Parse(existingDoc.DateOfBirth).ToString(Constants.Application.SCVDateFormat).Is(DateTime.Parse(request.DateOfBirth).ToString(Constants.Application.SCVDateFormat)))
            {
                changedAttributes.Add(VacManageAccountUpdateAttributes.DateOfBirth);
            }

            if (!existingEmail.Trim().Is(request.EmailId.Trim()))
            {
                changedAttributes.Add(VacManageAccountUpdateAttributes.Email);
            }

            return changedAttributes;
        }
    }
}
