using System;
using System.ServiceModel.Channels;

namespace VAA.EAPI.Infrastructure.WcfServiceMessaging
{
    public class CustomMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        private readonly MessageEncodingBindingElement _wrapped;

        public CustomMessageEncodingBindingElement(MessageEncodingBindingElement wrapped)
        {
            _wrapped = wrapped;
        }

        public override BindingElement Clone()
        {
            return new CustomMessageEncodingBindingElement((MessageEncodingBindingElement)_wrapped.Clone());
        }

        public override MessageVersion MessageVersion
        {
            get => _wrapped.MessageVersion;
            set
            {
                _wrapped.MessageVersion = value;
            }
        }

        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new CustomMessageEncoderFactory(_wrapped.CreateMessageEncoderFactory());
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return _wrapped.CanBuildChannelFactory<TChannel>(context);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return _wrapped.GetProperty<T>(context);
        }
    }
}
