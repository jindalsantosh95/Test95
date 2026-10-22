using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.PSS.Entities.SeatAvailability;
using VAA.EAPI.Infrastructure.PSS.Proxy.SeatAvailability;
using VAA.EAPI.Infrastructure.PSS.Repository;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class SeatAvailabilityMapper
    {
        public static RetrievePassengerMapRequestType RetrievePassengerMapRequestMapping(SeatAvailabilityQuery seatAvailabilityQuery)
        {
            return new RetrievePassengerMapRequestType()
            {
                RequestInfo = new RequestInfoType()
                {
                    AppChannelName = seatAvailabilityQuery.Air4RequestContext.AppChannelName,
                    ApplicationId = seatAvailabilityQuery.Air4RequestContext.AppId,
                    DebugOptions = seatAvailabilityQuery.Air4RequestContext.DebugInfo,
                    TestLab = seatAvailabilityQuery.Air4RequestContext.TestLab,
                    HostContextParams = new HostContextParamsType
                    {
                        City = ConfigReader.Get(Constants.Environment.AgentCityCode),
                        PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                    }
                },
                FlightInfo = new FlightInfoType()
                {
                    ArrivalAirportCd = seatAvailabilityQuery.DestinationAirportCode,
                    DepartureAirportCd = seatAvailabilityQuery.OriginAirportCode,
                    DepartureDate = seatAvailabilityQuery.ScheduledDepartureTime,
                    MarketingCarrierCode = seatAvailabilityQuery.MarketingCarrier,
                    MarketingFlightNumber = seatAvailabilityQuery.MarketingFlightNumber,
                    OperatingCarrierCode = seatAvailabilityQuery.OperatingCarrier,
                    OperatingFlightNumber = seatAvailabilityQuery.OperatingFlightNumber,
                    SegmentNumber = seatAvailabilityQuery.SegmentId
                },
                CurrencyCode = seatAvailabilityQuery.CurrencyCode,
                RecordLocator = seatAvailabilityQuery.PnrNumber
            };
        }
        public static Entities.SeatAvailability.RetrievePassengerMapResponse RetrievePassengerMapResponseMapping(Proxy.SeatAvailability.RetrievePassengerMapResponse retrievePassengerMapResponse)
        {
            return new Entities.SeatAvailability.RetrievePassengerMapResponse()
            {
                FlightSegment = GetFlightSegment(retrievePassengerMapResponse.RetrievePassengerMapResponse1.FlightInfo),
                PassengerCabins = GetPassengerCabins(retrievePassengerMapResponse.RetrievePassengerMapResponse1.PassengerCabin)
            };
        }

        private static FlightSegment GetFlightSegment(FlightInfoType flightInfo)
        {
            return new FlightSegment()
            {
                DestAirportCode = flightInfo.ArrivalAirportCd,
                OriginAirportCode = flightInfo.DepartureAirportCd,
                MarketingCarrier = flightInfo.MarketingCarrierCode,
                MarketingFlightNum = flightInfo.MarketingFlightNumber,
                OperatingCarrier = new()
                {
                    Code = flightInfo.OperatingCarrierCode
                },
                OperatingFlightNum = flightInfo.OperatingFlightNumber,
                SchedDepartLocalTs = flightInfo.DepartureDate,
                Id = Convert.ToInt32(flightInfo.SegmentNumber)
            };
        }
        private static List<SeatMap> GetPassengerCabins(IEnumerable<PassengerCabinType> passengerCabinTypes)
        {
            List<SeatMap> passengerCabins = new();
            foreach (var cabins in passengerCabinTypes)
            {
                passengerCabins.Add(new SeatMap()
                {
                    ClassType = cabins.ClassType,
                    Equipment = cabins.Equipment,
                    Location = cabins.Location,
                    SeatConfiguration = cabins.SeatConfiguration,
                    SeatRows = GetPassengerRows(cabins.PassengerRow)
                });
            }
            return passengerCabins;
        }
        private static List<SeatRow> GetPassengerRows(IEnumerable<PassengerRowType> passengerRowType)
        {
            List<SeatRow> passengerRows = new();
            foreach (var passengerRow in passengerRowType)
            {
                passengerRows.Add(new SeatRow()
                {
                    Number = passengerRow.Number,
                    ExitOnLeft = passengerRow.ExitOnLeft,
                    ExitOnRight = passengerRow.ExitOnRight,
                    Missing = passengerRow.Missing,
                    OverWing = passengerRow.OverWing,
                    SeatColumns = GetPassengerColumns(passengerRow.PassengerColumn)
                });
            }
            return passengerRows;
        }
        private static List<SeatColumn> GetPassengerColumns(IEnumerable<PassengerColumnType> passengerColumnTypes)
        {
            List<SeatColumn> passengerColumns = new();
            foreach (var passengerCol in passengerColumnTypes)
            {
                passengerColumns.Add(new SeatColumn()
                {
                    BrandId = passengerCol.BrandId,
                    index = passengerCol.Index,
                    Available = passengerCol.Available,
                    OccupationCode = passengerCol.OccupationCode,
                    Pos = passengerCol.Pos,
                    SeatId = passengerCol.SeatId,
                    SeatProductId = passengerCol.SeatProductId,
                    Aisle = passengerCol.Aisle,
                    CharacteristicCodes = passengerCol.CharacteristicCodes,
                    Reserved = passengerCol.Reserved
                });
            }
            return passengerColumns;
        }
    }
}
