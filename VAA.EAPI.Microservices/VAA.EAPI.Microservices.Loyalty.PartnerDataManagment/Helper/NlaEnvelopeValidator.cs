using System.Globalization;
using System.Text;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper
{
    public static class NlaEnvelopeValidator
    {
        public static bool IsValidEnvelope(NlaEnvelopeBase? envelope, string schemaId, int schemaVersion, out Guid correlationId)
        {
            return IsValidEnvelope(envelope, schemaId, schemaVersion, out correlationId, out _);
        }

        public static bool IsValidEnvelope(
            NlaEnvelopeBase? envelope,
            string schemaId,
            int schemaVersion,
            out Guid correlationId,
            out string validationError,
            bool validateEncryptedPayloadEncoding = true)
        {
            correlationId = Guid.Empty;
            validationError = string.Empty;

            if (envelope is null) return SetValidationError(out validationError, NlaErrorCodes.UnparseableEnvelope);
            if (envelope.SchemaId != schemaId) return SetValidationError(out validationError, NlaErrorCodes.SchemaIdTopicMismatch);
            if (envelope.SchemaVersion == 0) return SetValidationError(out validationError, NlaErrorCodes.MissingEnvelopeSchemaVersion);
            if (envelope.SchemaVersion is < 1 or > 99) return SetValidationError(out validationError, NlaErrorCodes.InvalidEnvelopeSchemaVersion);
            if (envelope.SchemaVersion != schemaVersion) return SetValidationError(out validationError, NlaErrorCodes.SchemaVersionTopicMismatch);
            if (envelope.Producer.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingEnvelopeProducer);
            if (!RegexCompiled.IsMatch(envelope.Producer, Constants.Application.NlaAirlineCodePattern)) return SetValidationError(out validationError, NlaErrorCodes.InvalidEnvelopeProducer);
            if (envelope.Consumer.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingEnvelopeConsumer);
            if (!RegexCompiled.IsMatch(envelope.Consumer, Constants.Application.NlaAirlineCodePattern)) return SetValidationError(out validationError, NlaErrorCodes.InvalidEnvelopeConsumerFormat);
            if (envelope.Timestamp.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingTimestamp);
            if (!TryParseNlaTimestamp(envelope.Timestamp, out var timestamp)) return SetValidationError(out validationError, NlaErrorCodes.InvalidEnvelopeTimestamp);
            if (timestamp > DateTimeOffset.UtcNow) return SetValidationError(out validationError, NlaErrorCodes.FutureTimestamp);
            if (envelope.MessageId.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingMessageId);
            if (!IsCanonicalGuid(envelope.MessageId)) return SetValidationError(out validationError, NlaErrorCodes.InvalidMessageId);
            if (envelope.CorrelationId.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingCorrelationId);
            if (!IsCanonicalGuid(envelope.CorrelationId, out correlationId)) return SetValidationError(out validationError, NlaErrorCodes.InvalidCorrelationId);
            if (envelope.EncryptionKeyId.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingEncryptionKeyId);
            if (envelope.EncryptionKeyId.Length > 50) return SetValidationError(out validationError, NlaErrorCodes.InvalidEncryptionKeyId);
            if (envelope.EncryptedPayload.IsNullOrWhiteSpace()) return SetValidationError(out validationError, NlaErrorCodes.MissingEncryptedPayload);
            if (envelope.EncryptedPayload.Length > Constants.Application.NlaEncryptedPayloadMaxLength) return SetValidationError(out validationError, NlaErrorCodes.EnvelopeTooLarge);
            if (!validateEncryptedPayloadEncoding) return true;
            if (!IsBase64Payload(envelope.EncryptedPayload)) return SetValidationError(out validationError, NlaErrorCodes.InvalidBase64Payload);
            return !IsAsciiArmoredPayload(envelope.EncryptedPayload) || SetValidationError(out validationError, NlaErrorCodes.AsciiArmoredPayload);
        }

        public static bool HasValidMessageId(NlaEnvelopeBase? envelope) => envelope is not null && IsCanonicalGuid(envelope.MessageId);

        private static bool SetValidationError(out string validationError, string error)
        {
            validationError = error;
            return false;
        }

        private static bool IsCanonicalGuid(string value) => IsCanonicalGuid(value, out _);

        private static bool IsCanonicalGuid(string value, out Guid valueAsGuid)
        {
            valueAsGuid = Guid.Empty;
            return Guid.TryParseExact(value, "D", out valueAsGuid) && value == valueAsGuid.ToString();
        }

        private static bool TryParseNlaTimestamp(string timestamp, out DateTimeOffset timestampAsUtc)
        {
            return DateTimeOffset.TryParseExact(timestamp, Constants.Application.EventTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out timestampAsUtc);
        }

        private static bool IsBase64Payload(string encryptedPayload)
        {
            if (encryptedPayload.IsNullOrWhiteSpace() || encryptedPayload.Any(char.IsWhiteSpace)) return false;

            try
            {
                Convert.FromBase64String(encryptedPayload);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool IsAsciiArmoredPayload(string encryptedPayload)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedPayload)).StartsWith("-----BEGIN PGP MESSAGE-----", StringComparison.Ordinal);
        }
    }
}
