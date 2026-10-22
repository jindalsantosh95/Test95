using MediatR;
using System.IO;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Models.Basket
{
    public sealed class BasketRequest : IRequest<BasketResponse>
    {
        public static explicit operator BasketRequest(Stream source)
        {
            if (source == null) return new BasketRequest();
            if (!source.CanRead) return new BasketRequest();
            if (source.Length == 0) return new BasketRequest();
            if (source.Position > 0) return new BasketRequest();

            return Json.Deserialize<BasketRequest>(new StreamReader(source).ReadToEndAsync().GetAwaiter().GetResult());
        }

        public JourneyType JourneyType { get; set; } = JourneyType.OneWay;
        public LocationType LocationOne { get; set; } = LocationType.LHR;
        public LocationType LocationTwo { get; set; } = LocationType.JFK;
        public LocationType LocationThree { get; set; }
        public LocationType LocationFour { get; set; }
        public LocationType LocationFive { get; set; }
        public LocationType LocationSix { get; set; }
        public int NumberOfAdults { get; set; } = 1;
        public int NumberOfChildren { get; set; }
        public int NumberOfYouths { get; set; }
        public int NumberOfInfants { get; set; }
    }
}
