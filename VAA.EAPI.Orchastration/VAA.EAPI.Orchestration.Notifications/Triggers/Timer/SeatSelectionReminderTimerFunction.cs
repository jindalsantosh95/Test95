using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.Notifications.Triggers.Timer;

public sealed class SeatSelectionReminderTimerFunction(ISeatSelectionReminderService seatSelectionReminderService)
{
    private readonly ISeatSelectionReminderService _seatSelectionReminderService = seatSelectionReminderService ?? throw new ArgumentNullException(nameof(seatSelectionReminderService));

    [Function("SeatSelectionReminderTimerFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task Run([TimerTrigger(Constants.Application.SeatSelectionReminderTimerSchedule)] TimerInfo timerInfo)
    {
        await _seatSelectionReminderService.SynchronizeAsync();
    }
}
