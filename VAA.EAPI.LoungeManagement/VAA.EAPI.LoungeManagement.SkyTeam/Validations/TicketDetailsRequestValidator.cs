using FluentValidation;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Validations

{
    public class TicketDetailsRequestValidator : AbstractValidator<RetrieveTicketDetailsRequest>
    {
        public TicketDetailsRequestValidator()
        {
            RuleFor(x => x.OperatorID).Length(2, 3).WithMessage("OperatorID must be between 2 and 3 characters");
            RuleFor(x => x.BookingReference).Length(6).WithMessage("BookingReference must be exactly 6 characters");
        }

    }
}
