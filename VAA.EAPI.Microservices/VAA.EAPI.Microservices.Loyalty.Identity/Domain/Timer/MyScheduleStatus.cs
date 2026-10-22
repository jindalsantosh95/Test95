using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Timer
{
    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }
        public DateTime Next { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
