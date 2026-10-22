using MediatR;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Infrastructure.Services.Analytics
{
    public sealed class TrackAnalyticsEventCommand : TrackAnalyticsMessage, IRequest<bool>
    {
        public static explicit operator TrackAnalyticsEventCommand(string source) => Json.Deserialize<TrackAnalyticsEventCommand>(source);

        public string ServiceBusConnectionString => ConfigReader.Try(Constants.Constants.AnalyticsServiceBusConnectionString);
    }
}
