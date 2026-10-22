using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Exceptions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services
{
    public sealed class AutoAccrualFeedbackOutboundKafkaMessageHandler(
        IPgpDecryptionService decryptor,
        ITableStorageService<AutoAccrualFeedbackOutboundMessage> tableStorage,
        ITableStorageService<AutoAccrualFeedbackMessageLookup> messageLookupTableStorage,
        ITableStorageInsertService tableStorageInsertService,
        IPartnerDataService partnerDataService,
        IAutoAccrualFeedbackOutboundEventHandler feedbackEventHandler,
        ILogger<AutoAccrualFeedbackOutboundKafkaMessageHandler>? logger = null)
    {
        public async Task HandleAsync(string messageEnvelope, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            NlaEnvelopeBase? envelope;
            try
            {
                envelope = Json.Deserialize<NlaEnvelopeBase>(messageEnvelope);
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "Outbound auto-accrual feedback envelope is not valid JSON. ErrorCode: {ErrorCode}", NlaErrorCodes.UnparseableEnvelope);
                return;
            }

            if (envelope is null)
            {
                logger?.LogError("Outbound auto-accrual feedback message envelope deserialized to null. ErrorCode: {ErrorCode}", NlaErrorCodes.UnparseableEnvelope);
                return;
            }

            tableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            tableStorage.TableName = Constants.Application.SkyTeamOutboundFeedbackDataTable;
            messageLookupTableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            messageLookupTableStorage.TableName = Constants.Application.SkyTeamOutboundFeedbackMessageLookupTable;
            if (System.Text.Encoding.UTF8.GetByteCount(messageEnvelope) > 8192)
            {
                await StoreEnvelopeValidationFailureAsync(envelope, NlaErrorCodes.EnvelopeTooLarge);
                return;
            }

            if (!NlaEnvelopeValidator.IsValidEnvelope(
                    envelope,
                    Constants.Application.FeedbackDbaSchemaId,
                    Constants.Application.FeedbackDbaSchemaVersion,
                    out _,
                    out var validationError))
            {
                await StoreEnvelopeValidationFailureAsync(envelope, validationError);
                return;
            }

            if (!await IsSkyTeamPartnerAsync(envelope.Producer, cancellationToken))
            {
                await StoreEnvelopeValidationFailureAsync(envelope, NlaErrorCodes.ProducerNotSkyTeamCarrier);
                return;
            }

            if (!await IsSkyTeamPartnerAsync(envelope.Consumer, cancellationToken))
            {
                await StoreEnvelopeValidationFailureAsync(envelope, NlaErrorCodes.ConsumerNotSkyTeamCarrier);
                return;
            }

            if (!envelope.Consumer.Is(Constants.Application.SkyTeamInboundConsumer))
            {
                await StoreEnvelopeValidationFailureAsync(envelope, NlaErrorCodes.InvalidEnvelopeConsumerFormat);
                return;
            }

            var storedMessage = new AutoAccrualFeedbackOutboundMessage
            {
                PartitionKey = envelope.CorrelationId,
                RowKey = envelope.MessageId,
                Producer = envelope.Producer,
                CorrelationId = envelope.CorrelationId,
                EncryptedPayload = envelope.EncryptedPayload,
                Status = Constants.Application.AutoAccrualFeedbackProcessingStatus
            };
            AutoAccrualFeedbackMessageLookup? lookupEntity = null;

            try
            {
                string decryptedPayload;
                try
                {
                    decryptedPayload = decryptor.Decrypt(
                        envelope.EncryptedPayload,
                        ConfigReader.Get(Constants.Environment.VsPgpPrivateKey));
                }
                catch (Exception exception)
                {
                    logger?.LogError(exception, "Unable to decrypt outbound auto-accrual feedback payload.");
                    await RecordErrorAsync(storedMessage, NlaErrorCodes.PgpDecryptionFailed);
                    return;
                }

                DistanceBasedAccrualFeedbackPayload feedback;
                try
                {
                    JToken.Parse(decryptedPayload);
                }
                catch (JsonReaderException exception)
                {
                    logger?.LogError(exception, "Outbound auto-accrual feedback payload is not valid JSON.");
                    await RecordErrorAsync(storedMessage, NlaErrorCodes.InvalidDecryptedPayload);
                    return;
                }

                try
                {
                    feedback = Json.Deserialize<DistanceBasedAccrualFeedbackPayload>(decryptedPayload)
                        ?? throw new InvalidOperationException("Outbound auto-accrual feedback payload deserialized to null.");
                }
                catch (Exception exception)
                {
                    logger?.LogError(exception, "Outbound auto-accrual feedback payload does not conform to the declared schema.");
                    await RecordErrorAsync(storedMessage, NlaErrorCodes.InvalidPayloadStructure);
                    return;
                }

                storedMessage.ParsedFeedbackRequest = Json.Serialize(feedback, ignoreNullValues: false);

                if (!AutoAccrualFeedbackOutboundEventHandler.IsValid(feedback, out var payloadValidationError))
                {
                    await RecordErrorAsync(storedMessage, payloadValidationError);
                    return;
                }

                if (!await IsSkyTeamPartnerAsync(feedback.UniqueIdentification?.AirlineCode, cancellationToken))
                {
                    await RecordErrorAsync(storedMessage, NlaErrorCodes.InvalidAirlineCode);
                    return;
                }

                var envelopePayloadErrorCode = GetEnvelopePayloadErrorCode(envelope, feedback);
                if (envelopePayloadErrorCode is not null)
                {
                    await RecordErrorAsync(storedMessage, envelopePayloadErrorCode);
                    return;
                }

                var lookupCandidate = new AutoAccrualFeedbackMessageLookup
                {
                    PartitionKey = envelope.MessageId,
                    RowKey = envelope.MessageId,
                    CorrelationId = envelope.CorrelationId
                };

                if (!await TryStoreMessageLookupAsync(lookupCandidate, cancellationToken))
                {
                    logger?.LogInformation("Outbound auto-accrual feedback duplicate message detected: {ErrorCode}", NlaErrorCodes.DuplicateMessageId);
                    return;
                }

                lookupEntity = lookupCandidate;
                await tableStorage.InsertOrMergeAsync(storedMessage);

                if (!await feedbackEventHandler.Handle(feedback))
                {
                    await RecordOperationalErrorAsync(storedMessage, "Outbound auto-accrual feedback did not update a flown record.");
                    return;
                }

                storedMessage.Status = Constants.Application.AutoAccrualFeedbackProcessedSuccessfullyStatus;
                storedMessage.ErrorCode = string.Empty;
                await tableStorage.InsertOrMergeAsync(storedMessage);
            }
            catch (AutoAccrualFeedbackCannotBeCorrelatedException exception)
            {
                logger?.LogError(exception, "Outbound auto-accrual feedback could not be correlated. CorrelationId: {CorrelationId}", envelope.CorrelationId);
                await RecordErrorAsync(storedMessage, NlaErrorCodes.InvalidUniqueIdentificationNumber);
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "Outbound auto-accrual feedback processing failed.");
                try
                {
                    await RecordOperationalErrorAsync(storedMessage, "Outbound auto-accrual feedback processing failed.");
                }
                finally
                {
                    if (lookupEntity is not null)
                    {
                        await CleanupLookupAsync(lookupEntity);
                    }
                }
                throw;
            }
        }

        private async Task RecordErrorAsync(AutoAccrualFeedbackOutboundMessage storedMessage, string errorCode)
        {
            storedMessage.Status = Constants.Application.AutoAccrualFeedbackErrorStatus;
            storedMessage.ErrorCode = errorCode;
            logger?.LogError("Outbound auto-accrual feedback processing failed: {ErrorCode}", errorCode);
            await tableStorage.InsertOrMergeAsync(storedMessage);
        }

        private async Task RecordOperationalErrorAsync(AutoAccrualFeedbackOutboundMessage storedMessage, string error)
        {
            storedMessage.Status = Constants.Application.AutoAccrualFeedbackErrorStatus;
            storedMessage.ErrorCode = NlaErrorCodes.FeedbackProcessingFailed;
            logger?.LogError("Outbound auto-accrual feedback processing failed: {Error}", error);
            await tableStorage.InsertOrMergeAsync(storedMessage);
        }

        private async Task StoreEnvelopeValidationFailureAsync(NlaEnvelopeBase envelope, string errorCode)
        {
            logger?.LogError("Outbound auto-accrual feedback envelope validation failed: {ErrorCode}", errorCode);

            if (!NlaEnvelopeValidator.HasValidMessageId(envelope) || !HasValidCorrelationId(envelope.CorrelationId)) return;

            await tableStorage.InsertOrMergeAsync(new AutoAccrualFeedbackOutboundMessage
            {
                PartitionKey = envelope.CorrelationId,
                RowKey = envelope.MessageId,
                Producer = envelope.Producer,
                CorrelationId = envelope.CorrelationId,
                EncryptedPayload = envelope.EncryptedPayload,
                Status = Constants.Application.AutoAccrualFeedbackErrorStatus,
                ErrorCode = errorCode
            });
        }

        private Task<bool> TryStoreMessageLookupAsync(AutoAccrualFeedbackMessageLookup lookupEntity, CancellationToken cancellationToken) =>
            tableStorageInsertService.TryInsertAsync(messageLookupTableStorage.TableName, lookupEntity, cancellationToken);

        private async Task CleanupLookupAsync(AutoAccrualFeedbackMessageLookup lookupEntity)
        {
            try
            {
                await messageLookupTableStorage.DeleteAsync(lookupEntity);
            }
            catch (Exception exception)
            {
                logger?.LogError(exception, "Unable to remove outbound auto-accrual feedback message lookup after processing failure.");
            }
        }

        private static bool HasValidCorrelationId(string correlationId) =>
            Guid.TryParseExact(correlationId, Constants.Application.NlaCanonicalGuidFormat, out _);

        private static string? GetEnvelopePayloadErrorCode(
            NlaEnvelopeBase envelope,
            DistanceBasedAccrualFeedbackPayload feedback)
        {
            if (!envelope.Producer.Is(feedback.Producer)) return NlaErrorCodes.EnvelopePayloadProducerMismatch;
            if (!envelope.Consumer.Is(feedback.Consumer)) return NlaErrorCodes.EnvelopePayloadConsumerMismatch;
            if (!string.Equals(envelope.SchemaId, feedback.SchemaId, StringComparison.Ordinal)) return NlaErrorCodes.EnvelopePayloadSchemaIdMismatch;
            return envelope.SchemaVersion == feedback.SchemaVersion ? null : NlaErrorCodes.EnvelopePayloadSchemaVersionMismatch;
        }

        private async Task<bool> IsSkyTeamPartnerAsync(string? partnerCode, CancellationToken cancellationToken)
        {
            var skyTeamPartners = await partnerDataService.GetSkyTeamPartnersAsync(cancellationToken);
            return skyTeamPartners.Any(partner => partner.PartnerCode.Is(partnerCode));
        }

    }
}
