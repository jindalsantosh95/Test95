using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.RetailTherapy.Triggers.Timer;

public sealed class FlightCrewRosterTimerFunction(IFlightCrewRosterService flightCrewRoster)
{
    private readonly IFlightCrewRosterService _flightCrewRoster = flightCrewRoster ?? throw new ArgumentNullException(nameof(flightCrewRoster));

    [Function("FlightCrewRosterTimerFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task Run([TimerTrigger(Constants.Application.FlightCrewRosterTimerSchedule, UseMonitor = false)] TimerInfo timerInfo)
    {
        await _flightCrewRoster.PublishAsync();
    }
}