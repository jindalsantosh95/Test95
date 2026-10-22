using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Mapper
{
    public class WalletEventGridMapper
    {
        public static CloudEvent WalletDataMapper(int recordCount,string correlationId)
        {
            return new CloudEvent
            {
                Id = correlationId,
                Subject = Constants.Application.WalletNotification,
                Data = recordCount,
                Source = Constants.Application.MemberTierUpdateEventTopicName,
                Type = Constants.Application.WalletNotification,
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
