using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.RetailTherapy.Triggers.Timer;

public sealed class FlightScheduleECommerceTimerFunction(IFlightScheduleECommerceService flightScheduleECommerceService)
{
    private readonly IFlightScheduleECommerceService _flightScheduleECommerceService = flightScheduleECommerceService ?? throw new ArgumentNullException(nameof(flightScheduleECommerceService));

    [Function("FlightScheduleECommerceTimerFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task Run([TimerTrigger(Constants.Application.FlightScheduleECommerceTimerSchedule, UseMonitor = false)] TimerInfo timerInfo)
    {
        await _flightScheduleECommerceService.PublishAsync();
    }
}