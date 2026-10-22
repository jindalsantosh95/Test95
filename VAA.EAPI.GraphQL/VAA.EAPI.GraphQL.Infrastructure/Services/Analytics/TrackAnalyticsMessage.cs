using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.GraphQL.Infrastructure.Services.Analytics
{
    public sealed class TrackAnalyticsMessage
    {
        public static explicit operator TrackAnalyticsMessage((string Type, string Subject, Dictionary<string, object> Data) source) => new(source.Type, source.Subject, source.Data);

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");

        public string Time { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt");

        public string Type { get; set; }

        public string Subject { get; set; }

        public string Source { get; set; }

        public JObject Data { get; set; } = [];

        public TrackAnalyticsMessage(string type, string subject)
        {
            Type = type;
            Subject = subject;
        }
        public TrackAnalyticsMessage(string type, string subject, Dictionary<string, object> data) : this(type, subject)
        {
            foreach (var item in data.Where(d => d.Value != null))
                Data[item.Key] = JToken.FromObject(item.Value);
        }
    }
}
