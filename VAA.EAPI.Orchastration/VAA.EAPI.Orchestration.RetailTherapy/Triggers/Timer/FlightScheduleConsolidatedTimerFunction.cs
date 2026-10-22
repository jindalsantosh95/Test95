using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.RetailTherapy.Triggers.Timer;

public sealed class FlightScheduleConsolidatedTimerFunction(IFlightScheduleConsolidatedService flightScheduleConsolidatedService)
{
    private readonly IFlightScheduleConsolidatedService _flightScheduleConsolidatedService = flightScheduleConsolidatedService ?? throw new ArgumentNullException(nameof(flightScheduleConsolidatedService));

    [Function("FlightScheduleConsolidatedTimerFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task Run([TimerTrigger(Constants.Application.FlightScheduleConsolidatedTimerSchedule, UseMonitor = false)] TimerInfo timerInfo)
    {
        await _flightScheduleConsolidatedService.PublishAsync();
    }
}