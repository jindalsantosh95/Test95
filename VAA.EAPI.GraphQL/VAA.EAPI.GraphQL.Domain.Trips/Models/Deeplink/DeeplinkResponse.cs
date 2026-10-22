
namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Deeplink
{
    public class DeeplinkResponse
    {
        public string? DeeplinkUrl { get; set; }
        public Enum.DeeplinkOption DeeplinkOption { get; set; }

        public static explicit operator DeeplinkResponse(Infrastructure.Models.TripMS.DeepLink.DeeplinkResponse source)
        {
            if (source == null) return null;

            return new DeeplinkResponse
            {
                DeeplinkUrl = source.DeeplinkUrl,
                DeeplinkOption = (Enum.DeeplinkOption)source.DeeplinkOption
            };
        }
    }
}