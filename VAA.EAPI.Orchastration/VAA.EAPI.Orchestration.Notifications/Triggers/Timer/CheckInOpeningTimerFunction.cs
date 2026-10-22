using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.Notifications.Triggers.Timer;

public sealed class CheckInOpeningTimerFunction(ICheckInOpeningService checkInOpeningService)
{
    private readonly ICheckInOpeningService _checkInOpeningService = checkInOpeningService ?? throw new ArgumentNullException(nameof(checkInOpeningService));

    [Function("CheckInOpeningTimerFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task Run([TimerTrigger(Constants.Application.CheckInOpeningTimerSchedule)] TimerInfo timerInfo)
    {
        await _checkInOpeningService.SynchronizeAsync();
    }
}