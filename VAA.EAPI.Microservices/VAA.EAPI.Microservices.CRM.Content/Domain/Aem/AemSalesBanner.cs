using System;

namespace VAA.EAPI.Microservices.CRM.Content.Domain.Aem;

public class AemSalesBanner
{
    public string Title { get; set; }
    public AemHtml BodyText { get; set; }
    public string Image { get; set; }
    public string ImageAltText { get; set; }

    // General visibility and ranking
    public bool Enabled { get; set; }
    public DateOnly? DisplayFrom { get; set; }
    public DateOnly? DisplayUntil { get; set; }
    public int? Ranking { get; set; }
    public string DisplayLocation { get; set; }
    public string[] BookingTypes { get; set; }
    public string Theme { get; set; }

    // Content-specific filters
    public string[] OriginAirports { get; set; }
    public string[] DestinationAirports { get; set; }
    public DateOnly? EarliestDepartureDate { get; set; }
    public DateOnly? LatestDepartureDate { get; set; }
    public DateOnly? EarliestReturnDate { get; set; }
    public DateOnly? LatestReturnDate { get; set; }
    public string[] FlightDirection { get; set; }
    public int? MinAdults { get; set; }
    public int? MaxAdults { get; set; }
    public int? MinTeens { get; set; }
    public int? MaxTeens { get; set; }
    public int? MinChildren { get; set; }
    public int? MaxChildren { get; set; }
    public int? MinInfants { get; set; }
    public int? MaxInfants { get; set; }
    public string[] DayOfWeek { get; set; }
}