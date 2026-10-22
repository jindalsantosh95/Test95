using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Offer.Interfaces
{

    public interface IRewardSeatCalendarService
    {
        Task<RewardSeatCalendarResponse> SearchAsync(
            RewardSeatCalendarRequest request,
            TimeSpan remainingDeadline,
            CancellationToken cancellationToken);
    }
}
