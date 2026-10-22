using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand
{
    public class EngagementScoreCommand : IRequest<Result<Response<EngagementResponse>>>
    {
        public string Email { get; set; }
    }
}
