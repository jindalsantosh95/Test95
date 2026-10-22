using System;
using System.IO;
using System.ServiceModel.Channels;

namespace VAA.EAPI.Infrastructure.WcfServiceMessaging
{
    public class CustomMessageEncoderFactory : MessageEncoderFactory
    {
        private readonly MessageEncoderFactory _wrapped;

        public CustomMessageEncoderFactory(MessageEncoderFactory wrapped)
        {
            _wrapped = wrapped;
        }

        public override MessageEncoder Encoder => new CustomMessageEncoder(_wrapped.Encoder);

        public override MessageVersion MessageVersion => _wrapped.MessageVersion;

        public override MessageEncoder CreateSessionEncoder()
        {
            return new CustomMessageEncoder(_wrapped.CreateSessionEncoder());
        }

        public class CustomMessageEncoder : MessageEncoder
        {
            private readonly MessageEncoder _wrapped;

            public CustomMessageEncoder(MessageEncoder wrapped)
            {
                _wrapped = wrapped;
            }

            public override string ContentType => _wrapped.ContentType;

            public override string MediaType => _wrapped.MediaType;

            public override MessageVersion MessageVersion => _wrapped.MessageVersion;

            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                var message = _wrapped.ReadMessage(buffer, bufferManager, contentType);
                return AddSecurityHeader(message);
            }

            public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
            {
                var message = _wrapped.ReadMessage(stream, maxSizeOfHeaders, contentType);
                return AddSecurityHeader(message);
            }

            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                return _wrapped.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
            }

            public override void WriteMessage(Message message, Stream stream)
            {
                _wrapped.WriteMessage(message, stream);
            }

            public override T GetProperty<T>()
            {
                return _wrapped.GetProperty<T>();
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                return _wrapped.IsContentTypeSupported(contentType);
            }

            private Message AddSecurityHeader(Message message)
            {
                var header = MessageHeader.CreateHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", "", true);
                message.Headers.Add(header);
                return message;
            }
        }
    }
}