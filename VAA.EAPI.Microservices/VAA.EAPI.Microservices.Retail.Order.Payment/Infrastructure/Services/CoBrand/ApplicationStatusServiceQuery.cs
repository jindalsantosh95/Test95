using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand
{
    public class ApplicationStatusServiceQuery : IRequest<Result<Response<ApplicationStatusResponse>>>
    {
        public string SynchronyToken { get; set; }
    }
}
