using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class ProductSummaryDetails
    {
        public string AvailabilityStatus { get; set; }
        public string ProductStatus { get; set; }
        public string ProductSubStatus { get; set; }
        public string ProductType { get; set; }
        public string Name { get; set; }
        public string Highlight { get; set; }
        public string PrimaryAttribute { get; set; }
        public string SecondaryAttribute { get; set; }
        public string ServiceStartDate { get; set; }
        public string ServiceEndDate { get; set; }
        public string DurationDescription { get; set; }
        public TravelDatesInfo TravelDates { get; set; }
        public TravelDurationInfo TravelDuration { get; set; }
        public bool Recommended { get; set; }
        public bool SpecialOffer { get; set; }
        public string ItemSource { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? Discount { get; set; }
        public decimal? PriceBeforeDiscount { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string QuoteExpiryDate { get; set; }
        public string ExperienceType { get; set; }
        public TravelTimes TravelTimings { get; set; }
        public decimal? TotalAmendCharges { get; set; }
        public decimal? TotalCancellationCharges { get; set; }
        public bool? VipBooking { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public DateTime? BookedDate { get; set; }
        public string? BookingType { get; set; }
        public string? BookingTypeName { get; set; }
        public string? BrandName { get; set; }
        public string? LeadPassengerName { get; set; }
        public string? Division { get; set; }
        public string ElementGroup { get; set; }
        public string ElementGroupName { get; set; }

        public static explicit operator ProductSummaryDetails(JToken source)
        {
            if (source == null)
                return null;

            return new()
            {
                Name = source["name"]?.Value<string>(),
                Highlight = source["highlight"]?.Value<string>(),
                ProductStatus = source["productStatus"]?.Value<string>(),
                ProductSubStatus = source["productSubStatus"]?.Value<string>(),
                AvailabilityStatus = source["availabilityStatus"]?.Value<string>(),
                Discount = source["discount"]?.Value<decimal>(),
                StartTime = source["startTime"]?.Value<DateTime>(),
                EndTime = source["endTime"]?.Value<DateTime>(),
                TravelTimings = (TravelTimes)source["travelTimes"],
                TravelDates = (TravelDatesInfo)source["travelDates"],
                TravelDuration = (TravelDurationInfo)source["travelDuration"],
                VipBooking = source["vipBooking"]?.Value<bool>(),
                PrimaryAttribute = source["primaryAttribute"]?.Value<string>(),
                SecondaryAttribute = source["secondaryAttribute"]?.Value<string>(),
                DepartureDate = source["departureDate"]?.Value<DateTime>(),
                ReturnDate = source["returnDate"]?.Value<DateTime>(),
                BookedDate = source["bookedDate"]?.Value<DateTime>(),
                BookingType = source["bookingType"]?.Value<string>(),
                BookingTypeName = source["bookingTypeName"]?.Value<string>(),
                BrandName = source["brandName"]?.Value<string>(),
                LeadPassengerName = source["leadPassengerName"]?.Value<string>(),
                Division = source["division"]?.Value<string>()
            };
        }

        public static explicit operator ProductSummaryDetails((JToken summary, JToken detail) source)
        {
            var (summary, details) = source;

            if (summary == null) return null;

            return new()
            {
                Name = summary["name"]?.Value<string>(),
                Highlight = summary["highlight"]?.Value<string>(),
                ProductStatus = summary["productStatus"]?.Value<string>(),
                ProductSubStatus = summary["productSubStatus"]?.Value<string>(),
                AvailabilityStatus = summary["availabilityStatus"]?.Value<string>(),
                Discount = summary["discount"]?.Value<decimal>(),
                StartTime = summary["startTime"]?.Value<DateTime>(),
                EndTime = summary["endTime"]?.Value<DateTime>(),
                TravelTimings = (TravelTimes)summary["travelTimes"],
                TravelDates = (TravelDatesInfo)summary["travelDates"],
                TravelDuration = (TravelDurationInfo)summary["travelDuration"],
                VipBooking = summary["vipBooking"]?.Value<bool>(),
                PrimaryAttribute = summary["primaryAttribute"]?.Value<string>(),
                SecondaryAttribute = summary["secondaryAttribute"]?.Value<string>(),
                DepartureDate = summary["departureDate"]?.Value<DateTime>(),
                ReturnDate = summary["returnDate"]?.Value<DateTime>(),
                BookedDate = summary["bookedDate"]?.Value<DateTime>(),
                BookingType = summary["bookingType"]?.Value<string>(),
                BookingTypeName = summary["bookingTypeName"]?.Value<string>(),
                BrandName = summary["brandName"]?.Value<string>(),
                LeadPassengerName = summary["leadPassengerName"]?.Value<string>(),
                Division = summary["division"]?.Value<string>(),
                ElementGroup = details?["elementGrp"]?.Value<string>(),
                ElementGroupName = details?["elementGrpName"]?.Value<string>()
            };
        }

        public static explicit operator ProductSummaryDetails((JToken, (string Name, string Highlight)) data)
        {
            var (summary, details) = data;
            if (summary == null) return null;
            var (name, highlight) = details;
            return new()
            {
                Name = name,
                Highlight = highlight,
                PrimaryAttribute = default,
                SecondaryAttribute = default,
                ProductStatus = summary["productStatus"]?.Value<string>(),
                ProductSubStatus = summary["productSubStatus"]?.Value<string>(),
                AvailabilityStatus = summary["availabilityStatus"]?.Value<string>(),
                Discount = summary["discount"]?.Value<decimal>(),
                StartTime = summary["startTime"]?.Value<DateTime>(),
                EndTime = summary["endTime"]?.Value<DateTime>(),
                TravelTimings = (TravelTimes)summary["travelTimes"],
                TravelDates = (TravelDatesInfo)summary["travelDates"],
                TravelDuration = (TravelDurationInfo)summary["travelDuration"],
                VipBooking = summary["vipBooking"]?.Value<bool>(),
                DepartureDate = summary["departureDate"]?.Value<DateTime>(),
                ReturnDate = summary["returnDate"]?.Value<DateTime>(),
                BookedDate = summary["bookedDate"]?.Value<DateTime>(),
                BookingType = summary["bookingType"]?.Value<string>(),
                BookingTypeName = summary["bookingTypeName"]?.Value<string>(),
                BrandName = summary["brandName"]?.Value<string>(),
                LeadPassengerName = summary["leadPassengerName"]?.Value<string>(),
                Division = summary["division"]?.Value<string>(),
                ElementGroup = Constants.Application.DisneyElementGroupCode,
                ElementGroupName = default
            };
        }
    }
}
