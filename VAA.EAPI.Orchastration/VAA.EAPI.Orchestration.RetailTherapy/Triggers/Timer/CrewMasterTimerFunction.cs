using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.RetailTherapy.Triggers.Timer;

public sealed class CrewMasterTimerFunction(ICrewMasterService crewMasterService)
{
    private readonly ICrewMasterService _crewMasterService = crewMasterService ?? throw new ArgumentNullException(nameof(crewMasterService));

    [Function("CrewMasterTimerFunction")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task Run([TimerTrigger(Constants.Application.CrewMasterTimerSchedule, UseMonitor = false)] TimerInfo timerInfo)
    {
        await _crewMasterService.PublishAsync();
    }
}