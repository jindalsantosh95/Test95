using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.QueueAccess;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Mapper
{
    public static class QueueAccessRequestMapper
    {
        public static QueueAccessRQRequest Map(
            string token,
            string pcc,
            bool listInd = true,
            QueueAccessRQNavigationAction? navigationAction = null,
            bool returnHostCommand = false)
        {
            var request = new QueueAccessRQRequest
            {
                MessageHeader = new()
                {
                    Action = Constants.Application.QueueAccessAction,
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
                Security = new() { BinarySecurityToken = token },
                QueueAccessRQ = new()
                {
                    QueueIdentifier = [new()
                    {
                        Number = Constants.Application.QueueNumber,
                        PseudoCityCode = pcc,
                    }]
                }
            };

            if (listInd)
            {
                request.QueueAccessRQ.QueueIdentifier[0].List = new() { Ind = true };
            }

            if (navigationAction is not null)
            {
                request.QueueAccessRQ.Navigation = new()
                {
                    Action = navigationAction.Value,
                    ActionSpecified = true
                };
            }

            if (returnHostCommand)
            {
                request.QueueAccessRQ.ReturnHostCommand = true;
                request.QueueAccessRQ.ReturnHostCommandSpecified = true;
            }

            return request;
        }
    }
}
