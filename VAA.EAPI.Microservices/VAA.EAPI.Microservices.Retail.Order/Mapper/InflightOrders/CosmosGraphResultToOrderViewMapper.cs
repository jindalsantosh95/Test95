using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.CosmosGraph;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders;

namespace VAA.EAPI.Microservices.Retail.Order.Mapper.InflightOrders
{
    public class CosmosGraphResultToOrderViewMapper
    {
        private static int upperBound = 0;
        private static int lowerBound = 0;

        /// <summary>
        /// Maps Cosmos Graph Data to Order View response with flights
        /// </summary>
        /// <param name="cosmosResult"></param>
        /// <param name="upperRange"></param>
        /// <param name="lowerRange"></param>
        /// <returns></returns>
        public static List<FlightOrders> MapCosmosGraphResultToOrderView(string cosmosResult, int upperRange, int lowerRange)
        {
            upperBound = upperRange;
            lowerBound = lowerRange;
            return MapCosmosGraphResultToOrderView(cosmosResult);
        }
        /// <summary>
        /// Maps Cosmos Graph Data to Order View response
        /// </summary>
        /// <param name="cosmosResult"></param>
        /// <returns></returns>
        public static List<FlightOrders> MapCosmosGraphResultToOrderView(string cosmosResult)
        {
            var cosmosObjectResult = Json.Deserialize<Dictionary<string, TreeVertexNode>>(cosmosResult);
            bool getAllRecords = upperBound == 0 && lowerBound == 0 ? true : false;

            List<FlightOrders> orderViewResponse = new List<FlightOrders>();
            foreach (var keyValue in cosmosObjectResult)
            {
                int orderCnt = 0;
                foreach (var customerEdgeNode in keyValue.Value.EdgeNodes)
                {
                    var customerEdge = customerEdgeNode.Value.Edge;

                    if (customerEdge.Label == "Makes")
                    {
                        var orderVertexNode = customerEdgeNode.Value.VertexNode;
                        foreach (var orderVertex in orderVertexNode)
                        {
                            var order = orderVertex.Value;
                            var orderId = GetPropertyValue(order.Vertex, "orderID").ToString();
                            orderViewResponse.Add(new FlightOrders()
                            {
                                OrderId = new OrderNumber() { OrderId = orderId },
                                CreationDateTime = new LocalAndUTCDateTime() { LocalDateTime = DateTimeHelper.UnixTimeStampToDateTime(GetPropertyValue(customerEdge, "orderlocalDateTime")) },
                                BookingReferences = new List<BookingReference>()
                                {
                                    new BookingReference()
                                    {
                                        Rloc = orderId.Substring(0,6),
                                        AirlineDesignator=null,
                                        RlocCreateDate = Convert.ToDateTime(orderId.Substring(6))
                                    }
                                },
                                OrderItems = new List<OrderItem>()
                            });


                            foreach (var orderItemEdge in order.EdgeNodes)
                            {
                                if (orderItemEdge.Value.Edge.Label == "ConsistsOf")
                                {
                                    var orderItemVertexNode = orderItemEdge.Value.VertexNode;

                                    foreach (var orderItem in orderItemVertexNode)
                                    {
                                        orderViewResponse[orderCnt].OrderItems.Add(new OrderItem()
                                        {
                                            OrderItemId = GetPropertyValue(orderItem.Value.Vertex, "partitionKey").ToString(),
                                            ExternalReferences = new ExternalOrderReferences() { ExternalOrderId = GetPropertyValue(orderItem.Value.Vertex, "externalOrderId").ToString() },
                                            Services = new List<FlightProduct>()
                                            {
                                                new FlightProduct()
                                                {
                                                    ProductCode = null,
                                                    ProductClass = null,
                                                    Flights = new List<FlightSegment>()
                                                }
                                            }
                                        });

                                        int orderItemcnt = orderViewResponse[orderCnt].OrderItems.Count;

                                        foreach (var flightSegmentEdge in orderItem.Value.EdgeNodes)
                                        {
                                            if (flightSegmentEdge.Value.Edge.Label == "HasFlightSegment")
                                            {
                                                var flightSegmentVertexNode = flightSegmentEdge.Value.VertexNode;
                                                foreach (var flightSegment in flightSegmentVertexNode)
                                                {
                                                    if (!getAllRecords)
                                                    {
                                                        var departDateTimeOutput = DateTimeHelper.UnixTimeStampToDateTime(GetPropertyValue(flightSegment.Value.Vertex, "scheduledDepartureLocalDateTime"));

                                                        var difference = departDateTimeOutput.Subtract(DateTime.UtcNow).TotalDays;
                                                        if (difference > upperBound || difference < lowerBound)
                                                        {
                                                            orderViewResponse[orderCnt].OrderItems.RemoveAt(orderItemcnt - 1);
                                                            orderItemcnt = orderViewResponse[orderCnt].OrderItems.Count;
                                                            continue;
                                                        }
                                                    }

                                                    if (GetPropertyValue(flightSegment.Value.Vertex, "operatingCarrier").ToString() != "VS")
                                                    {
                                                        orderViewResponse[orderCnt].OrderItems.RemoveAt(orderItemcnt - 1);
                                                        orderItemcnt = orderViewResponse[orderCnt].OrderItems.Count;
                                                        continue;
                                                    }

                                                    orderViewResponse[orderCnt].OrderItems[orderItemcnt - 1].Services[0].Flights.Add(new FlightSegment()
                                                    {
                                                        MarketingCarrier = GetPropertyValue(flightSegment.Value.Vertex, "marketingCarrier").ToString(),
                                                        MarketingFlightNumber = GetPropertyValue(flightSegment.Value.Vertex, "marketingFlightNumber").ToString(),
                                                        MarketingAirlineName = GetPropertyValue(flightSegment.Value.Vertex, "marketingAirlineName").ToString(),
                                                        OperatingCarrier = GetPropertyValue(flightSegment.Value.Vertex, "operatingCarrier").ToString(),
                                                        OperatingAirlineName = GetPropertyValue(flightSegment.Value.Vertex, "operatingAirlineName").ToString(),
                                                        OperatingFlightNumber = GetPropertyValue(flightSegment.Value.Vertex, "operatingFlightNumber").ToString(),
                                                        DepartureAirportCode = GetPropertyValue(flightSegment.Value.Vertex, "departureAirportCodeIATA").ToString(),
                                                        DepartureAirportName = GetPropertyValue(flightSegment.Value.Vertex, "departureAirportName").ToString(),
                                                        DepartureCityCode = GetPropertyValue(flightSegment.Value.Vertex, "departureCityCode").ToString(),
                                                        DepartureCityName = string.IsNullOrEmpty(GetPropertyValue(flightSegment.Value.Vertex, "departureCityName").ToString()) ? null : GetPropertyValue(flightSegment.Value.Vertex, "departureCityName").ToString(),

                                                        DepartureCountryCode = GetPropertyValue(flightSegment.Value.Vertex, "departureCountryCode").ToString(),
                                                        DepartureCountryName = GetPropertyValue(flightSegment.Value.Vertex, "departureCountryName").ToString(),
                                                        ArrivalAirportCode = GetPropertyValue(flightSegment.Value.Vertex, "arrivalAirportCodeIATA").ToString(),
                                                        ArrivalAirportName = GetPropertyValue(flightSegment.Value.Vertex, "arrivalAirportName").ToString(),
                                                        ArrivalCityCode = GetPropertyValue(flightSegment.Value.Vertex, "arrivalCityCode").ToString(),
                                                        ArrivalCityName = string.IsNullOrEmpty(GetPropertyValue(flightSegment.Value.Vertex, "arrivalCityName").ToString()) ? null : GetPropertyValue(flightSegment.Value.Vertex, "arrivalCityName").ToString(),
                                                        ArrivalCountryCode = GetPropertyValue(flightSegment.Value.Vertex, "arrivalCountryCode").ToString(),
                                                        ArrivalCountryName = GetPropertyValue(flightSegment.Value.Vertex, "arrivalCountryName").ToString(),
                                                        ScheduledDepartureTime = new LocalAndUTCDateTime()
                                                        {
                                                            LocalDateTime = DateTimeHelper.UnixTimeStampToDateTime(GetPropertyValue(flightSegment.Value.Vertex, "scheduledDepartureLocalDateTime"))
                                                        },
                                                        ScheduledArrivalTime = new LocalAndUTCDateTime()
                                                        {
                                                            LocalDateTime = DateTimeHelper.UnixTimeStampToDateTime(GetPropertyValue(flightSegment.Value.Vertex, "scheduledArrivalLocalDateTime"))
                                                        }
                                                    });

                                                }
                                            }
                                        }
                                    }





                                }
                            }
                            orderCnt++;
                        }

                    }

                }

            }

            return orderViewResponse.Where(x => x.OrderItems.Any(o => o.Services[0].Flights.Count > 0)).ToList();
        }

        private static object GetPropertyValue(GraphObject graphObject, string propertyName)
        {
            var vertexProperty = graphObject.GetType().GetProperty(propertyName);
            object property = null;
            if (vertexProperty != null)
            {
                property = vertexProperty.GetValue(graphObject, null);
            }
            else
            {
                var objectProperty = graphObject.GetProperties();
                if (objectProperty != null)
                {
                    var propertyExists = objectProperty.Properties.TryGetValue(propertyName, out property);
                    if (propertyExists)
                    {
                        if (graphObject.Type == "vertex")
                        {
                            var propertyValue = Json.Deserialize<List<PropertyValue>>(property.ToString());
                            return propertyValue[0].Value;
                        }
                        else
                            return property;

                    }

                }
            }

            return property;
        }
    }
}
