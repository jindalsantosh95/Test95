using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderAssociation
{
    public class OrderAssociationRequest : IRequest<Result<Response<List<FlightOrders>>>>
    {
        public string CustomerId { get; set; }
        public string PNRNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string MemberStatus { get; set; }
        public OrderAssociationRequest(string customerId, string bookingReference, string firstName, string lastName, string memberStatus)
        {
            CustomerId = customerId;
            PNRNumber = bookingReference;
            FirstName = firstName;
            LastName = lastName;
            MemberStatus = memberStatus;
        }

    }
}
