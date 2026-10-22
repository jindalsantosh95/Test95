using System.ServiceModel;
using System.ServiceModel.Channels;

namespace VAA.EAPI.Infrastructure.WcfServiceMessaging
{
    public class WcfCustomBinding
    {
        public static CustomBinding Binding(bool httpsEndpoint)
        {
            CustomBinding binding = new CustomBinding();
            var securityBindingElement = SecurityBindingElement.CreateCertificateOverTransportBindingElement();

            binding.Elements.Add(securityBindingElement);
            binding.Elements.Add(new TextMessageEncodingBindingElement
            {
                MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.None)
            });

            if (httpsEndpoint)
            {
                binding.Elements.Add(new HttpsTransportBindingElement { RequireClientCertificate = true, AllowCookies = false, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, TransferMode = TransferMode.Buffered, });
            }
            else
            {
                binding.Elements.Add(new HttpTransportBindingElement { AllowCookies = false, MaxBufferSize = int.MaxValue, MaxReceivedMessageSize = int.MaxValue, TransferMode = TransferMode.Buffered, });
            }

            var elements = binding.Elements;
            var bindingElementCounter = -1;

            for (bindingElementCounter = 0; bindingElementCounter < elements.Count; bindingElementCounter++)
            {
                if (typeof(MessageEncodingBindingElement).IsAssignableFrom(elements[bindingElementCounter].GetType()))
                {
                    break;
                }
            }

            var messageEncodingBindingElement = (MessageEncodingBindingElement)elements[bindingElementCounter];
            elements[bindingElementCounter] = new CustomMessageEncodingBindingElement(messageEncodingBindingElement);

            return new CustomBinding(binding);
        }

    }
}
