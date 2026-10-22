using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Infrastructure.Services.Analytics
{
    public class TrackAnalyticsMessage : CloudEvent
    {
        public static explicit operator TrackAnalyticsMessage(JObject source) => new()
        {
            Id = source["id"].ToString(),
            Date = source["Date"].ToString(),
            Type = source["Data"]["Type"].ToString(),
            Subject = source["Data"]["Subject"].ToString(),
            Data = (JObject)source["Data"]
        };
        public static explicit operator TrackAnalyticsMessage(string source) => Json.Deserialize<TrackAnalyticsMessage>(source);

        public static void Initialise(TrackAnalyticsMessage source)
        {
            if (source.Id.IsNullOrWhiteSpaceOr(Guid.Empty.ToString()))
                source.Id = Guid.NewGuid().ToString();

            if (source.Source.IsNullOrWhiteSpace())
                source.Source = Constants.Constants.AnalyticsServiceBusTopicName;

            source.Specversion = "1.0";

            if (source.Date.IsNullOrWhiteSpace())
                source.Date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            if (source.Time.IsNullOrWhiteSpace())
                source.Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt");

            source.Track("SessionId", AsyncContext.Current?.SessionId ?? Guid.Empty);
            source.Track("CorrelationId", AsyncContext.Current?.CorrelationId ?? Guid.Empty);
            source.Track("Channel", string.Join(' ', AsyncContext.Current?.Channel ?? Array.Empty<string>()));
            source.Track("DiagnosticsEnabled", AsyncContext.Current?.DiagnosticsEnabled ?? false);
        }

        private readonly Dictionary<string, string> items = new();

        public TrackAnalyticsMessage()
        {
            Initialise(this);

            Track("DocType", nameof(TrackAnalyticsEventCommand));
            Track("Type", Type);
            Track("Subject", Subject);
        }

        public string Date { get; set; }
        public new JObject Data
        {
            get
            {
                SyncBaseProperties();

                return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(items, Formatting.None));
            }
            set
            {
                if (value is null) return;

                foreach (var property in value)
                    Track(property.Key, property.Value?.ToString());
            }
        }

        public void Track<T>(string name, [AllowNull] T value)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            if (items.ContainsKey(name))
                items[name] = value?.ToString() ?? string.Empty;
            else
                items.Add(name, value?.ToString() ?? string.Empty);
        }

        public void Track(IEnumerable<KeyValuePair<string, object>> trackedItems)
        {
            if (trackedItems.IsNullOrEmpty()) throw new ArgumentNullException(nameof(trackedItems));

            foreach (var item in trackedItems)
                Track(item.Key, item.Value);
        }

        public T GetValue<T>(string name) => GetValue(name).Parse<T>();
        public string GetValue(string name)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));
            if (!items.ContainsKey(name)) throw new ArgumentOutOfRangeException(nameof(name));

            return items[name];
        }

        private void SyncBaseProperties()
        {
            Track(nameof(Date), Date);
            Track(nameof(Time), Time);
            Track(nameof(Type), Type);
            Track(nameof(Subject), Subject);
        }
    }
}
