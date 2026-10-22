using System.ServiceModel;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities;


namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Helper
{
    public class SkyClient
    {
        public static Func<IServiceProvider, OnlineRetroCreditPtClient> GetSkyRetroCreditClient()
        {
            var skyClient = new OnlineRetroCreditPtClient(
            HttpBinding.CreateBasicHttpBinding(), new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SkyTeamOnlineRetroCreditEndPoint)))
            );

            if (!skyClient.Endpoint.EndpointBehaviors.Contains(typeof(WsSecurityEndpointBehavior)))
            {
                skyClient.Endpoint.EndpointBehaviors.Add(new WsSecurityEndpointBehavior(ConfigReader.Get(Constants.Environment.SkyEndpointUserName), ConfigReader.Get(Constants.Environment.SkyEndpointPwd)));
            }

            skyClient.Endpoint.Binding.SendTimeout = TimeSpan.Parse(ConfigReader.Try(Constants.Environment.TimeOutTimeSpan, "00:01:40"));

            return s => skyClient;
        }
    }
}
