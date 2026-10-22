using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.FlightCustomers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query.FlightBookingCustomer
{
    public class FlightBookingCustomerQuery : IRequest<Result<Response<FlightCustomer>>>
    {
        public string Pnr { get; set; }
        public string OrderId { get; set; }

        public FlightBookingCustomerQuery(string pnr, string orderId)
        {
            Pnr = pnr;
            OrderId = orderId;
        }
    }
}
