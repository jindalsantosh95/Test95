using System;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Aem;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain.SalesBanner
{
    public class SalesBanner
    {
        // Content
        public string Title { get; set; }
        public string BodyPlainText { get; set; }
        public string BodyHtml { get; set; }
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

        public static explicit operator SalesBanner(AemSalesBanner source) => new SalesBanner
        {
            Title = source.Title,
            BodyPlainText = source.BodyText.PlainText,
            BodyHtml = source.BodyText.Html,
            Image = source.Image.IsNullOrEmpty() ? null : ConfigReader.Get(Constants.Environment.AemImagePrefix) + source.Image,
            ImageAltText = source.ImageAltText,

            Enabled = source.Enabled,
            DisplayFrom = source.DisplayFrom,
            DisplayUntil = source.DisplayUntil,
            Ranking = source.Ranking,
            DisplayLocation = source.DisplayLocation,
            BookingTypes = source.BookingTypes,
            Theme = source.Theme,

            OriginAirports = source.OriginAirports,
            DestinationAirports = source.DestinationAirports,
            EarliestDepartureDate = source.EarliestDepartureDate,
            LatestDepartureDate = source.LatestDepartureDate,
            EarliestReturnDate = source.EarliestReturnDate,
            LatestReturnDate = source.LatestReturnDate,
            FlightDirection = source.FlightDirection?.Select(fd => fd.Replace("flight-direction:", "")).ToArray(),
            MinAdults = source.MinAdults,
            MaxAdults = source.MaxAdults,
            MinTeens = source.MinTeens,
            MaxTeens = source.MaxTeens,
            MinChildren = source.MinChildren,
            MaxChildren = source.MaxChildren,
            MinInfants = source.MinInfants,
            MaxInfants = source.MaxInfants,
            DayOfWeek = source.DayOfWeek?.Select(fd => fd.Replace("day-of-week:", "")).ToArray()
        };
    }
}
