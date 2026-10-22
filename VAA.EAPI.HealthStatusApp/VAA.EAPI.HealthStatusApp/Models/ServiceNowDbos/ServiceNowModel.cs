using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;

namespace VAA.EAPI.HealthStatusApp.Models.NewFolder
{
    public class ServiceNowModel
    {
        [JsonPropertyName("Number")]
        public string Number { get; set; }

        [JsonPropertyName("Caller")]
        public string Caller { get; set; }

        [JsonPropertyName("Category")]
        public string Category { get; set; }

        [JsonPropertyName("ConfigurationItem")]
        public string ConfigurationItem { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("Priority")]
        public string Priority { get; set; }

        [JsonPropertyName("Channel")]
        public string Channel { get; set; }

        [JsonPropertyName("AssignmentGroup")]
        public string AssignmentGroup { get; set; }

        [JsonPropertyName("Shortdescription")]
        public string Shortdescription { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("Created")]
        public DateTime Created { get; set; }
    }
}
