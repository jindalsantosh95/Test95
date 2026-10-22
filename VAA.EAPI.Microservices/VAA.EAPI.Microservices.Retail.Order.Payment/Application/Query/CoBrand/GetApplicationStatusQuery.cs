using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.CoBrand
{
    public class GetApplicationStatusQuery : IRequest<Result<Response<ApplicationStatusResponse>>>
    {
        public string SynchronyToken { get; set; }
    }
}
