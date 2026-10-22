using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper
{
    public class TransactionalEventRequestMapper
    {
        public static TransactionalEvent CreateEventGridSchemaData(LoyaltySyncEventProcessCommand enrolmentRequest, bool ocdbFlag)
        {
            return new TransactionalEvent
            {
                Topic = null,
                Id = Guid.NewGuid().ToString(),
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
                            CustomerId = enrolmentRequest.Data.CustomerId,
                            AccountType = enrolmentRequest.Data.MemberType,
                            EnrolmentDateTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            EnrolmentChannel = enrolmentRequest.Data.EnrollmentChannel,
                            CampaignPromotionIdentifier = "",
                            MediaCode = enrolmentRequest.Data.MediaCode,
                            DefaultTierValue = enrolmentRequest.Data.CurrentTier,
                            CampaignCode = enrolmentRequest.Data.CampaignCode,
                            OCDBSyncFlag = ocdbFlag ? "Y" : "A",
                            MemberNumber = enrolmentRequest.Data.FcNumber,
                            Status = enrolmentRequest.Data.Status,
                            Source = Constants.Application.EventGridSource,
                            CurrentTierEndDate =enrolmentRequest.Data.TierEndDate.IsNullOrWhiteSpace() ? null : enrolmentRequest.Data.TierEndDate.Parse<DateTime>(default).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            CurrentTierStartDate = enrolmentRequest.Data.TierStartDate.IsNullOrWhiteSpace() ? null : enrolmentRequest.Data.TierStartDate.Parse<DateTime>(default).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            TestMember = enrolmentRequest.Data.TestMember ? "Y" : "N"
                        }
                    }
                },
                DataVersion = Constants.Application.DataVersion,
                MetadataVersion = null
            };
        }
    }
}
