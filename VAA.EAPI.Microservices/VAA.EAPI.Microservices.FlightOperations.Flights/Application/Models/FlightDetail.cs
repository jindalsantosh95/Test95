using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.AimsSoap;
using VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models
{
    public sealed class FlightDetail
    {
        public static explicit operator FlightDetail(FlightListTAIMSFlight source) => new()
        {
            AircraftType = source.FlightAcType,
            ActualDateOfArrival = DateOnly.ParseExact(source.FlightActualDayArr, Constants.Application.DateOnlyIncomingFormats),
            ActualDateOfDeparture = DateOnly.ParseExact(source.FlightActualDayDep, Constants.Application.DateOnlyIncomingFormats),
            Arrival = source.FlightArr,
            ArrivalDelays = source.FlightArrDelays,
            AssociatedCrewRoutes = [.. (source.FlightAssocCrwRtes as AssosiatedCrewRouteInfo[] ?? []).Select(x => (RouteDetail)x)],
            ActualTimeOfArrival = TimeOnly.TryParse(source.FlightAta, out var flightAta) ? flightAta : default,
            ActualTimeOfDeparture = TimeOnly.TryParse(source.FlightAtd, out var flightAtd) ? flightAtd : default,
            FlightBlkTime = TimeOnly.TryParse(source.FlightBlkTime, out var flightBlkTime) ? flightBlkTime : default,
            Carrier = source.FlightCarrier,
            DayOfDeparture = source.FlightDD.ToString(),
            Date = DateOnly.ParseExact(source.FlightDate, Constants.Application.DateOnlyIncomingFormats),
            Departure = source.FlightDep,
            DepartureDelays = source.FlightDepDelays,
            Description = source.FlightDesc,
            EstimatedTimeOfArrival = TimeOnly.TryParse(source.FlightEta, out var flightEta) ? flightEta : default,
            EstimatedTimeOfDeparture = TimeOnly.TryParse(source.FlightEtd, out var flightEtd) ? flightEtd : default,
            FlightLegCD = source.FlightLegCD,
            MonthOfDeparture = source.FlightMM.ToString(),
            FlightNo = source.FlightNo,
            Capacity = source.FlightNoOfPax,
            Registration = source.FlightReg,
            Status = source.FlightStatus,
            ScheduledTimeOfArrival = TimeOnly.TryParse(source.FlightSta, out var flightSta) ? flightSta : default,
            ScheduledTimeOfDeparture = TimeOnly.TryParse(source.FlightStd, out var flightStd) ? flightStd : default,
            TimeOfTouchDown = TimeOnly.TryParse(source.FlightTDOWN, out var flightTDOWN) ? flightTDOWN : default,
            TimeOfTakeOff = TimeOnly.TryParse(source.FlightTKOFF, out var flightTKOFF) ? flightTKOFF : default,
            Type = source.FlightType,
            YearOfDeparture = source.FlightYY.ToString()
        };
        public static explicit operator FlightDetail(FlightListX1TAIMSFlightX1 source) => new()
        {
            AircraftType = source.FlightAcType,
            ActualDateOfArrival = DateOnly.ParseExact(source.FlightActualDayArr, Constants.Application.DateOnlyIncomingFormats),
            ActualDateOfDeparture = DateOnly.ParseExact(source.FlightActualDayDep, Constants.Application.DateOnlyIncomingFormats),
            Arrival = source.FlightArr,
            ArrivalDelays = source.FlightArrDelays,
            AssociatedCrewRoutes = [.. source.FlightAssocCrwRtes.Select(x => (RouteDetail)x)],
            ActualTimeOfArrival = TimeOnly.TryParse(source.FlightAta, out var flightAta) ? flightAta : default,
            ActualTimeOfDeparture = TimeOnly.TryParse(source.FlightAtd, out var flightAtd) ? flightAtd : default,
            FlightBlkTime = TimeOnly.TryParse(source.FlightBlkTime, out var flightBlkTime) ? flightBlkTime : default,
            Carrier = source.FlightCarrier,
            DayOfDeparture = source.FlightDD,
            Date = DateOnly.ParseExact(source.FlightDate, Constants.Application.DateOnlyIncomingFormats),
            Departure = source.FlightDep,
            DepartureDelays = source.FlightDepDelays,
            Description = source.FlightDesc,
            EstimatedTimeOfArrival = TimeOnly.TryParse(source.FlightEta, out var flightEta) ? flightEta : default,
            EstimatedTimeOfDeparture = TimeOnly.TryParse(source.FlightEtd, out var flightEtd) ? flightEtd : default,
            FlightLegCD = source.FlightLegCD,
            MonthOfDeparture = source.FlightMM,
            FlightNo = source.FlightNo,
            Capacity = source.FlightNoOfPax,
            Registration = source.FlightReg,
            Status = source.FlightStatus,
            ScheduledTimeOfArrival = TimeOnly.TryParse(source.FlightSta, out var flightSta) ? flightSta : default,
            ScheduledTimeOfDeparture = TimeOnly.TryParse(source.FlightStd, out var flightStd) ? flightStd : default,
            TimeOfTouchDown = TimeOnly.TryParse(source.FlightTDOWN, out var flightTDOWN) ? flightTDOWN : default,
            TimeOfTakeOff = TimeOnly.TryParse(source.FlightTKOFF, out var flightTKOFF) ? flightTKOFF : default,
            Type = source.FlightType,
            YearOfDeparture = source.FlightYY,
            Cancelled = source.Cancelled,
            EstimatedDateOfArrival = DateOnly.ParseExact(source.ETADay, Constants.Application.DateOnlyIncomingFormats),
            EstimatedDateOfDeparture = DateOnly.ParseExact(source.ETDday, Constants.Application.DateOnlyIncomingFormats),
            NextCarrier = source.NextCarrier,
            NextDEP = source.NextDEP,
            NextDay = !source.NextDay.IsNullOrWhiteSpace() ? DateOnly.ParseExact(source.NextDay, Constants.Application.DateOnlyIncomingFormats) : default,
            NextFlt = source.NextFlt,
            NextLegCd = source.NextLegCd,
            NextStd = source.NextStd,
            TaxiArr = source.TaxiArr,
            TaxiDep = source.TaxiDep,
        };

        public string AircraftType { get; set; }
        public DateOnly ActualDateOfArrival { get; set; }
        public DateOnly ActualDateOfDeparture { get; set; }
        public string Arrival { get; set; }
        public object ArrivalDelays { get; set; }
        public RouteDetail[] AssociatedCrewRoutes { get; set; }
        public TimeOnly? ActualTimeOfArrival { get; set; }
        public TimeOnly? ActualTimeOfDeparture { get; set; }
        public TimeOnly? FlightBlkTime { get; set; }
        public string Carrier { get; set; }
        public string DayOfDeparture { get; set; }
        public DateOnly Date { get; set; }
        public string Departure { get; set; }
        public object DepartureDelays { get; set; }
        public string Description { get; set; }
        public TimeOnly? EstimatedTimeOfArrival { get; set; }
        public TimeOnly? EstimatedTimeOfDeparture { get; set; }
        public string FlightLegCD { get; set; }
        public string MonthOfDeparture { get; set; }
        public int FlightNo { get; set; }
        public int Capacity { get; set; }
        public string Registration { get; set; }
        public string Status { get; set; }
        public TimeOnly? ScheduledTimeOfArrival { get; set; }
        public TimeOnly? ScheduledTimeOfDeparture { get; set; }
        public TimeOnly? TimeOfTouchDown { get; set; }
        public TimeOnly? TimeOfTakeOff { get; set; }
        public string Type { get; set; }
        public string YearOfDeparture { get; set; }
        public string Cancelled { get; set; }
        public DateOnly EstimatedDateOfArrival { get; set; }
        public DateOnly EstimatedDateOfDeparture { get; set; }
        public short NextCarrier { get; set; }
        public string NextDEP { get; set; }
        public DateOnly? NextDay { get; set; }
        public short NextFlt { get; set; }
        public string NextLegCd { get; set; }
        public short NextStd { get; set; }
        public short TaxiArr { get; set; }
        public short TaxiDep { get; set; }
    }
}
