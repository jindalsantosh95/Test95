using System.Globalization;
using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;
using LocationType = VAA.EAPI.Automation.Core.Helpers.Enums.LocationType;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class UpdatePNRDetails
    {
        public static Dictionary<string, string> ChangeTravelDate(string airShoppingRequest,string orderCreateResult, string cabinCode = "5",  JourneyType journeyType = JourneyType.OneWay, LocationType locationOne = LocationType.LHR, LocationType locationTwo = LocationType.ATL, LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, int serviceCount = 0, SeatPriceType seatType = SeatPriceType.None, string flightType = "", string corporate = "", string passport = "", string greencard = "", string visa = "", string knowntravller = "", string redress = "", ChangeOrder changeOrder = RequestHelper.ChangeOrder.None, string reshopFlightType = "", string agentType = "Direct", string participatingOrgOrgID2 = "", ApiResult apiResult = ApiResult.None, string exchangeType = "EvenExchange", string flow = "", string orderChangePayment = "")
        {
            var updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            //Post Air Shopping request
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            var originalJourney1Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;

            string originalJourney2Date = "";

            if (journeyType.Equals(JourneyType.Return) || journeyType.Equals(JourneyType.MultiCity))
            {
                originalJourney2Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[2]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;
            }

            string originalJourney3Date = "";

            if (journeyType.Equals(JourneyType.MultiCity))
            {
                originalJourney3Date = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[3]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;
            }

            var result = new Dictionary<string, string>();

            var orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            if (apiResult.Equals(ApiResult.OrderRetrieveRQ))
            {
                result.Add("orderRetrieveRQ", orderRetrieveRequest);

                return result;
            }

            //Order Retrieve request
            var orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureIsSuccessStatusCode();

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("orderRetrieveRS", orderRetrieveRequest);

            if (apiResult.Equals(ApiResult.OrderRetrieveRS))
            {
                return result;
            }

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;

            var bookingID = orderRetrieveResponseXmlDocument.GetElementsByTagName("BookingID");

            var currencyCode= orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:TotalPrice//ct:TotalAmount//@CurCode", xmlNamespaceManager).InnerText;

            var countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:Order//ct:OrderItem[ct:FareDetail]", xmlNamespaceManager).Count;

            int counter = 0;
            var orderReshopRequest = "";
            string orderReshopResult = "";
            int retainJourneySegmentCount;
            IDictionary<string, string> reshopOfferDetails = new Dictionary<string, string>();

            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            result["orderReshopRQ"] = "";
            result["orderReshopRS"] = "";

            if (changeOrder.Equals(RequestHelper.ChangeOrder.CompleteJourney))
            {
                do
                {
                    DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter);

                    string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");

                    string reshopJourney2Date = "";

                    string reshopJourney3Date = "";

                    if (journeyType.Equals(JourneyType.Return) || journeyType.Equals(JourneyType.MultiCity))
                    {
                        DateTime date1 = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime newDate1 = date1.AddDays(counter);
                        reshopJourney2Date = newDate1.ToString("yyyy-MM-dd");
                    }

                    if (journeyType.Equals(JourneyType.MultiCity))
                    {
                        DateTime date2 = DateTime.ParseExact(originalJourney3Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime newDate2 = date2.AddDays(counter);
                        reshopJourney3Date = newDate2.ToString("yyyy-MM-dd");
                    }

                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date, datethree: reshopJourney3Date, flighttype: reshopFlightType);

                    result["orderReshopRQ"] = orderReshopRequest;

                    if (apiResult.Equals(ApiResult.OrderReshopRQ))
                    {
                        return result;
                    }

                    orderReshopRequest = FindAndReturnDataHelper.ReturnNewOrderItemID(orderReshopRequest, orderRetrieveResult);

                    //Post Order Reshop request
                    var orderReshopResponse = NDCTransformationEndpoints.OrderReshop(orderReshopRequest);

                    //Getting the Order Reshop response and converting to XML Document
                    orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    orderReshopResponseXmlDocument = new XmlDocument();
                    orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                    result["orderReshopRS"] = orderReshopResult;

                    if (apiResult.Equals(ApiResult.OrderReshopRS) && orderReshopResponse.IsSuccessStatusCode)
                    {
                        return result;
                    }

                    if ((apiResult.Equals(ApiResult.OrderQuoteRQ) || apiResult.Equals(ApiResult.OrderQuoteRS)) && orderReshopResponse.IsSuccessStatusCode)
                    {
                        break;
                    }

                    //Find offer based on Even Exchange
                    reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, exchangeType: exchangeType);

                    counter++;

                    if (reshopOfferDetails.Count > 0)
                    {
                        break;
                    }

                } while (counter < 6);
            }

            if (changeOrder.Equals(RequestHelper.ChangeOrder.OutBoundJourney))
            {
                do
                {
                    DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter++);

                    string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,JourneyType.OneWay, locationOne: locationOne, locationTwo: locationTwo, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourney1Date);

                    var updatedOrderReshopRequest = new XmlDocument();

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Get the count of segemnts for retaining segment

                    retainJourneySegmentCount = orderRetrieveResponseXmlDocument.SelectNodes("//ct:PaxJourney[2]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int reshopJourneySegmentCount = orderRetrieveResponseXmlDocument.SelectNodes("//ct:PaxJourney[1]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int totalSegmentsCount = retainJourneySegmentCount + reshopJourneySegmentCount;

                    //Update the service ID's in the reshop request

                    var deleteOrderItem = updatedOrderReshopRequest.SelectNodes("//ct:DeleteOrderItem", xmlNamespaceManager);

                    foreach (XmlNode node in deleteOrderItem)
                    {
                        for (int j = reshopJourneySegmentCount + 1; j <= totalSegmentsCount; j++)
                        {
                            XmlElement newElement1 = updatedOrderReshopRequest.CreateElement("RetainServiceID", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                            newElement1.InnerText = "SV" + j;
                            node.AppendChild(newElement1);
                        }
                    }

                    result["orderReshopRQ"] = updatedOrderReshopRequest.OuterXml;

                    if (apiResult.Equals(ApiResult.OrderReshopRQ))
                    {
                        return result;
                    }

                    orderReshopRequest = FindAndReturnDataHelper.ReturnNewOrderItemID(orderReshopRequest, orderRetrieveResult);

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Post Order Reshop request
                    var orderReshopResponse = NDCTransformationEndpoints.OrderReshop(updatedOrderReshopRequest.OuterXml);

                    //Getting the Order Reshop response and converting to XML Document
                    orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                    result["orderReshopRS"] = orderReshopResult;

                    if (apiResult.Equals(ApiResult.OrderReshopRS) && orderReshopResponse.IsSuccessStatusCode)
                    {
                        return result;
                    }

                    if ((apiResult.Equals(ApiResult.OrderQuoteRQ) || apiResult.Equals(ApiResult.OrderQuoteRS)) && orderReshopResponse.IsSuccessStatusCode)
                    {
                        break;
                    }

                    //Find offer based on Even Exchange
                    reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, exchangeType: exchangeType);

                    counter++;

                    if (reshopOfferDetails.Count > 0)
                    {
                        break;
                    }

                } while (counter < 6);

            }

            if (changeOrder.Equals(RequestHelper.ChangeOrder.InBoundJourney))
            {
                do
                {
                    DateTime date = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter++);

                    string reshopJourney2Date = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,JourneyType.OneWay, locationOne: locationThree, locationTwo: locationFour, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourney2Date);

                    var updatedOrderReshopRequest = new XmlDocument();

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Get the count of segemnts for retaining segment

                    retainJourneySegmentCount = orderRetrieveResponseXmlDocument.SelectNodes("//ct:PaxJourney[1]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int reshopJourneySegmentCount = orderRetrieveResponseXmlDocument.SelectNodes("//ct:PaxJourney[2]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int totalSegmentsCount = retainJourneySegmentCount + reshopJourneySegmentCount;

                    //Update the service ID's in the reshop request

                    var deleteOrderItem = updatedOrderReshopRequest.SelectNodes("//ct:DeleteOrderItem", xmlNamespaceManager);

                    foreach (XmlNode node in deleteOrderItem)
                    {
                        for (int j = 1; j <= retainJourneySegmentCount; j++)
                        {
                            XmlElement newElement1 = updatedOrderReshopRequest.CreateElement("RetainServiceID", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
                            newElement1.InnerText = "SV" + j;
                            node.AppendChild(newElement1);
                        }
                    }

                    result["orderReshopRQ"] = updatedOrderReshopRequest.OuterXml;

                    if (apiResult.Equals(ApiResult.OrderReshopRQ))
                    {
                        return result;
                    }

                    orderReshopRequest = FindAndReturnDataHelper.ReturnNewOrderItemID(orderReshopRequest, orderRetrieveResult);

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);
                    //Post Order Reshop request
                    var orderReshopResponse = NDCTransformationEndpoints.OrderReshop(updatedOrderReshopRequest.OuterXml);

                    //Getting the Order Reshop response and converting to XML Document
                    orderReshopResult = orderReshopResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

                    result["orderReshopRS"] = orderReshopResult;

                    if (apiResult.Equals(ApiResult.OrderReshopRS) && orderReshopResponse.IsSuccessStatusCode)
                    {
                        return result;
                    }

                    if ((apiResult.Equals(ApiResult.OrderQuoteRQ) || apiResult.Equals(ApiResult.OrderQuoteRS)) && orderReshopResponse.IsSuccessStatusCode)
                    {
                        break;
                    }

                    //Find offer based on Even Exchange
                    reshopOfferDetails = FindAndReturnDataHelper.FindOfferFromReshop(orderReshopResult, exchangeType: exchangeType);

                    counter++;

                    if (reshopOfferDetails.Count > 0)
                    {
                        break;
                    }

                } while (counter < 6);

            }

            string reshopOfferRefId;
            string reshopTotalAmount;

            if (reshopOfferDetails.Count > 0)
            {
                reshopOfferRefId = reshopOfferDetails["OfferRefID"];
                reshopTotalAmount = reshopOfferDetails["TotalAmount"];
            }
            else
            {
                Assert.False(orderReshopResponseXmlDocument.SelectNodes("//ct:ReshopResults//ct:ReshopOffers//ct:Offer", xmlNamespaceManager).Count == 0, $"order reshop failed response {orderReshopResult}  and request {orderReshopRequest}");
                reshopOfferRefId = orderReshopResponseXmlDocument.SelectSingleNode("//ct:ReshopResults//ct:ReshopOffers//ct:Offer[1]//ct:OfferID", xmlNamespaceManager).InnerText;
                reshopTotalAmount = FindAndReturnDataHelper.ReturnReshopOfferrTotalAmount(orderReshopResponseXmlDocument.SelectSingleNode("//ct:ReshopResults//ct:ReshopOffers//ct:Offer[1]", xmlNamespaceManager));
            }

            var orderQuoteRequest = RequestHelper.GenerateOrderQuoteRequest(airShoppingRequest, orderReshopResult, reshopOfferRefId, orderId);

            result.Add("orderQuoteRQ", orderQuoteRequest);

            if (apiResult.Equals(ApiResult.OrderQuoteRQ))
            {
                return result;
            }

            //Post Order Quote request
            var orderQuoteResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteRequest).EnsureIsSuccessStatusCode();

            //Getting the Order Quote response and converting to XML Document
            string orderQuoteResult = orderQuoteResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("orderQuoteRS", orderQuoteResult);

            if (apiResult.Equals(ApiResult.OrderQuoteRS))
            {
                return result;
            }

            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            Assert.False(orderQuoteResult.Has("Internal error"), $"order quote failed {orderQuoteResult}");

            var orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderId, orderChangePayment: orderChangePayment);

            var updatedOrderChangeRequest = new XmlDocument();

            updatedOrderChangeRequest.LoadXml(orderChangeRequest);

            if (exchangeType.Is("Refund"))
            {
                var nodeToRemove2 = updatedOrderChangeRequest.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);
                if (nodeToRemove2?.ParentNode != null)
                {
                    nodeToRemove2.ParentNode.RemoveChild(nodeToRemove2);
                }
            }

            var nodeToRemove1 = updatedOrderChangeRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList", xmlNamespaceManager);

            nodeToRemove1.ParentNode.RemoveChild(nodeToRemove1);

            result.Add("orderChangeRQ", updatedOrderChangeRequest.OuterXml);

            if (apiResult.Equals(ApiResult.OrderChangeRQ))
            {
                return result;
            }

            XmlDocument updatedOrderChangeResponse = new XmlDocument();

            var orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml).EnsureIsSuccessStatusCode();

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            result.Add("OrderChangeResult", orderChangeResult);

            return result;
        }

        public static Dictionary<string, string> AddSeatToPNR(string airShoppingRequest, string orderCreateResult, int serviceCount = 0, SeatPriceType seatType = SeatPriceType.None, string flightType = "", string corporate = "", string passport = "", string greencard = "", string visa = "", string knowntravller = "", string redress = "", string reshopFlightType = "", string agentType = "Direct", string participatingOrgOrgID2 = "", ApiResult apiResult = ApiResult.None, string flow = "", string orderChangePayment = "")
        {
            var updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            //Post Air Shopping request
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            var result = new Dictionary<string, string>();

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderCreateResult);

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;

            var bookingID = orderRetrieveResponseXmlDocument.GetElementsByTagName("BookingID");

            var currencyCode = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:TotalPrice//ct:TotalAmount//@CurCode", xmlNamespaceManager).InnerText;

            var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderId);

            //Running XSD validation against the request for SeatAvailability
            Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(Environment.NewLine, seatAvailabilityRequestExceptions));

            var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

            string seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("postSaleSeatAvailabilityRQ", seatAvailabilityRequest);

            if (apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRQ))
            {
                return result;
            }

            result.Add("postSaleSeatAvailabilityRS", seatAvailabilityResult);

            if (apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRS))
            {
                return result;
            }

            var orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: currencyCode, totalAmount: "0.00", bookingId: FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderId: orderId, seat: seatType, paymentMethod: orderChangePayment);

            result.Add("orderChangeSeatRQ", orderChangeSeatRequest);

            if (apiResult.Equals(ApiResult.OrderChangeSeatRQ))
            {
                return result;
            }

            XmlDocument updatedOrderChangeSeatRequest = new XmlDocument();

            updatedOrderChangeSeatRequest.LoadXml(orderChangeSeatRequest);

            var orderChangeSeatResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeSeatRequest.OuterXml).EnsureIsSuccessStatusCode();

            string orderChangeSeatResult = orderChangeSeatResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("orderChangeSeatRS", orderChangeSeatResult);

            if (apiResult.Equals(ApiResult.OrderChangeSeatRS))
            {
                return result;
            }

            return result;
        }

        public static Dictionary<string, string> CancelPNR(string airShoppingRequest, string orderCreateResult, ApiResult apiResult = ApiResult.None)
        {
            var updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            //Post Air Shopping request
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            var result = new Dictionary<string, string>();

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderCreateResult);

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;

            var orderReshopCancelRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), 1, journeyType: JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, flow: "cancel");

            result["orderReshopCancelRQ"] = orderReshopCancelRequest;

            if (apiResult.Equals(ApiResult.OrderReshopCancelRQ))
            {
                return result;
            }

            //Post Order Reshop request
            var orderReshopCancelResponse= NDCTransformationEndpoints.OrderReshop(orderReshopCancelRequest).EnsureIsSuccessStatusCode();

            string orderReshopCancelResult = orderReshopCancelResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();


            XmlDocument orderReshoCancelResponseXmlDocument = new XmlDocument();
            orderReshoCancelResponseXmlDocument.LoadXml(orderReshopCancelResult);

            result["orderReshopCancelRS"] = orderReshopCancelResult;

            if (apiResult.Equals(ApiResult.OrderReshopCancelRS))
            {
                return result;
            }

            var orderChangeCancelRQ = RequestHelper.GenerateOrderChangeCancelRequest(airShoppingRequest, orderId, orderReshopCancelResult);

            result["OrderChangeCancelRQ"] = orderChangeCancelRQ;

            if (apiResult.Equals(ApiResult.OrderChangeCancelRQ))
            {
                return result;
            }
            //Post Order Reshop request
            var orderChangeCancelRS = NDCTransformationEndpoints.OrderChange(orderChangeCancelRQ).EnsureIsSuccessStatusCode();

            var orderChangeCancelResult = orderChangeCancelRS.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result["OrderChangeCancelRS"] = orderChangeCancelResult;

            if (apiResult.Equals(ApiResult.OrderChangeCancelRS))
            {
                return result;
            }

            return result;
        }

        public static Dictionary<string, string> OrderChangeHeldBooking(string airShoppingRequest, string orderCreateResult, ApiResult apiResult = ApiResult.None, string orderChangePayment = "CPDVisa",string accountNumber="")
        {
            var updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            //Post Air Shopping request
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            var result = new Dictionary<string, string>();

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderCreateResult);

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;

            var orderQuoteHoldBookingRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

            result.Add("orderQuoteHoldBookingRQ", orderQuoteHoldBookingRequest);

            if (apiResult.Equals(ApiResult.OrderQuoteHoldBookingRQ))
            {
                return result;
            }

            var orderQuoteHoldBookingResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteHoldBookingRequest).EnsureSuccessStatusCode();
            string orderQuoteHoldBookingResult = orderQuoteHoldBookingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (apiResult.Equals(ApiResult.OrderQuoteHoldBookingRS))
            {
                result.Add("orderQuoteHoldBookingRS", orderQuoteHoldBookingResult);
                return result;
            }

            var amount = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:Order//ct:TotalPrice//ct:TotalAmount", xmlNamespaceManager).InnerText;
            
            var bookingID = orderRetrieveResponseXmlDocument.GetElementsByTagName("BookingID");

            var currencyCode = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:TotalPrice//ct:TotalAmount//@CurCode", xmlNamespaceManager).InnerText;

            var countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:Order//ct:OrderItem[ct:FareDetail]", xmlNamespaceManager).Count;

            var orderChangeHoldBookingRequest = GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteHoldBookingResult, currencyCode, amount, bookingID[0].InnerText, orderId, paymentType: orderChangePayment, accountNumber: accountNumber);

            var updatedOrderChangeConfirmHeldBookingRequest = new XmlDocument();
            updatedOrderChangeConfirmHeldBookingRequest.LoadXml(orderChangeHoldBookingRequest);

            XmlNode nodeRemove = updatedOrderChangeConfirmHeldBookingRequest.SelectSingleNode("//ct:ChangeOrderChoice", xmlNamespaceManager);
            nodeRemove.ParentNode.RemoveChild(nodeRemove);

            XmlNode nodeRemove2 = updatedOrderChangeConfirmHeldBookingRequest.SelectSingleNode("//ct:ContactInfoList", xmlNamespaceManager);
            nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

            if (apiResult.Equals(ApiResult.OrderChangeConifrmHeldBookingRQ))
            {
                result.Add("OrderChangeConifrmHeldBookingRQ", updatedOrderChangeConfirmHeldBookingRequest.OuterXml);
                return result;
            }

            var orderChangeHoldBookingResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeConfirmHeldBookingRequest.OuterXml).EnsureIsSuccessStatusCode();

            string orderChangeHoldBookingResult = orderChangeHoldBookingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("OrderChangeConifrmHeldBookingRS", orderChangeHoldBookingResult);

            result.Add("orderQuoteHoldBookingRS", orderQuoteHoldBookingResult);

            result.Add("OrderChangeConifrmHeldBookingRQ", updatedOrderChangeConfirmHeldBookingRequest.OuterXml);

            return result;
        }

        public static Dictionary<string, string> UpdatePaxName(string airShoppingRequest, string orderCreateResult,string paxRefID, ApiResult apiResult = ApiResult.None)
        {
            var result = new Dictionary<string, string>();

            var orderReshopUpdatePaxNameRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), journeyType:JourneyType.OneWay, locationOne: LocationType.LHR, locationTwo: LocationType.BLR, paxRefID: paxRefID, orderCreateRS: orderCreateResult);

            result["orderReshopUpdateNameRQ"] = orderReshopUpdatePaxNameRequest;

            if (apiResult.Equals(ApiResult.OrderReshopUpdatePaxNameRQ))
            {
                return result;
            }

            //Post Order Reshop request
            var orderReshopUpdatePaxNameResponse = NDCTransformationEndpoints.OrderReshop(orderReshopUpdatePaxNameRequest);

            //Getting the Order Reshop response and converting to XML Document
            var orderReshopUpdatePaxNameResult = orderReshopUpdatePaxNameResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderReshopUpdatePaxNameXmlDocument = new XmlDocument();
            orderReshopUpdatePaxNameXmlDocument.LoadXml(orderReshopUpdatePaxNameResult);

            result["orderReshopUpdateNameRS"] = orderReshopUpdatePaxNameResult;

            if (apiResult.Equals(ApiResult.OrderReshopUpdatePaxNameRS))
            {
                return result;
            }

            return result;
        }

    }
}
