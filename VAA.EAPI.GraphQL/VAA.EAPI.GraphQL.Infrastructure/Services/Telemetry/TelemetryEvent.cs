using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common;

namespace VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry
{
    public sealed class TelemetryEvent
    {
        public static explicit operator TelemetryEvent((Enum Type, Dictionary<string, object> Data) source)
            => new(source.Type, source.Data);

        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DateTimeUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public Enum Type { get; set; }
        public string Source { get; set; }

        private readonly Dictionary<string, string> items = new(StringComparer.OrdinalIgnoreCase);

        public JObject Data
        {
            get
            {
                SyncBaseProperties();
                var json = JsonConvert.SerializeObject(items, Formatting.None);
                return JsonConvert.DeserializeObject<JObject>(json);
            }
            set
            {
                if (value is null) return;
                foreach (var property in value)
                    Track(property.Key, property.Value?.ToString());
            }
        }

        public TelemetryEvent(Enum type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Initialise();
        }


        public TelemetryEvent(Enum type, Dictionary<string, object> data)
            : this(type)
        {
            if (data is null) return;
            foreach (var item in data.Where(d => d.Value != null))
                Track(item.Key, item.Value);
        }

        public void Track<T>(string name, T value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            items[name] = value?.ToString() ?? string.Empty;
        }

        private void Initialise()
        {
            if (string.IsNullOrWhiteSpace(Id))
                Id = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(DateTimeUtc))
                DateTimeUtc = DateTime.UtcNow.ToString("o");

            if (string.IsNullOrWhiteSpace(Source))
                Source = "TelemetryEvent";

            SyncBaseProperties();
        }

        private void SyncBaseProperties()
        {
            items["TimestampUtc"] = DateTimeUtc;
            items[nameof(Type)] = Type.ToString();
            items[nameof(Id)] = Id;

            if (!string.IsNullOrWhiteSpace(Source))
                items[nameof(Source)] = Source;
        }

    }
}
