using FluentValidation;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class EditVSRetroFlownDataValidator : AbstractValidator<EditVSRetroFlownDataRequest>
    {
        public EditVSRetroFlownDataValidator()
        {
            RuleFor(x => x.OriginAirport)
                .Must(value => !value.IsNullOrWhiteSpace())
                .WithMessage("OriginAirport must be provided.");
            RuleFor(x => x.DestinationAirport)
                .Must(value => !value.IsNullOrWhiteSpace())
                .WithMessage("DestinationAirport must be provided.");
            RuleFor(x => x.Status)
                .Must(value => !value.IsNullOrWhiteSpace())
                .WithMessage("Status must be provided.");
            RuleFor(x => x.TicketNumber)
                .Must(value => !value.IsNullOrWhiteSpace())
                .WithMessage("TicketNumber must be provided.");
        }
    }
}
