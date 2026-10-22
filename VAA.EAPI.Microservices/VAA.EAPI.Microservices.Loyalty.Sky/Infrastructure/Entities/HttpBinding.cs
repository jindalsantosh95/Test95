using System.ServiceModel;
using System.Xml;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities
{
    public class HttpBinding
    {
        public static BasicHttpBinding CreateBasicHttpBinding()
        {
            return new BasicHttpBinding
            {
                MaxBufferSize = int.MaxValue,
                ReaderQuotas = XmlDictionaryReaderQuotas.Max,
                MaxReceivedMessageSize = int.MaxValue,
                AllowCookies = false,
                Name = "BasicHttpBinding",
                Security = new BasicHttpSecurity
                {
                    Mode = BasicHttpSecurityMode.Transport
                },
            };
        }
    }
}
