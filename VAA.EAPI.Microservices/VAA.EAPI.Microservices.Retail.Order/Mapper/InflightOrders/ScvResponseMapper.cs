using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.SCVResults;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders
{
    public static class ScvResponseMapper
    {
        /// <summary>
        /// To map scvordercollection document to flightorders
        /// </summary>
        /// <param name="orderCollection">SCV OrderCollection dodument</param>
        /// <returns>List of FlightOrders</returns>        
        public static List<FlightOrders> MapScvOrderResponse(List<OrderCollectionResult> orderCollection, List<AirportDetails> airportDetails, List<CarrierDetails> carrierDetails)
        {
            return MapScvOrderResponse(orderCollection, true, airportDetails, carrierDetails);
        }

        private static List<FlightOrders> MapScvOrderResponse(List<OrderCollectionResult> orderCollection, bool allRecords, List<AirportDetails> airportDetails, List<CarrierDetails> carrierDetails)
        {
            var flightOrders = new List<FlightOrders>();
            int flightOrderCount = 0;

            foreach (var orderDocumentfromSCV in orderCollection)
            {
                flightOrders.Add(new FlightOrders()
                {
                    OrderId = new OrderNumber()
                    {
                        OrderId = orderDocumentfromSCV.OrderID,
                        OwnerAirline = orderDocumentfromSCV.OwnerAirline
                    },
                    CreationDateTime = new LocalAndUTCDateTime()
                    {
                        UtcDateTime = null,
                        LocalDateTime = Convert.ToDateTime(orderDocumentfromSCV.OrderID[6..])
                    },
                    BookingReferences = new List<Domain.InflightOrders.BookingReference>(),
                    OrderItems = new List<OrderItem>()

                });

                if (orderDocumentfromSCV.BookingReferences != null)
                {
                    foreach (var bookingReference in orderDocumentfromSCV.BookingReferences)
                    {
                        flightOrders[flightOrderCount].BookingReferences.Add(new Domain.InflightOrders.BookingReference()
                        {
                            AirlineDesignator = null,
                            Rloc = bookingReference.RLOC,
                            RlocCreateDate = Convert.ToDateTime(bookingReference.RLOCCreateDate)
                        });
                    }
                }

                int orderItemNum = 0;
                if (orderDocumentfromSCV.SegmentDetails != null && orderDocumentfromSCV.SegmentDetails.Count > 0)
                {
                    foreach (var segmentDetail in orderDocumentfromSCV.SegmentDetails)
                    {
                        if ((segmentDetail.FlightWithInRetailWindow || allRecords) && segmentDetail.OperatingCarrier == "VS")
                        {
                            orderItemNum += 1;
                            flightOrders[flightOrderCount].OrderItems.Add(new OrderItem()
                            {
                                OrderItemId = orderDocumentfromSCV.OrderID + "-" + orderItemNum.ToString("D2"),
                                ExternalReferences = new ExternalOrderReferences()
                                {
                                    OwnerAirline = orderDocumentfromSCV.OwnerAirline,
                                    ExternalOrderId = orderDocumentfromSCV.BookingReferences.FirstOrDefault().ExternalRecordLocator
                                },
                                Services = new List<FlightProduct>()
                                {
                                    new FlightProduct()
                                    {
                                       ProductCode = null,
                                       ProductClass = null,
                                       Flights = new List<FlightSegment>()
                                       {
                                           new FlightSegment()
                                           {
                                               MarketingCarrier = segmentDetail.MarketingCarrier,
                                               MarketingFlightNumber = segmentDetail.MarketingFlightNumber,
                                               MarketingAirlineName = carrierDetails.FirstOrDefault(c=>c.IataAlphaCode == segmentDetail.MarketingCarrier)?.AirlineName,
                                               OperatingCarrier = segmentDetail.OperatingCarrier,
                                               OperatingFlightNumber = segmentDetail.OperatingFlightNumber,
                                               OperatingAirlineName = carrierDetails.FirstOrDefault(c=>c.IataAlphaCode == segmentDetail.OperatingCarrier)?.AirlineName,

                                               DepartureAirportCode = segmentDetail.OriginAirportCode,
                                               DepartureAirportName  = airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.OriginAirportCode)?.Name,
                                               DepartureCityCode = airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.OriginAirportCode)?.CityIataCode,
                                               DepartureCityName = null,
                                               DepartureCountryCode= airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.OriginAirportCode)?.CountryCode,
                                               DepartureCountryName=airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.OriginAirportCode)?.Country,

                                               ArrivalAirportCode = segmentDetail.DestinationAirportCode,
                                               ArrivalAirportName = airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.DestinationAirportCode)?.Name,
                                               ArrivalCityCode = airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.DestinationAirportCode)?.CityIataCode,
                                               ArrivalCityName = null,
                                               ArrivalCountryCode = airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.DestinationAirportCode)?.CountryCode,
                                               ArrivalCountryName = airportDetails.FirstOrDefault(a=>a.IataAirportCode == segmentDetail.DestinationAirportCode)?.Country,

                                               ScheduledDepartureTime = new LocalAndUTCDateTime()
                                               {
                                                    LocalDateTime = DateTimeHelper.Air4DepartDateFormatConversion(segmentDetail.DepartureDate, segmentDetail.SegmentDepartureYear, segmentDetail.DepartureTime),
                                                    UtcDateTime = null
                                               },
                                               ScheduledArrivalTime =new LocalAndUTCDateTime()
                                               {
                                                    LocalDateTime = DateTimeHelper.Air4ArrivalDateFormatConversion(segmentDetail.LegArrivalDate,segmentDetail.ArrivalTime),
                                                    UtcDateTime = null
                                               }
                                           }
                                       }
                                    }
                                }
                            });

                        }
                    }
                }

                flightOrderCount += 1;
            }
            return flightOrders.Where(f => f.OrderItems.Count > 0).ToList();
        }
        /// <summary>
        /// To map scvordercollection document to flightorders with flight date falling in the provided order window.
        /// </summary>
        /// <param name="orderCollection">SCV OrderCollection dodument</param>
        /// <param name="upperBound">Past Days</param>
        /// <param name="lowerBound">Future Days</param>
        /// <returns>List of FlightOrders</returns>
        public static List<FlightOrders> MapScvOrderResponse(List<OrderCollectionResult> orderCollection, int upperBound, int lowerBound, List<AirportDetails> airportDetails, List<CarrierDetails> carrierDetails)
        {
            orderCollection = orderCollection.Where(d => d.SegmentDetails.Count > 0).ToList();
            return MapScvOrderResponse(CalculateInflightOrderWindow(orderCollection, upperBound, lowerBound), false, airportDetails, carrierDetails);
        }

        private static List<OrderCollectionResult> CalculateInflightOrderWindow(List<OrderCollectionResult> scvOrdersCollectionRecords, int upperBound, int lowerBound)
        {
            scvOrdersCollectionRecords.ForEach(d =>
            {
                d.SegmentDetails?.ForEach(s =>
                {
                    var departDateTimeOutput = DateTimeHelper.Air4DepartDateFormatConversion(s.DepartureDate, s.SegmentDepartureYear, s.DepartureTime);
                    if (departDateTimeOutput.HasValue)
                    {
                        var difference = departDateTimeOutput.Value.Subtract(DateTime.UtcNow).TotalDays;
                        if (difference <= upperBound && difference >= lowerBound)
                        {
                            s.FlightWithInRetailWindow = true;
                            s.FlightDepartureDateTimeLocal = departDateTimeOutput.Value;
                            s.DateDifference = difference;
                        }
                    }
                });
            });
            return scvOrdersCollectionRecords;
        }
    }
}
