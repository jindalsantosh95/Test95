using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Application.Models
{
    [Flags]
    public enum EligibilityType
    {
        None = 0,
        DutyFree = 1,
        POPS = 2,
        Both = 4,
    }
}
