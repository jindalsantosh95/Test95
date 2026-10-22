using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MillionMiler;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.MillionMiler
{
    public class MillionMilerTransactionEventMapper
    {
        public static TransactionCreateEventData MapMillionMilerTransactionCreateEventData(CustomerDTO customer, string partnerId, Domain.Entities.BirthdayBonus.Product productDetail)
        {
            TransactionDocument transactionDocument = new TransactionDocument
            {
                CustomerId = customer.Customer?.CustomerId,
                CreatedBy = Constants.Application.MillionMilerUser,
                LastUpdatedBy = Constants.Application.MillionMilerUser,
                MemberNumber = customer.Customer?.LoyaltyNumber,
                TransactionDate = DateTime.UtcNow.Date.ToString(Constants.Application.TxnDateTimeFormat),
                ProductId = productDetail.ProductId,
                PartnerId = partnerId,
                TransactionType = Constants.Application.AccrualTrxnType,
                TransactionSubType = Constants.Application.AccrualTrxnSubType,
                TransactionValue = Constants.Application.TransactionValue,
                TransactionChannel = Constants.Application.MillionMilerTrxnChannel,
                ActivityDate = DateTime.UtcNow.Date.ToString(Constants.Application.TxnDateTimeFormat),
                StatusCode = Constants.Application.SuccessTrxnStatus,
                StatementText = Constants.Application.StatementText,
                CreditDetails = new List<Creditdetail>
                {
                    new Creditdetail
                    {
                        CreatedBy = Constants.Application.MillionMilerUser,
                        LastUpdatedBy = Constants.Application.MillionMilerUser,
                        CreditValue= Constants.Application.TransactionValue,
                        PromotionRuleActionId=null,
                        PromotionId=null,
                        PointType= Constants.Application.MilesPointType,
                        PointSubType=Constants.Application.BaseMilesPointSubType,
                        QualifyingFlag = Constants.Application.MilesQualifyingFlag
                    }
                }
            };
            return new TransactionCreateEventData
            {
                TransactionDocument = transactionDocument
            };
        }

        public static CloudEvent MapTxnDocCreateEvent(TransactionCreateEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            return new CloudEvent()
            {
                Data = eventData,
                Type = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.TransactionCreateActionExecution.ToString())),
                Source = ConfigReader.Get(Constants.Environment.CustomerTransactionTopicName),
                Subject = EnumOperations.GetEnumDescription((LoyaltyMemberEventType)Enum.Parse(typeof(LoyaltyMemberEventType), LoyaltyMemberEventType.TransactionCreateActionExecution.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
