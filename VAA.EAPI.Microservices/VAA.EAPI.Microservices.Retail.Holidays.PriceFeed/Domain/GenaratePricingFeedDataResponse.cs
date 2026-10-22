using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Domain
{
    public class GenaratePricingFeedDataResponse
    {
        public string? DestinationId { get; set; }
        public string? Address { get; set; }
        public string? Url { get; set; }
        public string? Price { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? LengthOfStay { get; set; }
        public string? NumTravelers { get; set; }
        public string? TravelStart { get; set; }
        public string? TravelEnd { get; set; }
        public string? DepartureAirport { get; set; }
        public string? Name { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Description { get; set; }


        public static explicit operator GenaratePricingFeedDataResponse(Holiday h)
        {
            var hotel = h.Hotel;
            var content = hotel.Content;
            var special = h.SpecialOffers;
            var loc = content.Location;

            return new GenaratePricingFeedDataResponse
            {
                DestinationId = hotel.Id ?? "",
                Address = content.ParentLocation ?? "",
                Url = content.Url ?? "",
                Price = h.PricePerPerson.ToString() ?? "",
                Status = "active",
                Type = special.Type ?? "",
                LengthOfStay = special.Duration.ToString(),
                NumTravelers = (special.Adults + special.Children + special.Infants).ToString(),
                TravelStart = special.MinCheckInDate ?? "",
                TravelEnd = special.MaxCheckInDate ?? "",
                DepartureAirport = special.DepartureAirportCode ?? "",
                Name = content.Name ?? "",
                Latitude = loc.Lat.ToString(),
                Longitude = loc.Lon.ToString(),
                Description = content.HotelDescription ?? content.VirginView ?? ""
            };
        }
    }

 }
