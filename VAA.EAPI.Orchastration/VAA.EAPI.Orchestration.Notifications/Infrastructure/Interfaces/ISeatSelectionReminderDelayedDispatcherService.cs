using System;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces
{
    public interface ISeatSelectionReminderDelayedDispatcherService
    {
        int Count { get; }
        bool Schedule(FlightDetail item, TimeSpan delay);
    }
}
