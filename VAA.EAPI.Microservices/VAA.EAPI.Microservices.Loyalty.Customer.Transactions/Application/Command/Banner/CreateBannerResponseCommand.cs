using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Banner
{
    public class CreateBannerResponseCommand : IRequest<Result<Response<string>>>
    {
        public CreateBannerResponseCommand(string correlationId)
        {
            CorrelationId = correlationId;
        }

        public string CorrelationId { get; set; }

    }
}
