using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.SessionClose
{
    public class SessionCloseRQMapper
    {
        public static SessionCloseRQRequest CloseSessionRQMapper(SessionCloseCommand request)
        {
            return new SessionCloseRQRequest()
            {
                MessageHeader = new MessageHeader()
                {
                    From = new From
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new To
                    {
                        PartyId = [new PartyId() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },
                    CPAId = request.PCC,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    Action = Constants.Application.SessionCloseAction
                },
                
                Security = new Proxy.SessionClose.Security()
                {
                    BinarySecurityToken = request.BinaryToken
                }                ,
                SessionCloseRQ= new()
                {
                    POS = new()
                    {
                        Source = new()
                        {
                            PseudoCityCode = request.PCC
                        }
                    }
                }
            };
        }
    }
}
