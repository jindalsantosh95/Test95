using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities
{
    public class WsSecurityEndpointBehavior : IEndpointBehavior
    {
        private readonly string password;
        private readonly string username;

        public WsSecurityEndpointBehavior(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new WsSecurityMessageInspector(username, password));
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }   
}

