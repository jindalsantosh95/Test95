using System;

namespace VAA.EAPI.Microservices.CRM.Content.Domain.SalesBanner;

public class SalesBannerRequest
{
    public string OriginAirport { get; set; }
    public string DestinationAirport { get; set; }
    public string DestinationURL { get; set; }
    public DateOnly? DepartureDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public string FlightDirection { get; set; }
    public int? NumAdults { get; set; }
    public int? NumTeens { get; set; }
    public int? NumChildren { get; set; }
    public int? NumInfants { get; set; }
    public string BookingType { get; set; }
}