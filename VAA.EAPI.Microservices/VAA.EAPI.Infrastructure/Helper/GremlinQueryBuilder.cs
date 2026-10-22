using System;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Enum;
using VAA.EAPI.Domain.Models.CosmosGraph;

namespace VAA.EAPI.Infrastructure.Helper
{
    public static class GremlinQueryBuilder
    {

        #region "Public Methods"

        /// <summary>
        /// Get Gremlin Query for GraphObject
        /// </summary>
        /// <param name="value"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public static string GetGremlinQuery(GraphObject value, GraphOperation operationType)
        {

            switch (operationType)
            {
                case GraphOperation.CreateVertex:
                    switch (value.Label)
                    {
                        case "customer":
                            return CreateCustomer(value);
                        case "order":
                            return CreateOrder(value);
                        case "orderitem":
                            return CreateOrderItem(value);
                        case "flightsegment":
                            return CreateFlightSegment(value);
                        default:
                            throw new NotImplementedException("This vertex not supported by graph");
                    }

                case GraphOperation.CheckVertex:
                    switch (value.Label)
                    {
                        case "customer":
                            return CheckCustomer(value);
                        case "order":
                            return CheckOrder(value);
                        case "orderitem":
                            return CheckOrderItem(value);
                        case "flightsegment":
                            return CheckFlightSegment(value);
                        default:
                            throw new NotImplementedException("This vertex not supported by graph");
                    }
                case GraphOperation.CreateEdge:
                    switch (value.Label)
                    {
                        case "Makes":
                            return CreateEdgeMakes(value);
                        case "ConsistsOf":
                            return CreateEdgeOrderConsistsOfOrderItem(value);
                        case "HasFlightSegment":
                            return CreateEdgeOrderItemHasFlightSegment(value);
                        case "CustomerFliesSegment":
                            return CreateEdgeCustomerFliesSegment(value, false);
                        default:
                            throw new NotImplementedException("This vertex not supported by graph");
                    }
                case GraphOperation.CheckEdge:
                    switch (value.Label)
                    {

                        case "Makes":
                            return CheckEdgeMakes(value);
                        case "ConsistsOf":
                            return CheckEdgeOrderConsistsOfOrderItem(value);
                        case "HasFlightSegment":
                            return CheckEdgeOrderItemHasFlightSegment(value);
                        case "CustomerFliesSegment":
                            return CheckEdgeCustomerFliesSegment(value, false);
                        default:
                            throw new NotImplementedException("This edge check not supported by graph");
                    }
                case GraphOperation.CheckEdgeReverse:
                    switch (value.Label)
                    {
                        case "CustomerFliesSegment":
                            return CheckEdgeCustomerFliesSegment(value, true);
                        default:
                            throw new NotImplementedException("No Reverse edge exists for this Edge.");
                    }
                case GraphOperation.CreateEdgeReverse:
                    switch (value.Label)
                    {
                        case "CustomerFliesSegment":
                            return CreateEdgeCustomerFliesSegment(value, true);
                        default:
                            throw new NotImplementedException("No Reverse edge exists for this Edge.");
                    }
                case GraphOperation.UpdateVertex:
                    throw new NotImplementedException("Update Vertex Not Supported");
                case GraphOperation.DeleteVertex:
                    throw new NotImplementedException("Update Vertex Not Supported");
                case GraphOperation.GetVertex:
                    throw new NotImplementedException("Graph Operation Not Supported");
                    break;
                case GraphOperation.GetEdge:
                    throw new NotImplementedException("Graph Operation Not Supported");
                    break;
                default:
                    throw new NotImplementedException("Graph Operation Not Supported");

            }

        }

        /// <summary>
        /// Gets Query to fetch Customer Flights from Cosmos Graph DB
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetCustomerFlightsGraphQuery(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.V()");
            gremlinQuery.AppendFormat(".hasLabel('customer').has('partitionKey', '{0}').outE('Makes').inV().outE('ConsistsOf').inV().outE('HasFlightSegment').inV().tree()", ExtractGraphObjectPropertyValue(value, "CustomerID"));
            return gremlinQuery.ToString();
        }

        #endregion

        #region "Private Methods"

        private static object ExtractGraphObjectPropertyValue(GraphObject value, string propertyName)
        {
            var vertexProperty = value.GetType().GetProperty(propertyName);
            object propertyValue = null;
            if (vertexProperty != null)
            {
                propertyValue = vertexProperty.GetValue(value, null);
            }
            else
            {
                var objectProperty = value.GetProperties();
                if (objectProperty != null)
                {
                    var valueExists = objectProperty.Properties.TryGetValue(propertyName, out propertyValue);
                    if (valueExists)
                        return propertyValue;
                }
            }

            return propertyValue;
        }


        private static string CreateCustomer(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            gremlinQuery.AppendFormat("addV('{0}')", value.Label);
            gremlinQuery.Append(GetPropertyQuery("id", ExtractGraphObjectPropertyValue(value, "CustomerID"), "s"));
            gremlinQuery.Append(GetPropertyQuery("partitionKey", ExtractGraphObjectPropertyValue(value, "CustomerID"), "s"));
            gremlinQuery.Append(GetPropertiesQuery(value.GetProperties()));
            return gremlinQuery.ToString();
        }

        private static string CheckCustomer(GraphObject value)
        {
            var customerId = ExtractGraphObjectPropertyValue(value, "CustomerID");
            StringBuilder gremlinQuery = new StringBuilder("g.V().hasLabel('customer').has('partitionKey', '" + customerId + "').Count()");
            return gremlinQuery.ToString();
        }

        private static string CreateOrder(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            string orderId = value.GetType().GetProperty("OrderId").GetValue(value, null).ToString();
            gremlinQuery.AppendFormat("addV('{0}')", value.Label);
            gremlinQuery.Append(GetPropertyQuery("id", orderId, "s"));
            gremlinQuery.Append(GetPropertyQuery("partitionKey", orderId, "s"));
            gremlinQuery.Append(GetPropertiesQuery(value.GetProperties()));
            return gremlinQuery.ToString();
        }

        private static string CheckOrder(GraphObject value)
        {
            string orderId = value.GetType().GetProperty("OrderId").GetValue(value, null).ToString();
            StringBuilder gremlinQuery = new StringBuilder("g.V().hasLabel('order').has('partitionKey', '" + orderId + "').Count()");
            return gremlinQuery.ToString();
        }

        private static string CreateOrderItem(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            string partitionKey = ExtractGraphObjectPropertyValue(value, "OrderId").ToString()
                 + "-" +
                Convert.ToInt32(ExtractGraphObjectPropertyValue(value, "OrderItemId")).ToString("D2");
            gremlinQuery.AppendFormat("addV('{0}')", value.Label);
            gremlinQuery.Append(GetPropertyQuery("id", partitionKey, "s"));
            gremlinQuery.Append(GetPropertyQuery("partitionKey", partitionKey, "s"));
            gremlinQuery.Append(GetPropertiesQuery(value.GetProperties()));
            return gremlinQuery.ToString();
        }

        private static string CheckOrderItem(GraphObject value)
        {
            string orderItemId = ExtractGraphObjectPropertyValue(value, "OrderId").ToString()
                 + "-" +
                Convert.ToInt32(ExtractGraphObjectPropertyValue(value, "OrderItemId")).ToString("D2");
            StringBuilder gremlinQuery = new StringBuilder("g.V().hasLabel('orderitem').has('partitionKey', '" + orderItemId + "').Count()");
            return gremlinQuery.ToString();
        }

        private static string CreateFlightSegment(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            string partitionKey = GetFlightSegmentPartitionKey(value);
            gremlinQuery.AppendFormat("addV('{0}')", value.Label);
            gremlinQuery.Append(GetPropertyQuery("id", partitionKey.ToUpper(), "s"));
            gremlinQuery.Append(GetPropertyQuery("partitionKey", partitionKey.ToUpper(), "s"));
            gremlinQuery.Append(GetPropertiesQuery(value.GetProperties()));
            return gremlinQuery.ToString();
        }

        private static string GetFlightSegmentPartitionKey(GraphObject flightSegment)
        {
            string carrierName = "VS", flightNummber;
            if (ExtractGraphObjectPropertyValue(flightSegment, "marketingCarrier").ToString() == carrierName)
                flightNummber = ExtractGraphObjectPropertyValue(flightSegment, "marketingFlightNumber").ToString();
            else if (ExtractGraphObjectPropertyValue(flightSegment, "operatingCarrier").ToString() == carrierName)
                flightNummber = ExtractGraphObjectPropertyValue(flightSegment, "operatingFlightNumber").ToString();
            else
            {
                carrierName = ExtractGraphObjectPropertyValue(flightSegment, "operatingCarrier").ToString();
                flightNummber = ExtractGraphObjectPropertyValue(flightSegment, "marketingFlightNumber").ToString();
            }

            return string.Concat(carrierName, flightNummber.Trim(), DateTimeHelper.UnixTimeStampToDateTime(ExtractGraphObjectPropertyValue(flightSegment, "scheduledDepartureLocalDateTime")).ToString("ddMMMyyyy")).ToUpper();

        }

        private static string CheckFlightSegment(GraphObject value)
        {
            string id = GetFlightSegmentPartitionKey(value);
            StringBuilder gremlinQuery = new StringBuilder("g.V().hasLabel('" + value.Label + "').has('partitionKey', '" + id.ToUpper() + "').Count()");
            return gremlinQuery.ToString();
        }

        private static string CreateEdgeMakes(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");

            gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(fromV, "CustomerId") + "')");

            gremlinQuery.AppendFormat(".addE('{0}')", value.Label);
            gremlinQuery.Append(GetPropertiesQuery(value.GetProperties()));

            gremlinQuery.Append(".to(g.V().hasLabel('" + toV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(toV, "OrderId") + "'))");


            return gremlinQuery.ToString();
        }

        private static string CheckEdgeMakes(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");

            gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(fromV, "CustomerId") + "')");

            gremlinQuery.AppendFormat(".outE('{0}')", value.Label);

            gremlinQuery.Append(".inV().hasLabel('" + toV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(toV, "OrderId") + "').Count()");

            return gremlinQuery.ToString();
        }

        private static string CheckEdgeOrderConsistsOfOrderItem(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");

            gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(fromV, "OrderId") + "')");

            gremlinQuery.AppendFormat(".outE('{0}')", value.Label);

            string orderItemPartitionKey = ExtractGraphObjectPropertyValue(toV, "OrderId").ToString() + "-" + Convert.ToInt32(ExtractGraphObjectPropertyValue(toV, "OrderItemId")).ToString("D2");

            gremlinQuery.Append(".inV().hasLabel('" + toV.Label + "').has('partitionKey', '" + orderItemPartitionKey + "').Count()");

            return gremlinQuery.ToString();
        }

        private static string CheckEdgeOrderItemHasFlightSegment(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");

            string orderItemPartitionKey = ExtractGraphObjectPropertyValue(fromV, "OrderId").ToString() + "-" + Convert.ToInt32(ExtractGraphObjectPropertyValue(fromV, "OrderItemId")).ToString("D2");
            gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + orderItemPartitionKey + "')");

            gremlinQuery.AppendFormat(".outE('{0}')", value.Label);

            string flightSegmentPartitionKey = GetFlightSegmentPartitionKey(toV);
            gremlinQuery.Append(".inV().hasLabel('" + toV.Label + "').has('partitionKey', '" + flightSegmentPartitionKey.ToUpper() + "').Count()");

            return gremlinQuery.ToString();
        }

        private static string CheckEdgeCustomerFliesSegment(GraphObject value, bool checkReverse)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");

            if (!checkReverse)
            {
                string flightSegmentPartitionKey = GetFlightSegmentPartitionKey(toV);
                gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(fromV, "CustomerId") + "')");
                gremlinQuery.AppendFormat(".outE('{0}')", value.Label);
                gremlinQuery.Append(".inV().hasLabel('" + toV.Label + "').has('partitionKey', '" + flightSegmentPartitionKey.ToUpper() + "').Count()");
                return gremlinQuery.ToString();
            }
            else
            {
                string flightSegmentPartitionKey = GetFlightSegmentPartitionKey(fromV);
                gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + flightSegmentPartitionKey.ToUpper() + "')");
                gremlinQuery.AppendFormat(".outE('{0}')", value.Label);
                gremlinQuery.Append(".inV().hasLabel('" + toV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(toV, "CustomerId") + "').Count()");
                return gremlinQuery.ToString();
            }

        }

        private static string CreateEdgeOrderConsistsOfOrderItem(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");
            string orderItemPartitionKey = ExtractGraphObjectPropertyValue(toV, "OrderId").ToString() + "-" + Convert.ToInt32(ExtractGraphObjectPropertyValue(toV, "OrderItemId")).ToString("D2");
            gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(fromV, "OrderId") + "')");
            gremlinQuery.AppendFormat(".addE('{0}')", value.Label);
            gremlinQuery.Append(".to(g.V().hasLabel('" + toV.Label + "').has('partitionKey', '" + orderItemPartitionKey + "'))");
            return gremlinQuery.ToString();
        }

        private static string CreateEdgeOrderItemHasFlightSegment(GraphObject value)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");
            string orderItemPartitionKey = ExtractGraphObjectPropertyValue(fromV, "OrderId").ToString() + "-" + Convert.ToInt32(ExtractGraphObjectPropertyValue(fromV, "OrderItemId")).ToString("D2");
            gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + orderItemPartitionKey + "')");
            gremlinQuery.AppendFormat(".addE('{0}')", value.Label);
            string flightSegmentPartitionKey = GetFlightSegmentPartitionKey(toV);
            gremlinQuery.Append(".to(g.V().hasLabel('" + toV.Label + "').has('partitionKey', '" + flightSegmentPartitionKey.ToUpper() + "'))");
            return gremlinQuery.ToString();
        }

        private static string CreateEdgeCustomerFliesSegment(GraphObject value, bool reverseEdge)
        {
            StringBuilder gremlinQuery = new StringBuilder("g.");
            GraphObject fromV = (GraphObject)ExtractGraphObjectPropertyValue(value, "FromV");
            GraphObject toV = (GraphObject)ExtractGraphObjectPropertyValue(value, "ToV");

            if (!reverseEdge)
            {
                string flightSegmentPartitionKey = GetFlightSegmentPartitionKey(toV);
                gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(fromV, "CustomerId") + "')");
                gremlinQuery.AppendFormat(".addE('{0}')", value.Label);
                gremlinQuery.Append(".to(g.V().hasLabel('" + toV.Label + "').has('partitionKey', '" + flightSegmentPartitionKey.ToUpper() + "'))");
                return gremlinQuery.ToString();
            }
            else
            {
                string flightSegmentPartitionKey = GetFlightSegmentPartitionKey(fromV);
                gremlinQuery.Append("V().hasLabel('" + fromV.Label + "').has('partitionKey', '" + flightSegmentPartitionKey.ToUpper() + "')");
                gremlinQuery.AppendFormat(".addE('{0}')", value.Label);
                gremlinQuery.Append(".to(g.V().hasLabel('" + toV.Label + "').has('partitionKey', '" + ExtractGraphObjectPropertyValue(toV, "CustomerId") + "'))");
                return gremlinQuery.ToString();
            }

        }


        private static string GetPropertiesQuery(PropertyValue property)
        {
            StringBuilder propertyString = new StringBuilder("");
            foreach (var propertyValue in property.Properties)
            {
                propertyString.Append(GetPropertyQuery(propertyValue.Key, propertyValue.Value, "s"));
            }
            return propertyString.ToString();
        }

        private static string GetPropertyQuery(string property, object value, string type)
        {
            string inputString = ".property('{0}', '{1}')";

            switch (type)
            {
                case "s":
                    return String.Format(inputString, property, value == null ? "" : value.ToString().Replace("'", "\\'"));
                case "dttm":
                    return String.Format(inputString, property, DateTimeHelper.UnixTimeStampToDateTime(value));
                case "n":
                    int val;
                    var parse = Int32.TryParse(value.ToString(), out val);
                    return String.Format(inputString, property, val);
                case "f":
                    break;
                case "b":
                    break;


            }
            return "NotImplemented";
        }

        #endregion


    }
}
