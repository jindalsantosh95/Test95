using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.TravelRequirements
{
    public sealed class GovernmentForm
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GovernmentFormsStatus Status { get; set; }

        public string? Id { get; set; }
        public string? LinkId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime? AvailableFromUtc { get; set; }
        public string? AvailableFromDescription { get; set; }
        public bool IsAvailable { get; set; }
    }
}
