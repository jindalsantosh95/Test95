using CSharpFunctionalExtensions;
using MediatR;
using System;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Basket.Application.Command
{
    public class UpdateFlightSummaryCommand : IRequest<Result<Response<Domain.Basket>>>
    {
        public Guid Id { get; set; }
        public string FlightSummary { get; set; }
        public string SummaryLink { get; set; }

    }
}
