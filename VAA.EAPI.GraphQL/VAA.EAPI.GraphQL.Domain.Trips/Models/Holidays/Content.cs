using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayContent")]
    public class Content
    {
        public string Type { get; set; }
        public string TripAdvisorCode { get; set; }
        public string AccommodationSafety { get; set; }
        public string FireSafety { get; set; }
        public string BeachSafety { get; set; }
        public string TopTips { get; set; }
        public string ResortFees { get; set; }
        public string ResortFeesDetail { get; set; }
        public string VillaLocation { get; set; }
        public string VillaChoice { get; set; }
        public string EatsDrinks { get; set; }
        public string Facilities { get; set; }
        public string FunGames { get; set; }
        public string Entertainment { get; set; }
        public string HandyStuff { get; set; }
        public string AdditionalInfo { get; set; }
        public string GettingAround { get; set; }
        public string DisabilityInfoHeading { get; set; }
        public string DisabilityInfoDescription { get; set; }
        public string EcmsId { get; set; }
        public string TbxId { get; set; }
        public string AccommodationType { get; set; }
        public IEnumerable<Image> Images { get; set; }
        public GeoCoordinates LocationCoordinates { get; set; }
        public string VRating { get; set; }
      
    }



}
