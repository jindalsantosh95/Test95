using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger
{
    public class FlownPassengerDetails : TableEntity
    {
        public string FlightAta { get; set; } = string.Empty;
        public string FlightAtd { get; set; } = string.Empty;
        public string FlightBlkTime { get; set; } = string.Empty;
        public string FlightCarrier { get; set; } = string.Empty;
        public string FlightDD { get; set; } = string.Empty;
        public string FlightDate { get; set; } = string.Empty;
        public string FlightDep { get; set; } = string.Empty;
        public string FlightDesc { get; set; } = string.Empty;
        public string FlightEta { get; set; } = string.Empty;
        public string FlightEtd { get; set; } = string.Empty;
        public string FlightLegCD { get; set; } = string.Empty;
        public string FlightMM { get; set; } = string.Empty;
        public int FlightNo { get; set; }
        public int FlightNoOfPax { get; set; }
        public string FlightReg { get; set; } = string.Empty;
        public string FlightSta { get; set; } = string.Empty;
        public string FlightStatus { get; set; } = string.Empty;
        public string FlightStd { get; set; } = string.Empty;
        public string FlightTDOWN { get; set; } = string.Empty;
        public string FlightTKOFF { get; set; } = string.Empty;
        public string FlightType { get; set; } = string.Empty;
        public string FlightYY { get; set; } = string.Empty;
        public string Cancelled { get; set; } = string.Empty;
        public string ETADay { get; set; } = string.Empty;
        public string ETDday { get; set; } = string.Empty;
    }
}
