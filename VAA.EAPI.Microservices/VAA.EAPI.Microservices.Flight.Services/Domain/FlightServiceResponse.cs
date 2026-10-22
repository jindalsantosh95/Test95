using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Flight.Services.Domain
{
    public class FlightServiceResponse
    {
        public Parties Parties { get; set; }
        public Pos Pos { get; set; }
        public string OrderId { get; set; }
        public string OfferExpirationTimeLimit { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }

        public static explicit operator FlightServiceResponse(FlightServiceResponseDocument flightServiceResponseDoc)
        {
            return new FlightServiceResponse()
            {
                Pos = flightServiceResponseDoc.Pos,
                Parties = flightServiceResponseDoc.Parties,
                Offers = flightServiceResponseDoc.Offers,
                OrderId = flightServiceResponseDoc.OrderId
            };
        }
    }
}
