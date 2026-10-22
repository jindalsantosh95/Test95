using FluentValidation;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class SkyPartnerRetroValidator : AbstractValidator<SkyPartnerRetroRequest>
    {
        public SkyPartnerRetroValidator(HttpMethod verb)
        {
            if (verb == HttpMethod.Post)
            {
                var flightActivityNoOfDays = ConfigReader.Try(Common.Constants.Environment.RetroFlightActivityNoOfDays, Constants.Application.FlightActivityNoOfDays);

                RuleFor(x => x.LoyTransaction.CustomerDetails.CustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.LoyTransaction.CustomerDetails.MemberNumber));
                RuleFor(x => x.LoyTransaction.CustomerDetails.MemberNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.LoyTransaction.CustomerDetails.CustomerId));
                RuleFor(x => x.LoyTransaction.CustomerDetails.DocumentFirstName).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.CustomerDetails.DocumentSurname).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.GdsRecordLocator).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.TicketNumber).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.FlightDetails.OperatedAirlineCode).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.FlightDetails.OperatedFlightNumber).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.FlightDetails.OriginAirportCode).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.FlightDetails.DestinationAirportCode).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate).NotNull().NotEmpty();
                RuleFor(x => x.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate).GreaterThan(DateTime.Today.AddDays(flightActivityNoOfDays)).WithMessage(Constants.Application.FlightActivityNoOfDaysErrMessage);
                RuleFor(x => x.TransactionChannel).NotNull().NotEmpty();
                RuleFor(x => x.CreatedBy).NotNull().NotEmpty();
            }
        }
    }
}
