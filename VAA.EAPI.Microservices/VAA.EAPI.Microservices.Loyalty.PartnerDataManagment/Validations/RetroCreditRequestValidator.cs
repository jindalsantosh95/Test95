using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Validations
{
    public  class RetroCreditRequestValidator: AbstractValidator<RetroCreditRequest>
    {
        public RetroCreditRequestValidator()
        {
            RuleFor(x => x.TrackID).NotNull().NotEmpty();
            RuleFor(x => x.FFPCarrierCode).NotNull().NotEmpty();
            RuleFor(x => x.FFPAccountNumber).NotNull().NotEmpty();
            RuleFor(x => x.PassengerFirstName).NotNull().NotEmpty();
            RuleFor(x => x.PassengerLastName).NotNull().NotEmpty();
            RuleFor(m => m.OperatingCarrierCode).NotNull().NotEmpty();
            RuleFor(m => m.OperatingFlightNumber).NotNull().NotEmpty();
            RuleFor(m => m.ScheduledFlightSegmentDepartureDate).NotNull().NotEmpty();
            RuleFor(m => m.FlightSegmentOrigin).NotNull().NotEmpty();
            RuleFor(m => m.FlightSegmentDestination).NotNull().NotEmpty();
            RuleFor(m => m.EticketNumber).NotNull().NotEmpty();
        }
    }
}
