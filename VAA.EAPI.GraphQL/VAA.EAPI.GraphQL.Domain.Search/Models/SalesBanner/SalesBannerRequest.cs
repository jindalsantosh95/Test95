namespace VAA.EAPI.GraphQL.Domain.Search.Models.SalesBanner;

public class SalesBannerRequest
{
    [GraphQLDescription("IATA code for the origin airport of this sector.")]
    public string? OriginAirport { get; set; }
    [GraphQLDescription("IATA code for the destination airport of this sector.")]
    public string? DestinationAirport { get; set; }
    [GraphQLDescription("Departure date of the outbound flight, even if shown against an inbound flight.")]
    public string? DestinationURL { get; set; }
    [GraphQLDescription("Destination URL of the sector if it's called from Holidays")]
    public DateOnly? DepartureDate { get; set; }
    [GraphQLDescription("For round trips, the departure date of the inbound flight, even if shown against an outbound flight. For one-way searches, the departure date of that flight.")]
    public DateOnly? ReturnDate { get; set; }
    [GraphQLDescription("'outbound', 'inbound' or 'one-way'.")]
    public string? FlightDirection { get; set; }
    [GraphQLDescription("Indicates which search experience tab this applies to. Options include 'flight-only', 'flight-hotel', 'flight-car', 'holidays' and 'reward-flights'.")]
    public string? BookingType { get; set; }
    [GraphQLDescription("Integer 0 - 9.")]
    public int? NumAdults { get; set; }
    [GraphQLDescription("Integer 0 - 9.")]
    public int? NumTeens { get; set; }
    [GraphQLDescription("Integer 0 - 9.")]
    public int? NumChildren { get; set; }
    [GraphQLDescription("Integer 0 - 9.")]
    public int? NumInfants { get; set; }
}