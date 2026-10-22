using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;

namespace VAA.EAPI.Automation.MobileApp.Tests.ServiceBus;

public class NotificationTests
{
    private static readonly string serviceBusConnectionString = Vault.Default.GetSecretValue("ServiceBusConnectionString");
    private static readonly string topic = "servicenotifications";
    private static readonly string typeAndSubject = "Orches.Notif.CheckinOpenRequest";

    [Fact]
    public async Task SendAndReceiveServiceBusMessage_ShouldSucceed()
    {
        List<Dictionary<string, string>> allPnrs = RetrieveFromStorageTable.RetrieveAllOciPnrs();

        DateTime today = DateTime.UtcNow;
        DateTime mondaySameTime = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        List<Dictionary<string, string>> matchingItems = allPnrs
            .Where(pnr =>
                pnr.ContainsKey("airline") &&
                !string.IsNullOrWhiteSpace(pnr["airline"]) &&

                pnr.ContainsKey("flightNumber") &&
                !string.IsNullOrWhiteSpace(pnr["flightNumber"]) &&

                pnr.ContainsKey("departureDate") &&
                !string.IsNullOrWhiteSpace(pnr["departureDate"]) &&
                DateTime.TryParse(pnr["departureDate"], out DateTime futureDate) &&
                futureDate > today &&

                // Timestamp filter:
                pnr.ContainsKey("timestamp") &&
                !string.IsNullOrWhiteSpace(pnr["timestamp"]) &&
                DateTime.TryParse(pnr["timestamp"], out DateTime timestamp) &&
                timestamp >= mondaySameTime
            )
            .ToList();

        Assert.NotEmpty(matchingItems);

        Random rand = new();

        int randomIndex = rand.Next(matchingItems.Count);

        Dictionary<string, string> randomMatchingItem = matchingItems[randomIndex];

        string pnr = randomMatchingItem["pnr"];

        string messageId = Guid.NewGuid().ToString();

        string carrier = randomMatchingItem["airline"];

        string flightNo = randomMatchingItem["flightNumber"];
        // Extract digits only
        string digitsOnly = new(flightNo.Where(char.IsDigit).ToArray());
        // Pad with leading zeros to desired length (e.g., 5 digits)
        string padded = digitsOnly.Length == 3 ? digitsOnly.PadLeft(5, '0') : digitsOnly;

        string scheduledTimeOfDeparture = randomMatchingItem["departureDate"];
        scheduledTimeOfDeparture = scheduledTimeOfDeparture.Replace("T", " ").Replace(".", ":");
        DateTime dateTime = DateTime.Parse(scheduledTimeOfDeparture);
        // Get only the time portion
        string timeOnly = dateTime.ToString("HH:mm:ss");

        string date = randomMatchingItem["departureDate"];
        date = date.Replace("T", " ").Replace(".", ":");
        dateTime = DateTime.Parse(date);
        // Get only the date portion
        string dateOnly = dateTime.ToString("yyyy-MM-dd");

        string sessionId = Guid.NewGuid().ToString();
        string correlationId = Guid.NewGuid().ToString();

        string id = $"{dateOnly.Replace("-", "")}{timeOnly.Replace(":", "")}{flightNo}";

        string origin = randomMatchingItem["origin"];

        string destination = randomMatchingItem["destination"];

        string messageBody = $@"
                                {{
                                    ""Id"" : ""{messageId}"",
                                    ""Type"" : ""{typeAndSubject}"",
                                    ""DiagnosticsEnabled"": false,
                                    ""SessionId"": ""{sessionId}"",
                                    ""CorrelationId"": ""{correlationId}"",
                                    ""Channel"": [""{correlationId}""],
                                    ""Data"" : 
                                    {{
                                        ""Carrier"" : ""{carrier}"",
                                        ""FlightNo"" : ""{padded}"",
                                        ""ScheduledTimeOfDeparture"" : ""{timeOnly}"",
                                        ""Date"" : ""{dateOnly}"",
                                        ""Id"" : ""{id}"", 
                                        ""Arrival"" : ""{destination}"", 
                                        ""Departure"" : ""{origin}""
                                    }}
                                }}";

        await using ServiceBusClient client = new(serviceBusConnectionString, new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpTcp
        });

        //Leaving for checking the output when debugging locally. 
        Debug.WriteLine($"MessageId - {messageId} | SessionId - {sessionId} | CorrelationId - {correlationId} | Carrier - {carrier} | FlightNumber {padded} | ScheduledTimeDept - {timeOnly} | Date - {dateOnly} | Id - {id} ");

        // Create a sender
        await using ServiceBusSender serviceBusSender = client.CreateSender(topic);

        string messageDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:00");

        // Create a message
        ServiceBusMessage serviceBusMessage = new(messageBody)
        {
            Subject = typeAndSubject,
            CorrelationId = correlationId,
            MessageId = messageId
        };

        // Send the message
        await serviceBusSender.SendMessageAsync(serviceBusMessage);

        RetryHelper.RunWithRetry(() =>
        {
            List<NotificationsTableEntity> allNotifications = RetrieveFromStorageTable.RetrieveNotificationRequests("Crm.Notif.FlightOrderNotifRequest", messageDateTime).ToList();

            bool containsPnr = allNotifications.Any(notification =>
            {
                JObject payload = JObject.Parse(notification.PayLoad);
                string? data = payload["data"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(data))
                {
                    return false;
                }

                string result = StringExtensions.DecompressToString(data);
                JObject notificationPayload = JObject.Parse(result);
                JToken? flightOrderData = notificationPayload["flightOrderData"];

                return flightOrderData?.Any(item => item["bookingReference"]?["pnrLocator"]?.ToString() == pnr) == true;
            });

            Assert.True(containsPnr, $"Rloc was not found in any matching request from the Storage Table. Rloc expected to be there was {pnr}");

        }, 6, 5000);
    }
}
