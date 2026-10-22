using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Application.OrderRetrieve
{
    public class OrderRetrieveQuery : IRequest<Result<Response<IATA_OrderViewRS>>>
    {
        public IATA_OrderRetrieveRQ OrderRetrieveRQ { get; set; }

        public string SubscriptionKey { get; set; }
    }
}
