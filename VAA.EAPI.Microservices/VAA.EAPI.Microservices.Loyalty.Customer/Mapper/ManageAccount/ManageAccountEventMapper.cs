using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class ManageAccountEventMapper
    {
        public static CloudEvent AddressUpdateDataMapper(string customerID, AccountAddressDetail newAddressDetails, string id)
        {
            return new CloudEvent
            {
                Id = id,
                Subject = Constants.Application.UpdateManageAccountAddressHHSubject,
                Data = new UpdateMemberAddressCommand
                {
                    MemberId = customerID,
                    AddressDetail = newAddressDetails,
                    Id = id
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateManageAccountAddressHHSubject,
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent PhoneNumberUpdateDataMapper(UpdatePhoneNumberEventCommand phoneNumberEventCommand, string id)
        {
            return new CloudEvent
            {
                Id = id,
                Subject = Constants.Application.UpdateManageAccountPhoneHHSubject,
                Data = new UpdatePhoneNumberCommand
                {
                    CustomerID = phoneNumberEventCommand.CustomerID,
                    FcNumber = phoneNumberEventCommand.FcNumber,
                    PhoneNumber = phoneNumberEventCommand.PhoneNumber,
                    CountryCode = phoneNumberEventCommand.CountryCode,
                    CorrelationId = id
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateManageAccountPhoneHHSubject,
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
