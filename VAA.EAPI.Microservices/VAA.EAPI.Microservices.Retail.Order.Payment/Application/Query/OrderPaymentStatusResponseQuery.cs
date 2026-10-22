using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query
{
    ///<summary>
    ///OrderPaymentStatusResponse is not in use as per user story -  296010
    ///</summary>
    public class OrderPaymentStatusResponseQuery : IRequest<Result<Response<OrderPaymentResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
