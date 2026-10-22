using MediatR;
using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta.FlightOfferGql;

public class FlightOfferGqlServiceQuery : IRequest<Response<FlightOfferResponse>>
{
    public FlightOfferRequest Request { get; set; }
    public IEnumerable<Price> SliceZeroPrice { get; set; }
    public Guid DocId { get; set; }
}