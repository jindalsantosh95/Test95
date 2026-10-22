using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Serialisation;
using DomainAirshipRequest = VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification.AirshipRequest;
using DomainCampaigns = VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification.Campaigns;
using AirshipNotifRequest = VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Models.Airship.NotifRequest;

namespace VAA.EAPI.Microservices.CRM.Notification.Tests
{
    public class NotifRequestTests
    {
        [Fact]
        public void ExplicitOperator_WithCustomEventAndCampaigns_AddsAirshipPayload()
        {
            // Arrange
            var request = CreateAirshipRequest();
            request.CustomEventName = "emit_starlink";
            request.Campaigns = new DomainCampaigns
            {
                Categories = ["Starlink", "WiFi"]
            };

            // Act
            var json = Json.Serialize((AirshipNotifRequest)request);
            var payload = JObject.Parse(json);

            // Assert
            Assert.Equal("emit_starlink", payload["notification"]?["actions"]?["add_custom_event"]?["name"]?.Value<string>());
            Assert.Equal(new[] { "Starlink", "WiFi" }, payload["campaigns"]?["categories"]?.Values<string>());
        }

        [Fact]
        public void ExplicitOperator_WithoutCustomEventAndCampaigns_OmitsOptionalAirshipPayload()
        {
            // Arrange
            var request = CreateAirshipRequest();

            // Act
            var json = Json.Serialize((AirshipNotifRequest)request);
            var payload = JObject.Parse(json);

            // Assert
            Assert.Null(payload["notification"]?["actions"]?["add_custom_event"]);
            Assert.Null(payload["campaigns"]);
        }

        private static DomainAirshipRequest CreateAirshipRequest()
        {
            return new DomainAirshipRequest
            {
                PnrsWithDate = ["ESSDPN06FEB26"],
                Group = "Trips",
                Message = "You're now ready to check in for your flight to Orlando.",
                DeepLink = "https://virginatlantic.com/ywnux"
            };
        }
    }
}
