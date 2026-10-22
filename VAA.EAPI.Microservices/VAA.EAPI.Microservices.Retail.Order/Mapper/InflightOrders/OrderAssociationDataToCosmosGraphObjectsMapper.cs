using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.CosmosGraph;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CosmosGraphModel;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders
{
    public static class OrderAssociationDataToCosmosGraphObjectsMapper
    {

        public static Dictionary<string, GraphObject> GetGraphModel(OrderAssociationRequestData requestData, List<AirportDetails> airportDetails, List<CarrierDetails> carrierDetails)
        {
            Dictionary<string, GraphObject> graphItems = new Dictionary<string, GraphObject>();

            Customer customer = new Customer() { Label = "customer", Type = "vertex", CustomerId = requestData.CustomerId };
            graphItems.Add("Create Vertex Customer", customer);
            foreach (var order in requestData.Orders)
            {
                var orderToAdd = new Domain.InflightOrders.CosmosGraphModel.Order()
                {
                    Label = "order",
                    Type = "vertex",
                    OrderId = order.OrderID
                };
                graphItems.Add("Create Vertex Order", orderToAdd);

                graphItems.Add("Create Edge Makes", new GraphEdge()
                {
                    Label = "Makes",
                    Type = "edge",
                    EdgeProperties = new List<PropertyValue>()
                    {
                       new PropertyValue(){Id = "orderlocalDateTime", Value=DateTimeHelper.ConvertDateTimeToUnix(order.OrderID[6..])}
                    },
                    FromV = customer,
                    ToV = orderToAdd,
                });

                int orderItemId = 0;
                foreach (var segmentDetail in order.SegmentDetails.FindAll(x => !string.IsNullOrEmpty(x.MarketingCarrier) && !string.IsNullOrEmpty(x.OperatingCarrier)))
                {
                    orderItemId++;
                    var orderItemToAdd = new Domain.InflightOrders.CosmosGraphModel.OrderItem()
                    {
                        Label = "orderitem",
                        Type = "vertex",
                        OrderItemId = orderItemId,
                        OrderId = order.OrderID,
                        OwnerAirline = order.OwnerAirline,
                        ExternalOrderId = order.BookingReferences.FirstOrDefault().ExternalRecordLocator
                    };

                    graphItems.Add("Create Vertex OrderItem:" + orderItemId.ToString(), orderItemToAdd);

                    graphItems.Add("Create Edge ConsistsOf:" + orderItemId.ToString(), new GraphEdge()
                    {
                        Label = "ConsistsOf",
                        Type = "edge",
                        FromV = orderToAdd,
                        ToV = orderItemToAdd,
                    });
                    var originAirportCodeInfo = airportDetails.FirstOrDefault(a => a.IataAirportCode == segmentDetail.OriginAirportCode);
                    var destinationAirportCodeInfo = airportDetails.FirstOrDefault(a => a.IataAirportCode == segmentDetail.DestinationAirportCode);
                    var flightSegmentToAdd = new Domain.InflightOrders.CosmosGraphModel.FlightSegment()
                    {
                        Label = "flightsegment",
                        Type = "vertex",

                        DepartureAirportCodeIATA = segmentDetail.OriginAirportCode,
                        DepartureAirportName = originAirportCodeInfo?.Name,
                        DepartureCityCode = originAirportCodeInfo?.CityIataCode,
                        DepartureCityName = null,
                        DepartureCountryCode = originAirportCodeInfo?.CountryCode,
                        DepartureCountryName = originAirportCodeInfo?.Country,

                        ArrivalAirportCodeIATA = segmentDetail.DestinationAirportCode,
                        ArrivalAirportName = destinationAirportCodeInfo?.Name,
                        ArrivalCityCode = destinationAirportCodeInfo?.CityIataCode,
                        ArrivalCityName = null,
                        ArrivalCountryCode = destinationAirportCodeInfo?.CountryCode,
                        ArrivalCountryName = destinationAirportCodeInfo?.Country,


                        MarketingCarrier = segmentDetail.MarketingCarrier,
                        MarketingFlightNumber = segmentDetail.MarketingFlightNumber,
                        MarketingAirlineName = carrierDetails.FirstOrDefault(c => c.IataAlphaCode == segmentDetail.MarketingCarrier)?.AirlineName,

                        OperatingCarrier = segmentDetail.OperatingCarrier,
                        OperatingFlightNumber = segmentDetail.OperatingFlightNumber,
                        OperatingAirlineName = carrierDetails.FirstOrDefault(c => c.IataAlphaCode == segmentDetail.OperatingCarrier)?.AirlineName,

                        ScheduledDepartureLocalDateTime = DateTimeHelper.ConvertDateTimeToUnix(DateTimeHelper.Air4DepartDateFormatConversion(segmentDetail.DepartureDate, segmentDetail.SegmentDepartureYear, segmentDetail.DepartureTime).ToString()).ToString(),
                        ScheduledArrivalLocalDateTime = DateTimeHelper.ConvertDateTimeToUnix(DateTimeHelper.Air4ArrivalDateFormatConversion(segmentDetail.LegArrivalDate, segmentDetail.ArrivalTime).ToString()).ToString(),

                    };

                    graphItems.Add("Create Vertex FlightSegment:" + orderItemId.ToString(), flightSegmentToAdd);

                    graphItems.Add("Create Edge HasFlightSegment:" + orderItemId.ToString(), new GraphEdge()
                    {
                        Label = "HasFlightSegment",
                        Type = "edge",
                        FromV = orderItemToAdd,
                        ToV = flightSegmentToAdd,
                    });

                    graphItems.Add("Create Edge CustomerFliesSegment:" + orderItemId.ToString(), new GraphEdge()
                    {
                        Label = "CustomerFliesSegment",
                        Type = "edge",
                        FromV = customer,
                        ToV = flightSegmentToAdd,
                    });

                    graphItems.Add("Create Reverse Edge CustomerFliesSegment:" + orderItemId.ToString(), new GraphEdge()
                    {
                        Label = "CustomerFliesSegment",
                        Type = "edge",
                        FromV = flightSegmentToAdd,
                        ToV = customer,
                    });
                }

            }

            return graphItems;
        }


    }
}
