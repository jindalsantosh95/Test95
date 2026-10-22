using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.InflightFetchOrders
{
    public class OrderSearchCommand : IRequest<Result<Response<List<FlightOrders>>>>
    {
        public string CustomerId { get; set; }
        public string MemberStatus { get; set; }
        public string FutureDays { get; set; }
        public string PastDays { get; set; }
        public OrderSearchCommand(string customerId, string membrStatus, string futureDays, string pastDays)
        {
            CustomerId = customerId;
            MemberStatus = membrStatus;
            FutureDays = futureDays;
            PastDays = pastDays;
        }
    }
}
