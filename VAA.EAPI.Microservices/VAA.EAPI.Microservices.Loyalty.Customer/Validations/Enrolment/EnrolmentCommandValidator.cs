using FluentValidation;
using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using static VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.EnrolCustomerDetails;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public class EnrolmentCommandValidator : AbstractValidator<EnrolmentRequestCommand>
    {
        public EnrolmentCommandValidator()
        {
            RuleFor(x => x.Data.CustomerDetails.FirstName).NotNull().NotEmpty();
            RuleFor(x => x.Data.CustomerDetails.LastName).NotNull().NotEmpty();
            RuleFor(x => x.Data.CustomerDetails.Title).NotNull().NotEmpty().When(y => y.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty && y.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.PowerAppChannelID));
            RuleFor(x => x.Data.CustomerDetails.AddressDetails.FirstOrDefault().AddressLine1).NotNull().NotEmpty()
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);
            RuleFor(x => x.Data.CustomerDetails.AddressDetails.FirstOrDefault().AddressType).NotNull().NotEmpty()
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);
            RuleFor(x => x.Data.CustomerDetails.AddressDetails.FirstOrDefault().PostalCode).NotNull().NotEmpty()
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty && type.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.DeltaChannelID) && type.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.WebMyaChannelId) && type.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.AppMyaChannelId));
            RuleFor(x => x.Data.CustomerDetails.AddressDetails.FirstOrDefault().City).NotNull().NotEmpty()
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);


            RuleFor(x => x.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel).NotNull().NotEmpty();

            RuleFor(x => x.Data.CustomerDetails.CustomerType).NotNull().NotEmpty();


            RuleFor(x => x.Data.CustomerDetails.ContactDetails.PhoneDetails.All(A => A.PhoneType.HasValue)).NotNull().NotEmpty()
                .When(phone => phone.Data.CustomerDetails.ContactDetails.PhoneDetails != null && phone.Data.CustomerDetails.ContactDetails.PhoneDetails.All(b => !string.IsNullOrEmpty(b.PhoneNumber)) && phone.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);


            RuleFor(x => x.Data.CustomerDetails.ContactDetails.PhoneDetails.All(A => string.Compare(A.PhoneNumber, " ") > 0)).NotNull().NotEmpty()
               .When(phone => phone.Data.CustomerDetails.ContactDetails.PhoneDetails != null && phone.Data.CustomerDetails.ContactDetails.PhoneDetails.All(b => b.PhoneType.HasValue) && phone.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);


            RuleFor(x => x.Data.CustomerDetails.ContactDetails.PhoneDetails.All(A => string.Compare(A.PhoneNumber, "") > 0)).NotNull().NotEmpty()
              .When(phone => phone.Data.CustomerDetails.ContactDetails.PhoneDetails != null && phone.Data.CustomerDetails.ContactDetails.PhoneDetails.All(b => b.PhoneType.HasValue) && phone.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);

            RuleFor(x => x.Data.CustomerDetails.DateOfBirth).NotNull().NotEmpty().Must(BeAValidDate)
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Loyalty);

            RuleFor(x => x.Data.CustomerDetails.ContactDetails.EmailDetails.All(mail => string.Compare(mail.EmailId, " ") > 0)).NotNull().NotEmpty()
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Basic);

            RuleFor(x => x.Data.CustomerDetails.ContactDetails.EmailDetails.All(mail => string.Compare(mail.EmailId, "") > 0)).NotNull().NotEmpty()
                .When(type => type.Data.CustomerDetails.CustomerType == EnrolCustomerDetails.CustomerTypeEnum.Basic);

        }
        private bool BeAValidDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date);
        }

    }
}
