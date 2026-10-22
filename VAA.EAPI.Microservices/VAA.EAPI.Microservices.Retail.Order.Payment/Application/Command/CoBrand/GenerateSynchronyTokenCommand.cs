using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand
{
    public class GenerateSynchronyTokenCommand : IRequest<Result<Response<SynchronyTokenResponse>>>
    {
        public SynchronyTokenRequest TokenRequest { get; set; }
    }
}
