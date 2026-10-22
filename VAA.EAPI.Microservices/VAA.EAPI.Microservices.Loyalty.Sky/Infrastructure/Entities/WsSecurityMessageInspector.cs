using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities
{
    public class WsSecurityMessageInspector : IClientMessageInspector
    {
        private readonly string password;
        private readonly string username;

        public WsSecurityMessageInspector(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var header = new Security
            {
                UsernameToken =
                    {
                        Password = new Password
                        {
                            Value = password,
                            Type = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"
                        },
                        Username = username
                    }
            };
            request.Headers.Add(header);
            return null;
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
        }
    }
}
