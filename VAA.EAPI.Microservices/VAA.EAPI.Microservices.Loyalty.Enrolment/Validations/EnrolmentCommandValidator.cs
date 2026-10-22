using FluentValidation;
using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using static VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails.Customer;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public class EnrolmentCommandValidator : AbstractValidator<EnrolmentRequest>
    {
        public EnrolmentCommandValidator()
        {
            RuleFor(x => x.CustomerDetails.FirstName).NotNull().NotEmpty();
            RuleFor(x => x.CustomerDetails.LastName).NotNull().NotEmpty();
            RuleFor(x => x.CustomerDetails.Title).NotNull().NotEmpty().When(y => y.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty && y.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.PowerAppChannelID));
            RuleFor(x => x.CustomerDetails.AddressDetails.FirstOrDefault().AddressLine1).NotNull().NotEmpty()
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);
            RuleFor(x => x.CustomerDetails.AddressDetails.FirstOrDefault().AddressType).NotNull().NotEmpty()
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);
            RuleFor(x => x.CustomerDetails.AddressDetails.FirstOrDefault().PostalCode).NotNull().NotEmpty()
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty && (type.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.DeltaChannelID) || type.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.WebMyaChannelId)));
            RuleFor(x => x.CustomerDetails.AddressDetails.FirstOrDefault().City).NotNull().NotEmpty()
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);


            RuleFor(x => x.CustomerDetails.LoyaltyMember.EnrollmentChannel).NotNull().NotEmpty();

            RuleFor(x => x.CustomerDetails.CustomerType).NotNull().NotEmpty();


            RuleFor(x => x.CustomerDetails.ContactDetails.PhoneDetails.All(A => A.PhoneType.HasValue)).NotNull().NotEmpty()
                .When(phone => phone.CustomerDetails.ContactDetails.PhoneDetails != null && phone.CustomerDetails.ContactDetails.PhoneDetails.All(b => !string.IsNullOrEmpty(b.PhoneNumber)) && phone.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);


            RuleFor(x => x.CustomerDetails.ContactDetails.PhoneDetails.All(A => string.Compare(A.PhoneNumber, " ") > 0)).NotNull().NotEmpty()
               .When(phone => phone.CustomerDetails.ContactDetails.PhoneDetails != null && phone.CustomerDetails.ContactDetails.PhoneDetails.All(b => b.PhoneType.HasValue) && phone.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);


            RuleFor(x => x.CustomerDetails.ContactDetails.PhoneDetails.All(A => string.Compare(A.PhoneNumber, "") > 0)).NotNull().NotEmpty()
              .When(phone => phone.CustomerDetails.ContactDetails.PhoneDetails != null && phone.CustomerDetails.ContactDetails.PhoneDetails.All(b => b.PhoneType.HasValue) && phone.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);

            RuleFor(x => x.CustomerDetails.DateOfBirth).NotNull().NotEmpty().Must(BeAValidDate)
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Loyalty);

            RuleFor(x => x.CustomerDetails.ContactDetails.EmailDetails.All(mail => string.Compare(mail.EmailId, " ") > 0)).NotNull().NotEmpty()
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Basic);

            RuleFor(x => x.CustomerDetails.ContactDetails.EmailDetails.All(mail => string.Compare(mail.EmailId, "") > 0)).NotNull().NotEmpty()
                .When(type => type.CustomerDetails.CustomerType == CustomerTypeEnum.Basic);

        }
        private bool BeAValidDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date);
        }

    }
}
