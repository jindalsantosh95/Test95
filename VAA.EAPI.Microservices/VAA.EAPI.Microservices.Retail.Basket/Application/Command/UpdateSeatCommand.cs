using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Seat;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateSeatCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
    }
}