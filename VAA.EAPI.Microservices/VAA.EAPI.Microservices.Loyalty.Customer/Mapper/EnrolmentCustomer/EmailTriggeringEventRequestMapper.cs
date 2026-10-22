using System;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class EmailTriggeringEventRequestMapper
    {
        public static EmailTriggeringEventRequest CreateEventGridSchemaData(CreateCustomerEntities enrolmentRequest)
        {
            return new EmailTriggeringEventRequest
            {
                Topic = null,
                Id = Guid.NewGuid().ToString(),
                EventType = Constants.Application.Event,
                Subject = Constants.Application.EventSubject,
                EventTime = DateTime.UtcNow,
                Data = new SendEmailRequest()
                {
                    DataFields = new DataFields()
                    {
                        Customer_FirstName = enrolmentRequest?.FirstName,
                        Customer_LastName = enrolmentRequest?.LastName,
                        Customer_Title = enrolmentRequest?.Title,
                        Membership_Number = enrolmentRequest?.CustomerId,
                        Member_First_Name = enrolmentRequest?.FirstName,
                    },
                    To = enrolmentRequest?.ContactDetails?.EmailDetails.Find(x => x.PreferredFlag == "Y").EmailId
                }
            };
        }
    }
}
