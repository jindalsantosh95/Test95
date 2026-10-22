using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.Xml;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Retail.Analytics.Application.Command;

namespace VAA.EAPI.Microservices.Retail.Analytics.Tests
{
    public sealed class TrackAnalyticsEventCommandTest
    {
        public TrackAnalyticsEventCommandTest()
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
        }

        [Fact]
        public void DataPayloadGeneration()
        {
            var track = new TrackAnalyticsEventCommand
            {
                Type = "TestTracking",
                Subject = "Test"
            };

            track.Track("Test", nameof(DataPayloadGeneration));

            var dataObject = track.Data!;

            Assert.True(dataObject.SelectToken("$.Type")?.ToString() == "TestTracking", "Token 'Type' expected to be of value of 'TestTracking'.");
            Assert.True(dataObject.SelectToken("$.Subject")?.ToString() == "Test", "Subject 'Test' expected to be of value of 'Test'.");
            Assert.True(dataObject.SelectToken("$.Test") is not null, "Token 'Test' expected in json data payload.");
        }

        [Fact]
        public void DataPayloadOverride()
        {
            var track = new TrackAnalyticsEventCommand
            {
                Type = "TestTracking",
                Subject = "Test",
                Data = JsonConvert.DeserializeObject<JObject>($"{{ \"Test\": \"{nameof(DataPayloadOverride)}\" }}")
            };

            var dataObject = track.Data!;

            Assert.True(dataObject.SelectToken("$.Type")?.ToString() == "TestTracking", "Token 'Type' expected to be of value of 'TestTracking'.");
            Assert.True(dataObject.SelectToken("$.Subject")?.ToString() == "Test", "Subject 'Test' expected to be of value of 'Test'.");
            Assert.True(dataObject.SelectToken("$.Test") is not null, "Token 'Test' expected in json data payload.");
        }

        [Fact]
        public void ConversionToTrackingDocument()
        {
            var track = new TrackAnalyticsEventCommand
            {
                Type = "TestTracking",
                Subject = "Test"
            };

            track.Track("Test", nameof(DataPayloadGeneration));

            var document = (TrackAnalyticsDocument)track;

            Assert.True(document.Data.SelectToken("$.Type")?.ToString() == "TestTracking", "Token 'Type' expected to be of value of 'TestTracking'.");
            Assert.True(document.Data.SelectToken("$.Subject")?.ToString() == "Test", "Subject 'Test' expected to be of value of 'Test'.");
            Assert.True(document.Data.SelectToken("$.Test") is not null, "Token 'Test' expected in json data payload.");
        }
    }
}
