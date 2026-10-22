using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestTimerInfo : TimerInfo
    {
        public TestTimerInfo() : base(new TestSchedule(), new ScheduleStatus(), false) { }

        private class TestSchedule : TimerSchedule
        {
            public override bool AdjustForDST => false;

            public override DateTime GetNextOccurrence(DateTime now) => now.AddMinutes(1);
        }
    }
}
