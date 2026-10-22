using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerAssignAward;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.CustomerAssignAward
{
    public class AssignAwardCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(LoyaltyCustomerAssignAwardCommand loyaltyCustomerAssignAwardCommand)
        {
            return new CloudEvent()
            {
                Data = loyaltyCustomerAssignAwardCommand,
                Type = EnumOperations.GetEnumDescription((CustomerEventType)Enum.Parse(typeof(CustomerEventType), CustomerEventType.AssignLoyaltyAward.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.AssignSwardEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
