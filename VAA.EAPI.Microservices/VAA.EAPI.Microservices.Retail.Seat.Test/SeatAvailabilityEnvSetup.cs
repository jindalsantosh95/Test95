using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Seat.Tests
{
    public class SeatAvailabilityEnvSetup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"DebugOptions", "Y"},
                {"TestLab", "TVTH"},
                {"PrinterData", "14450A"},
                {"MicroServiceDBKey", "DefaultEndpointsProtocol=https;AccountName=avadeunstofferms;AccountKey=jeoJ3mYA9Vo+rRfC2X4QdWmVnCQPCNF0YtLxYSXhBszVj8+fNwWJRIcmoYQSZUYsYqZMnW4wgcaH+ASt0o8bfA==;EndpointSuffix=core.windows.net"},
                {"KeyVaultClientID","AirportLocation" },
                {"KeyVaultClientSecret", "QuS8Q~f5rsqK-NNKDV5EdW6HdTLgfrcKc3wH~dkO"},
                {"KeyVaultUri", "https://avadeunkveapis.vault.azure.net/"},
                {"DeltaCertificateName", "NdcDLSoaCertificate"},
                {"Air4SeatV8EndPoint", "https://soaws.vs01.vs.air4.com:12000/seat/v8"},
                {"MicroServiceEventStoreTableName", "SeatFulfilmentEventStore"},
                {"MicroServiceResponseTableName", "SeatFulfilmentResponse"},
                {"OpenApi__Scope", "https://test/"},
                {"OpenApi__TokenUrl", "https://test.microsoft.com/"},
                {"SeatFulfilmentResponseUrl", "http://localhost:7045/SeatFulfilment/correlationid"},
                {"SeatDB", "SeatDB_Sit"},
                {"SeatDbCollection", "SeatOfferCollection"},
                {"OfferExpiryLimitInMinutes", "60"},
                {"SeatDbEndpoint", "https://avadeuncmdbseat.documents.azure.com:443/"},
                {"ServiceBusTopicName", "seatfulfilmenteventmessage"},
                {"ServiceBusConnectionString", "Endpoint=sb://avadeunsbseatms.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=DlhlAAnQONgMeRkU1UJeXd/D3VEnnzjT1+ASbPuGDro="},
                { "CosmosSqlDBConnectionMode", "Gateway"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
