using System.Runtime.InteropServices;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class EnhancedEndTransactionRequestMapper
    {
        public static EnhancedEndTransactionRQRequest GetEnhancedEndTransactionRequest(string token, [Optional] bool isScheduleChangeInd)
        {
            return new()
            {
                EnhancedEndTransactionRQ = new()
                {
                    version = Constants.Application.EnhancedEndTransactionRQVersion,
                    Source = new() { ReceivedFrom = Constants.Application.SabreSource },
                    EndTransaction = new() { Ind = true, ScheduleChange = isScheduleChangeInd ? new() { Ind = true } : null }
                },
                MessageHeader = new()
                {
                    Action = Constants.Application.EnhancedEndTransactionAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }
            };
        }
    }
}
