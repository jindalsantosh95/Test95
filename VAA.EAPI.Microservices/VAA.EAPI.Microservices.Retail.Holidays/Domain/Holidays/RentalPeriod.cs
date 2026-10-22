using NodaTime;
using System;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class Rentalperiod
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string TbxId { get; set; }

    }
}
