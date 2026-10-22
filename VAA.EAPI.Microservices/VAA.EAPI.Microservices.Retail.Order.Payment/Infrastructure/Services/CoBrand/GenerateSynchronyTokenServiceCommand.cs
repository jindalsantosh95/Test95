using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CoBrand
{
    public class GenerateSynchronyTokenServiceCommand : IRequest<Result<Response<SynchronyTokenResponse>>>
    {
        public Customer ApplicantDetails { get; set; }
        public string AccountInfo { get; set; }
        public string InitialPurchaseAmount { get; set; }
    }
}
