using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class VSRetroClaimValidator : AbstractValidator<VSRetroClaimRequest>
    {
        public VSRetroClaimValidator()
        {
            RuleFor(x => x.ArrivalAirport).NotNull().NotEmpty().WithMessage("ArrivalAirport must be provided.");
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().WithMessage("MemberNumber must be provided.");
            RuleFor(x => x.OperatingAirlineCode).NotNull().NotEmpty().WithMessage("OperatingAirlineCode must be provided.");
            RuleFor(x => x.OperatingFlightNumber).NotNull().NotEmpty().WithMessage("OperatingFlightNumber must be provided.");
            RuleFor(x => x.DepartureAirport).NotNull().NotEmpty().WithMessage("DepartureAirport must be provided.");
            RuleFor(x => x.FlownCabinClass).NotNull().NotEmpty().WithMessage("FlownCabinClass must be provided.");
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id must be provided.");
            RuleFor(x => x.TicketNumber).NotEmpty().WithMessage("TicketNumber must be provided.");
            RuleFor(x => x.DepartureDate).NotEmpty().WithMessage("DepartureDate must be provided.");
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage("InitiatedBy must be provided.");
            RuleFor(x => x.IataAgentNumber).NotNull().NotEmpty().WithMessage("IataAgentNumber must be provided.");
        }
    }
}
