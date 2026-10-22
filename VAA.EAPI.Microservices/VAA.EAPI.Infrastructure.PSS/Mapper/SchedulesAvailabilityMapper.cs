using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.PSS.Proxy.SchedulesAvailability;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class SchedulesAvailabilityMapper
    {
        public static RetrieveSpecificAvailabilityRequestType RetrieveSpecificAvailabilityRequestMapping(FlightSchedulesAvailabilityQuery flightSchedulesAvailabilityQuery)
        {
            return new RetrieveSpecificAvailabilityRequestType()
            {
                RequestInfo = new RequestInfoType()
                {

                    ApplicationId = flightSchedulesAvailabilityQuery.Air4RequestContext.AppId,
                    DebugOptions = flightSchedulesAvailabilityQuery.Air4RequestContext.DebugInfo,
                    TestLab = flightSchedulesAvailabilityQuery.Air4RequestContext.TestLab,
                    SessionId = flightSchedulesAvailabilityQuery.SessionId,
                },
                TravelReqData = new TravelReqDataType[]
                                                               {
                                        new TravelReqDataType() {
                                                AirlineCode = flightSchedulesAvailabilityQuery.Carrier,
                                                OriginStation = flightSchedulesAvailabilityQuery.Origin,
                                                DestinationStation = flightSchedulesAvailabilityQuery.Destination,
                                                FlightNumber = flightSchedulesAvailabilityQuery.FlightNumber,
                                                TripDate = flightSchedulesAvailabilityQuery.FlightDate.ToString("ddMMM").ToUpper()
                                        }
                            }
            };
        }


        public static Segment RetrieveSpecificAvailabilityResponseMapping(FlightSchedulesAvailabilityQuery flightSchedulesAvailabilityQuery, RetrieveSpecificAvailabilityResponseType retrieveSpecificAvailabilityResponseType)
        {

            var response = retrieveSpecificAvailabilityResponseType.FlightSegmentData.Where(s => s.SegmentBoardPoint == flightSchedulesAvailabilityQuery.Origin)
                                                                    .First(s => s.SegmentOffPoint == flightSchedulesAvailabilityQuery.Destination);

            return new Segment
            {
                ScheduledDepartureTime = new Domain.Virgin.Models.Common.UtcAndLocalDateTime { LocalDateTime = flightSchedulesAvailabilityQuery.FlightDate.ToString(Constants.Application.DateTimeFormat) },
                OperatingCarrier = response.OperatingCarrierCode,
                MarketingCarrier = response.MarketingCarrierCode,
                MarketingFlightNumber = response.FlightNumber,
                OriginAirportCode = response.SegmentBoardPoint,
                DestinationAirportCode = response.SegmentOffPoint,
                AirCraft = response.InventoryEquipmentCode,
                Class = PopulateClassAndAvailabilityDataType(retrieveSpecificAvailabilityResponseType.ClassAndAvailabilityData).ToList(),
                Compartment = PopulateCompartment(retrieveSpecificAvailabilityResponseType.CompartmentAvailabilityData).ToList(),
            };
        }

        private static IEnumerable<ClassAvailability> PopulateClassAndAvailabilityDataType(ClassAndAvailabilityDataType[] classAndAvailabilityDataType)
        {

            return classAndAvailabilityDataType
                .Select(cad => new ClassAvailability
                {
                    Class = cad.ClassCode,
                    Compartment = cad.PhysicalCompartmentCode.Is("C") ? "J" : cad.PhysicalCompartmentCode,
                    AuthorisedAvailability = int.Parse(cad.AuthorizedAvailability),
                    ProhibitSalesIndicator = cad.ProhibitSalesIndicator.Is("Y"),
                    Hierachy = int.Parse(cad.InventoryVirtualNumberAssignment),
                    WaitlistOpen = cad.WaitListOpenIndicator.Is("Y"),
                })
                .OrderBy(o => o.Compartment)
                .ThenBy(o => o.Hierachy);

        }

        private static IEnumerable<CompartmentAvailability> PopulateCompartment(CompartmentAvailabilityDataType[] compartmentAvailabilityDataType)
        {

            return (from cad in compartmentAvailabilityDataType
                    select new CompartmentAvailability
                    {
                        Code = cad.CompartmentIndicator,
                        ActualAvailableInventory = int.Parse(cad.ActualAvailableInventory),
                        AuthorisedAvailableInventory = int.Parse(cad.AuthorizedAvailableInventory),
                        Capacity = int.Parse(cad.ShipCapacityForCompartment),
                        InventorySold = int.Parse(cad.InventorySold)

                    });
        }
    }
}



