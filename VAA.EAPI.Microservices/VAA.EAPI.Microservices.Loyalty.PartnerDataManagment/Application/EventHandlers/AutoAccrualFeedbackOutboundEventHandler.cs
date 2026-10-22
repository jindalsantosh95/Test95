using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;


namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    using System.Globalization;
    using VAA.EAPI.Common.Extensions;
    using VAA.EAPI.Common.Helpers;
    using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Exceptions;
    using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
    using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

    public class AutoAccrualFeedbackOutboundEventHandler : IAutoAccrualFeedbackOutboundEventHandler
    {
        private readonly ILogger<AutoAccrualFeedbackOutboundEventHandler> _logger;
        private readonly IFlownDbRepository _flownDbRepository;

        public AutoAccrualFeedbackOutboundEventHandler(
            ILogger<AutoAccrualFeedbackOutboundEventHandler> logger,
            IFlownDbRepository flownDbRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _flownDbRepository = flownDbRepository ?? throw new ArgumentNullException(nameof(flownDbRepository));
        }

        public async Task<bool> Handle(DistanceBasedAccrualFeedbackPayload message)
        {
            try
            {
                if (message == null)
                {
                    _logger.LogError("SkyTeam auto accrual feedback message is null.");
                    return false;
                }

                var updated = await _flownDbRepository.UpdateFlownRecordWithAutoAccrualFeedbackAsync(message);

                if (!updated)
                {
                    var uniqueIdentificationNumber = message.UniqueIdentification?.UniqueIdentificationNumber ?? string.Empty;
                    _logger.LogError("No matching eligible flown record found for SkyTeam auto accrual feedback. UIN: {Uin}", uniqueIdentificationNumber);
                    throw new AutoAccrualFeedbackCannotBeCorrelatedException(uniqueIdentificationNumber);
                }

                _logger.LogInformation("Successfully processed SkyTeam auto accrual feedback. UIN: {Uin}, PostingStatus: {PostingStatus}", message.UniqueIdentification?.UniqueIdentificationNumber, message.PostingDetails?.PostingStatus);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing SkyTeam Auto Accrual feedback.");
                throw;
            }
        }

        internal static bool IsValid(DistanceBasedAccrualFeedbackPayload message, out string validationErrorCode)
        {
            validationErrorCode = string.Empty;
            var uniqueIdentification = message.UniqueIdentification;
            var postingDetails = message.PostingDetails;
            var mileDetails = message.MileDetails;

            if (message.SchemaId.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingPayloadSchemaId;
            else if (message.SchemaVersion == 0) validationErrorCode = NlaErrorCodes.MissingPayloadSchemaVersion;
            else if (message.SchemaVersion is < 1 or > 99) validationErrorCode = NlaErrorCodes.InvalidPayloadSchemaVersion;
            else if (message.RequestType.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingRequestType;
            else if (message.RequestType != Constants.Application.AutoAccrualRequestType) validationErrorCode = NlaErrorCodes.InvalidRequestType;
            else if (message.AccrualType.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingAccrualType;
            else if (message.AccrualType.Length != Constants.Application.DistanceBasedAccrualType.Length) validationErrorCode = NlaErrorCodes.InvalidAccrualType;
            else if (message.AccrualType != Constants.Application.DistanceBasedAccrualType) validationErrorCode = NlaErrorCodes.AccrualTypeTopicMismatch;
            else if (message.Producer.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingPayloadProducer;
            else if (!HasValidAirlineCode(message.Producer)) validationErrorCode = NlaErrorCodes.InvalidPayloadProducer;
            else if (message.Consumer.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingPayloadConsumer;
            else if (!HasValidAirlineCode(message.Consumer)) validationErrorCode = NlaErrorCodes.InvalidPayloadConsumer;
            else if (uniqueIdentification is null) validationErrorCode = NlaErrorCodes.MissingUniqueIdentification;
            else if (uniqueIdentification.UniqueIdentificationNumber.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingUniqueIdentificationNumber;
            else if (uniqueIdentification.UniqueIdentificationNumber.Length > Constants.Application.UniqueIdentificationNumberMaxLength) validationErrorCode = NlaErrorCodes.InvalidUniqueIdentificationNumber;
            else if (!HasValidAirlineCode(uniqueIdentification.AirlineCode)) validationErrorCode = NlaErrorCodes.InvalidAirlineCode;
            else if (postingDetails is null) validationErrorCode = NlaErrorCodes.MissingPostingDetails;
            else if (postingDetails.PostingDateTime.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingPostingDateTime;
            else if (!DateTimeOffset.TryParseExact(postingDetails.PostingDateTime, Constants.Application.EventTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _)) validationErrorCode = NlaErrorCodes.InvalidPostingDateTime;
            else if (postingDetails.PostingStatus.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingPostingStatus;
            else if (!postingDetails.PostingStatus.Is(Constants.Application.SuccessfulPostingStatus) && !postingDetails.PostingStatus.Is(Constants.Application.UnsuccessfulPostingStatus)) validationErrorCode = NlaErrorCodes.InvalidPostingStatus;
            else if (mileDetails is null) validationErrorCode = NlaErrorCodes.MissingMileDetails;
            else if (mileDetails.BaseMiles is null) validationErrorCode = NlaErrorCodes.MissingBaseMiles;
            else if (!HasValidMileage(mileDetails, out validationErrorCode)) return false;
            else if (message.ProductCode.IsNullOrWhiteSpace()) validationErrorCode = NlaErrorCodes.MissingProductCode;
            else if (!RegexCompiled.IsMatch(message.ProductCode, Constants.Application.NlaProductCodePattern)) validationErrorCode = NlaErrorCodes.InvalidProductCode;
            else if (message.ProductCode != Constants.Application.AccrualDbaProductCode) validationErrorCode = NlaErrorCodes.BilateralAgreementProductCodeNotCovered;
            else if (message.AdditionalInformation?.Length > Constants.Application.NlaAdditionalInformationMaxLength) validationErrorCode = NlaErrorCodes.InvalidAdditionalInformation;
            else if (postingDetails.PostingStatus.Is(Constants.Application.UnsuccessfulPostingStatus) && !HasValidErrors(message.ErrorInformation, out validationErrorCode)) return false;
            return validationErrorCode.Length == 0;
        }

        private static bool HasValidAirlineCode(string? airlineCode) =>
            !airlineCode.IsNullOrWhiteSpace() &&
            RegexCompiled.IsMatch(airlineCode, Constants.Application.NlaAirlineCodePattern);

        private static bool HasValidMileage(MileDetails mileDetails, out string validationErrorCode)
        {
            validationErrorCode = string.Empty;
            if (!IsValidMileageValue(mileDetails.BaseMiles)) validationErrorCode = NlaErrorCodes.InvalidBaseMiles;
            else if (!IsValidMileageValue(mileDetails.ClassOfServiceBonusMiles)) validationErrorCode = NlaErrorCodes.InvalidClassOfServiceBonusMiles;
            else if (!IsValidMileageValue(mileDetails.EliteTierBonusMiles)) validationErrorCode = NlaErrorCodes.InvalidEliteTierBonusMiles;
            else if (!IsValidMileageValue(mileDetails.PromotionalBonusMiles)) validationErrorCode = NlaErrorCodes.InvalidPromotionalBonusMiles;
            return validationErrorCode.Length == 0;
        }

        private static bool IsValidMileageValue(int? value) => value is null || value is >= 0 and <= 99999;

        private static bool HasValidErrors(BusinessErrorInformation? errorInformation, out string validationErrorCode)
        {
            validationErrorCode = string.Empty;
            if (errorInformation is null)
            {
                validationErrorCode = NlaErrorCodes.MissingErrorInformation;
                return false;
            }

            if (errorInformation.ErrorDetails is null || errorInformation.ErrorDetails.Count == 0)
            {
                validationErrorCode = NlaErrorCodes.MissingErrorDetails;
                return false;
            }

            foreach (var error in errorInformation.ErrorDetails)
            {
                if (error.ErrorCode.IsNullOrWhiteSpace())
                {
                    validationErrorCode = NlaErrorCodes.MissingBusinessErrorCode;
                    return false;
                }

                if (!RegexCompiled.IsMatch(error.ErrorCode, Constants.Application.NlaErrorCodePattern))
                {
                    validationErrorCode = NlaErrorCodes.InvalidBusinessErrorCode;
                    return false;
                }

                if (error.ErrorDescription?.Length > Constants.Application.NlaErrorDescriptionMaxLength)
                {
                    validationErrorCode = NlaErrorCodes.InvalidBusinessErrorDescription;
                    return false;
                }
            }

            return validationErrorCode.Length == 0;
        }
    }
}
