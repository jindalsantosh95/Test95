using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Services.Analytics
{
    public interface ITrackAnalyticsEventCommandHandler
    {
        Task<bool> Handle(TrackAnalyticsEventCommand request, CancellationToken cancellationToken);
    }
}