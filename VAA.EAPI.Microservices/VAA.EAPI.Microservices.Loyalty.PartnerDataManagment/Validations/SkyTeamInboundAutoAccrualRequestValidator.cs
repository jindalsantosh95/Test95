using FluentValidation;
using System.Globalization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Validations
{
    public class SkyTeamInboundAutoAccrualRequestValidator : AbstractValidator<DistanceBasedAccrualPayload>
    {
        public SkyTeamInboundAutoAccrualRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.SchemaId).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingPayloadSchemaId).MaximumLength(16).WithErrorCode(NlaErrorCodes.InvalidPayloadStructure).Equal(Constants.Application.AccrualDbaSchemaId).WithErrorCode(NlaErrorCodes.SchemaIdTopicMismatch);
            RuleFor(x => x.SchemaVersion).Equal(Constants.Application.AccrualDbaSchemaVersion).WithErrorCode(NlaErrorCodes.InvalidPayloadSchemaVersion);
            RuleFor(x => x.Producer).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingPayloadProducer).Length(2, 3).WithErrorCode(NlaErrorCodes.InvalidPayloadProducer);
            RuleFor(x => x.Consumer).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingPayloadConsumer).Length(2, 3).WithErrorCode(NlaErrorCodes.InvalidPayloadConsumer);
            RuleFor(x => x.RequestType).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingRequestType).Length(4, 4).WithErrorCode(NlaErrorCodes.InvalidRequestType).Equal(Constants.Application.AccrualDbaRequestType).WithErrorCode(NlaErrorCodes.InvalidRequestType);
            RuleFor(x => x.AccrualType).Equal(Constants.Application.AccrualDbaAccrualType).WithErrorCode(NlaErrorCodes.AccrualTypeTopicMismatch);
            RuleFor(x => x.UniqueIdentification).NotNull().WithErrorCode(NlaErrorCodes.MissingUniqueIdentification);
            RuleFor(x => x.PassengerName).NotNull().WithErrorCode(NlaErrorCodes.MissingPassengerName);
            RuleFor(x => x.FfpDetails).NotNull().WithErrorCode(NlaErrorCodes.MissingFfpDetails);
            RuleFor(x => x.FlightDetails).NotNull().WithErrorCode(NlaErrorCodes.MissingFlightDetails);
            RuleFor(x => x.FlightSegment).NotNull().WithErrorCode(NlaErrorCodes.MissingFlightSegment);
            RuleFor(x => x.BookingDetails).NotNull().WithErrorCode(NlaErrorCodes.MissingBookingDetails);
            RuleFor(x => x.DocumentDetails).NotNull().WithErrorCode(NlaErrorCodes.MissingDocumentDetails);
            RuleFor(x => x.ScheduledDepartureDate).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingScheduledDepartureDate).Must(IsScheduledDepartureDate).WithErrorCode(NlaErrorCodes.InvalidScheduledDepartureDate);
            RuleFor(x => x.ProductCode).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingProductCode).MaximumLength(3).WithErrorCode(NlaErrorCodes.InvalidProductCode);

            When(x => x.UniqueIdentification is not null, () => RuleFor(x => x.UniqueIdentification!.UniqueIdentificationNumber).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingUniqueIdentificationNumber).MaximumLength(21).WithErrorCode(NlaErrorCodes.InvalidUniqueIdentificationNumber));
            When(x => x.PassengerName is not null, () =>
            {
                RuleFor(x => x.PassengerName!.SurName).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingSurname).MaximumLength(64).WithErrorCode(NlaErrorCodes.InvalidSurname);
                RuleFor(x => x.PassengerName!.GivenName).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingGivenName).MaximumLength(64).WithErrorCode(NlaErrorCodes.InvalidGivenName);
            });
            When(x => x.FfpDetails is not null, () =>
            {
                RuleFor(x => x.FfpDetails!.FfpCarrierCode).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingFfpCarrierCode).Length(2, 3).WithErrorCode(NlaErrorCodes.InvalidFfpCarrierCode);
                RuleFor(x => x.FfpDetails!.FfpAccountNumber).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingFfpAccountNumber).MaximumLength(20).WithErrorCode(NlaErrorCodes.InvalidFfpAccountNumber);
            });
            When(x => x.FlightDetails is not null, () =>
            {
                RuleFor(x => x.FlightDetails!.MarketingFlight).NotNull().WithErrorCode(NlaErrorCodes.MissingMarketingFlight);
                RuleFor(x => x.FlightDetails!.OperatingFlight).NotNull().WithErrorCode(NlaErrorCodes.MissingOperatingFlight);
            });
            When(x => x.FlightDetails?.MarketingFlight is not null, () =>
            {
                RuleFor(x => x.FlightDetails!.MarketingFlight!.MarketingCarrierCode).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingMarketingCarrierCode).MaximumLength(3).WithErrorCode(NlaErrorCodes.InvalidMarketingCarrierCode);
                RuleFor(x => x.FlightDetails!.MarketingFlight!.MarketingFlightNumber).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingMarketingFlightNumber).MaximumLength(5).WithErrorCode(NlaErrorCodes.InvalidMarketingFlightNumber);
            });
            When(x => x.FlightDetails?.OperatingFlight is not null, () =>
            {
                RuleFor(x => x.FlightDetails!.OperatingFlight!.OperatingCarrierCode).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingOperatingCarrierCode).MaximumLength(3).WithErrorCode(NlaErrorCodes.InvalidOperatingCarrierCode);
                RuleFor(x => x.FlightDetails!.OperatingFlight!.OperatingFlightNumber).MaximumLength(5).WithErrorCode(NlaErrorCodes.InvalidOperatingFlightNumber);
            });
            When(x => x.FlightSegment is not null, () =>
            {
                RuleFor(x => x.FlightSegment!.Origin).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingOrigin).MaximumLength(3).WithErrorCode(NlaErrorCodes.InvalidOrigin);
                RuleFor(x => x.FlightSegment!.Destination).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingDestination).MaximumLength(3).WithErrorCode(NlaErrorCodes.InvalidDestination);
            });
            When(x => x.BookingDetails is not null, () =>
            {
                RuleFor(x => x.BookingDetails!.CabinClassCode).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingCabinClassCode).Length(1, 1).WithErrorCode(NlaErrorCodes.InvalidCabinClassCode);
                RuleFor(x => x.BookingDetails!.OperatingCarrierBookingClass).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingOperatingCarrierBookingClass).MaximumLength(2).WithErrorCode(NlaErrorCodes.InvalidOperatingCarrierBookingClass);
                RuleFor(x => x.BookingDetails!.MarketingCarrierBookingClass).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingMarketingCarrierBookingClass).MaximumLength(2).WithErrorCode(NlaErrorCodes.InvalidMarketingCarrierBookingClass);
                RuleFor(x => x.BookingDetails!.OperatingCarrierFareType).MaximumLength(8).WithErrorCode(NlaErrorCodes.InvalidOperatingCarrierFareType);
                RuleFor(x => x.BookingDetails!.MarketingCarrierFareType).MaximumLength(8).WithErrorCode(NlaErrorCodes.InvalidMarketingCarrierFareType);
            });
            When(x => x.DocumentDetails is not null, () =>
            {
                RuleFor(x => x.DocumentDetails!.DocumentNumber).Cascade(CascadeMode.Stop).NotEmpty().WithErrorCode(NlaErrorCodes.MissingDocumentNumber).MaximumLength(13).WithErrorCode(NlaErrorCodes.InvalidDocumentNumber).Matches("^[0-9]+$").WithErrorCode(NlaErrorCodes.InvalidDocumentNumber);
                RuleFor(x => x.DocumentDetails!.DocumentCouponNumber).LessThanOrEqualTo(9).WithErrorCode(NlaErrorCodes.InvalidDocumentCouponNumber);
                RuleFor(x => x.DocumentDetails!.DocumentIssueDateTime).MaximumLength(20).WithErrorCode(NlaErrorCodes.InvalidDocumentIssueDateTime).Must(IsUtcDateTime).WithErrorCode(NlaErrorCodes.InvalidDocumentIssueDateTime).When(x => !x.DocumentDetails!.DocumentIssueDateTime.IsNullOrWhiteSpace());
            });
            RuleFor(x => x.FlightTypeIndicator).MaximumLength(1).WithErrorCode(NlaErrorCodes.InvalidFlightTypeIndicator);
            RuleFor(x => x.AdditionalPassengerInformation).MaximumLength(31).WithErrorCode(NlaErrorCodes.InvalidAdditionalPassengerInformation);
            RuleFor(x => x.AdditionalInformation).MaximumLength(25).WithErrorCode(NlaErrorCodes.InvalidAdditionalInformation);
        }

        private static bool IsScheduledDepartureDate(string date) => DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

        private static bool IsUtcDateTime(string? dateTime) => dateTime is not null && DateTimeOffset.TryParseExact(dateTime, Constants.Application.EventTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _);
    }
}
