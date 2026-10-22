using MediatR;
using VAA.EAPI.Infrastructure.Services.Analytics;

namespace VAA.EAPI.Microservices.Retail.Analytics.Application.Command
{
    public sealed class TrackAnalyticsMessageCommand : TrackAnalyticsDocument, IRequest<bool>
    {
        public static explicit operator TrackAnalyticsMessageCommand(string source)
        {
            return (TrackAnalyticsMessageCommand)(TrackAnalyticsMessage)source;
        }

        public static explicit operator TrackAnalyticsMessageCommand(TrackAnalyticsMessage source)
        {
            var trackAnalyticsDocument = (TrackAnalyticsDocument)source;

            return new TrackAnalyticsMessageCommand()
            {
                Id = trackAnalyticsDocument.Id,
                Date = trackAnalyticsDocument.Date,
                DocType = trackAnalyticsDocument.DocType,
                PartitionKey = trackAnalyticsDocument.PartitionKey,
                Data = trackAnalyticsDocument.Data,
            };
        }
    }
}
