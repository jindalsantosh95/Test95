using System.ServiceModel.Channels;
using System.Xml.Serialization;
using System.Xml;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Entities
{
    public class Security : MessageHeader
    {
        public Security()
        {
            UsernameToken = new UsernameToken();
        }

        public UsernameToken UsernameToken { get; set; }

        public override string Name => GetType().Name;

        public override string Namespace =>
            "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

        public override bool MustUnderstand => true;

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            var serializer = new XmlSerializer(typeof(UsernameToken));
            serializer.Serialize(writer, UsernameToken);
        }
    }
}
