using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.NDC
{
    public class NDCAgencyValidator : AbstractValidator<NDCAgencyDetails?>
    {
        public NDCAgencyValidator(string operationName)
        {
            When(model => model is not null, () =>
            {
                RuleFor(model => model!.AgencyId).NotEmpty().NotNull();
                if (operationName.Equals("Create"))
                    RuleFor(model => model!.AgencyName).NotEmpty().NotNull();

            });
        }
    }
}
