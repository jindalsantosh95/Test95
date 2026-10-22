using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Mapper
{
    public class WalletSubToCommandMapper
    {
        public static SendPushNotificationCommand WalletEventRequestMapper(CloudEvent walletRequestEventMessage)
        {
            var reqData = Json.Deserialize<CustomerTierEntity>(Convert.ToString(walletRequestEventMessage.Data));

            return new SendPushNotificationCommand
            {
                CustomerId = reqData.CustomerId,
                TierCode = reqData.NewTier
            };
        }
        public static SendPushNotificationCommand MemberTierUpdateEventRequestMapper(CloudEvent walletRequestEventMessage)
        {
            var reqData = Json.Deserialize<MembershipTierChangeDocument>(Convert.ToString(walletRequestEventMessage.Data));

            return new SendPushNotificationCommand
            {
                CustomerId = reqData.MembershipChangeDocument.CustomerId,
                TierCode = reqData.MembershipChangeDocument.TierCode
            };
        }
    }
}
