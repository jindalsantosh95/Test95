using System.Collections.Generic;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Search.Models.Search.Request;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class Booking
    {
        public static explicit operator Booking(BookingSessionRequest source) => new()
        {
            Continuation = source.BasketId,
            Links = new Link[]
            {
                new Link()
                {
                    Href =ConfigReader.Get(Constants.Environment.BookingCheckOutURL),
                    Rel = "checkout"
                }
            },
            Price = new()
        };

        public string Continuation { get; set; }
        public Price Price { get; set; }
        public IEnumerable<Link> Links { get; set; }
    }
}
