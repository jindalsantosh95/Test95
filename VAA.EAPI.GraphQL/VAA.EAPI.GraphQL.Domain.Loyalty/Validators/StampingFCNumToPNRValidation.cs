using FluentValidation;
using System.Linq;
using System.Net.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.StampFCNumToPNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class StampingFCNumToPNRValidation : AbstractValidator<StampFCNumToPNRRequest>
    {
        public StampingFCNumToPNRValidation(HttpMethod verb)
        {
            if (verb == HttpMethod.Post)
            {
                RuleFor(x => x.FCStampingDetails.FirstOrDefault().MemberNumber).NotNull().NotEmpty();
                RuleFor(x => x.FCStampingDetails.FirstOrDefault().FirstName).NotNull().NotEmpty();
                RuleFor(x => x.FCStampingDetails.FirstOrDefault().LastName).NotNull().NotEmpty();
                RuleFor(x => x.FCStampingDetails.FirstOrDefault().PNR).NotNull().NotEmpty();

            }
        }
    }
}
