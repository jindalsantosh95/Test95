using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class TransactionalEventRequestMapper
    {
        public static TransactionalEvent CreateEventGridSchemaData(CreateCustomerEventResponse request, string Id)
        {
            return new TransactionalEvent
            {
                Topic = null,
                Id = Id,
                EventType = Constants.Application.EventType,
                Subject = Constants.Application.EventSubject,
                EventTime = DateTime.UtcNow,
                Data = new TransactionalEventData
                {
                    CustomerLoyaltyEvent = new CustomerLoyaltyEvent
                    {
                        SpecVersion = Constants.Application.SpecVersion,
                        EventType = Constants.Application.EventType,
                        Source = ConfigReader.Get(Constants.Environment.Source),
                        Subject = Constants.Application.EventSubject,
                        EventId = Guid.NewGuid().ToString(),
                        TimeStamp = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                        ContentType = Constants.Application.ContentType,
                        DataVersion = "",
                        Data = new DataClass
                        {
                            CustomerId = request.CustomerId,
                            AccountType = request.MemberType,
                            EnrolmentDateTime = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                            EnrolmentChannel = request.EnrollmentChannel,
                            CampaignPromotionIdentifier = "",
                            MediaCode = request.MediaCode,
                            DefaultTierValue = request.CurrentTier,
                            CampaignCode = request.CampaignCode,
                            MemberNumber = request.FcNumber,
                            Status = request.Status,
                            Source = Constants.Application.EventGridSource,
                            CurrentTierEndDate = request.TierEndDate.IsNullOrWhiteSpace() ? null : request.TierEndDate.Parse<DateTime>(default).ToString(Constants.Application.ISODateTimeFormat),
                            CurrentTierStartDate = request.TierStartDate.IsNullOrWhiteSpace() ? null : request.TierStartDate.Parse<DateTime>(default).ToString(Constants.Application.ISODateTimeFormat),
                            TestMember = request.TestMember == "true" ? "Y" : "N"
                        }
                    }
                },
                DataVersion = Constants.Application.DataVersion,
                MetadataVersion = null
            };
        }
    }
}
