using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper
{
    public class TransactionalEventRequestMapper
    {
        public static TransactionalEvent CreateEventGridSchemaData(CreateCustomerEntities enrolmentRequest, string Id)
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
                        TimeStamp = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        ContentType = Constants.Application.ContentType,
                        DataVersion = "",
                        Data = new EventData
                        {
                            CustomerId = enrolmentRequest.CustomerId,
                            AccountType = enrolmentRequest.MemberType,
                            EnrolmentDateTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            EnrolmentChannel = enrolmentRequest.EnrollmentChannel,
                            CampaignPromotionIdentifier = "",
                            MediaCode = enrolmentRequest.MediaCode,
                            DefaultTierValue = enrolmentRequest.CurrentTier,
                            CampaignCode = enrolmentRequest.CampaignCode,
                            MemberNumber = enrolmentRequest.FcNumber,
                            Status = enrolmentRequest.Status,
                            Source = Constants.Application.EventGridSource,
                            CurrentTierEndDate = enrolmentRequest.TierEndDate.IsNullOrWhiteSpace() ? null : enrolmentRequest.TierEndDate.Parse<DateTime>(default).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            CurrentTierStartDate = enrolmentRequest.TierStartDate.IsNullOrWhiteSpace() ? null : enrolmentRequest.TierStartDate.Parse<DateTime>(default).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            TestMember = enrolmentRequest.TestMember == "true" ? "Y" : "N"
                        }
                    }
                },
                DataVersion = Constants.Application.DataVersion,
                MetadataVersion = null
            };
        }
    }
}
