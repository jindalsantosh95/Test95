using System.Globalization;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class Reference
    {
        public string? AirlineDesignator { get; set; }
        public string? PnrLocator { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ConfirmationNumber { get; set; }
        public string? ChainCode { get; set; }
        public DateTime? BookingDate { get; set; }
        public string? OrderId { get; set; }

        public static explicit operator Reference(Infrastructure.Models.TripMS.Reference source)
        {
            if (source == null)
                return null;

            return new Reference()
            {
                AirlineDesignator = source.AirlineDesignator,
                PnrLocator = source.PnrLocator,
                CreatedDate = source.CreatedDate,
                ConfirmationNumber = source.ConfirmationNumber,
                ChainCode = source.ChainCode,
                BookingDate = source.BookingDate,
                OrderId = $"{source.PnrLocator}{source.CreatedDate.ToString("ddMMMyy", CultureInfo.InvariantCulture)}".ToUpper()
            };
        }

        public static explicit operator Reference(Infrastructure.Models.HolidaysMS.Reference source)
        {
            if (source == null)
                return null;

            return new()
            {
                PnrLocator = source.PnrLocator,
                AirlineDesignator = source.AirlineDesignator,
                BookingDate = source.BookingDate,
                ConfirmationNumber = source.ConfirmationNumber,
                CreatedDate = source.CreatedDate,
                OrderId = source.OrderId
            };
        }
    }
}
