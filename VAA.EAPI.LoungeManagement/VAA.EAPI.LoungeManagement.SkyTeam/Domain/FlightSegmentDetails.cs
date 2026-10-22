using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using VAA.EAPI.LoungeManagement.SkyTeam.Helper;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain
{
    public class FlightSegmentDetails
    {
        [JsonProperty("OperatingcarrierCode")]
        public string OperatingcarrierCode { get; set; }

        [JsonProperty("FlightNumber")]
        public string FlightNumber { get; set; }

        [JsonProperty("BookingFareClass")]
        public string BookingFareClass { get; set; }

        [JsonProperty("TravellingFareClass")]
        public string TravellingFareClass { get; set; }

        [JsonProperty("ScheduledFlightDepartureDate")]
        public string ScheduledFlightDepartureDate { get; set; }

        [JsonProperty("ScheduledFlightDepartureTime")]
        public string ScheduledFlightDepartureTime { get; set; }

        [JsonProperty("ScheduledFlightArrivalDate")]
        public string ScheduledFlightArrivalDate { get; set; }

        [JsonProperty("ScheduledFlightArrivalTime")]
        public string ScheduledFlightArrivalTime { get; set; }

        [JsonProperty("FlightOrigin")]
        public string FlightOrigin { get; set; }

        [JsonProperty("FlightDestination")]
        public string FlightDestination { get; set; }

        [JsonProperty("VoluntaryUpgradeIndicator", DefaultValueHandling = DefaultValueHandling.Populate)]
        public int VoluntaryUpgradeIndicator { get; set; }

        public static explicit operator FlightSegmentDetails(TicketData.TicketCoupon coupon)
        {
            var maxSqNo = coupon.EtCpnCrrRole.Max(e => Convert.ToInt16(e.CpnStSqNb));
            var etCpnCrrRole = coupon.EtCpnCrrRole.FirstOrDefault(e => Convert.ToInt16(e.CpnStSqNb) == maxSqNo);

            return new FlightSegmentDetails
            {
                OperatingcarrierCode = etCpnCrrRole.OpAsCrrCd,
                FlightNumber = etCpnCrrRole?.EqpOwnrFltNb,
                BookingFareClass = etCpnCrrRole.EqOwrTktgCOSCd,
                TravellingFareClass = string.IsNullOrEmpty(etCpnCrrRole.UpgradedClassOfService) ? etCpnCrrRole.EqOwrTktgCOSCd : etCpnCrrRole.UpgradedClassOfService,
                ScheduledFlightDepartureDate = string.IsNullOrEmpty(etCpnCrrRole.SchdDprtLDt) ? null : DateTime.ParseExact(etCpnCrrRole.SchdDprtLDt, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                ScheduledFlightDepartureTime = DataFormatHelper.ConvertToTimeFormat(etCpnCrrRole.SchdDprtLTmTxt),
                ScheduledFlightArrivalDate = string.IsNullOrEmpty(etCpnCrrRole.SchdArrLDt) ? null : DateTime.ParseExact(etCpnCrrRole.SchdArrLDt, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                ScheduledFlightArrivalTime = DataFormatHelper.ConvertToTimeFormat(etCpnCrrRole.SchdArrLTmTxt),
                FlightOrigin = etCpnCrrRole.ROrigTrpnLocTxt,
                FlightDestination = etCpnCrrRole.RDestTrpnLocTxt,
                VoluntaryUpgradeIndicator = etCpnCrrRole.VolantaryUpgrade
            };
        }

    }
}

