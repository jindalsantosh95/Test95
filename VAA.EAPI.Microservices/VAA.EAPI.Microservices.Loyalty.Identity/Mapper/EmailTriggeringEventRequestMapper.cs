using System;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class EmailTriggeringEventRequestMapper
    {

        public static EmailTriggeringEventRequest CreateEventGridSchemaDatawithWelcomeEmail(CreateCustomerEntities request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new EmailTriggeringEventRequest
            {
                Id = Guid.NewGuid().ToString(),
                EventType = Constants.Application.EmailCreateEvent,
                Subject = Constants.Application.ActivationLinkemail,
                EventTime = DateTime.UtcNow,
                Data = new SendEmailRequest
                {
                    Template = ConfigReader.Get(Constants.Environment.WelcomeEmailTemplate),
                    DataFields = new DataFields
                    {
                        Membership_Number = request.FcNumber ?? request.FlyingClubCardNumber,
                        Customer_FirstName = request.FirstName,
                        Member_Name = request.FirstName,
                        Customer_LastName = request.LastName,
                        Territory = request.AddressDetails.First().CountryCode,
                        Tier = Constants.Application.TierCode,
                        Virgin_Points = "0"
                    },
                    To = ConfigReader.Get(Constants.Environment.AllowTestEmail) == "Y"
                        ? ConfigReader.Get(Constants.Environment.TestEmailId)
                        : request.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId
                }
            };
        }
    }
}
