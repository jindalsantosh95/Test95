using System.Globalization;
using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class ShoppingHelper
    {
        public static Dictionary<string, string> ShoppingRequest(int addDays = 100, int maximumDays = 200, string participatingOrgOrgID = AgentID.ParticipatingOrgIdTravelRepublic,
            int numberOfadults = 1, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string cabinCode = "5", CabinType cabinName = CabinType.EconomyClassicFlex,
            FlightScenarioType flight = FlightScenarioType.Virgin, JourneyType journeyType = JourneyType.OneWay, LocationType locationOne = LocationType.LHR, LocationType locationTwo = LocationType.ATL,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
            int serviceCount = 0, SeatPriceType seatType = SeatPriceType.None, string flightType = "", string corporate = "", string offerRequestType = "PrivateFare", ApiResult apiResult = ApiResult.None, string bookingType = "InstantPurchase",
            string paymentType = "CreditCard", string accountNumber = "", string agentType = "Direct", string participatingOrgOrgID2 = "", FlightType operatorName = FlightType.VS, string taxonomy = "", string fareTypeCode = "", FareSubTypeCode fareSubType = FareSubTypeCode.None, ChangeOrder changeOrder = RequestHelper.ChangeOrder.None, string reshopFlightType = "", string exchangeType = "EvenExchange", string orderChangePayment = "Cash")
        {
            var result = new Dictionary<string, string>();

            var deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");
            string journeyDate = GenerateDataHelper.RandomDate(deptDateTime, maximumDays);
            var updatedAirShoppingRequest = new XmlDocument();

            // Local helper function to generate the AirshoppingRequest XML string with overrides
            string BuildRequest(
                string? customCorporate = null,
                string? customAgentType = null,
                string? customOrgID2 = null,
                string? customTaxonomy = null,
                string? customFareTypeCode = null,
                FareSubTypeCode? customFareSubType = null)
            {
                return GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode,
                    numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journeyDate, flighttype: flightType, fareTypeCode: customFareTypeCode ?? fareTypeCode, subTypeCode: customFareSubType ?? fareSubType, taxonomy: customTaxonomy ?? taxonomy, corporate: customCorporate ?? corporate, agentType: customAgentType ?? agentType, participatingOrgOrgID2: customOrgID2 ?? participatingOrgOrgID2);
            }

            // Combine overrides based on inputs
            string? finalCorporate = null;
            string? finalAgentType = null;
            string? finalOrgID2 = null;
            string? finalTaxonomy = null;
            string? finalFareTypeCode = null;
            FareSubTypeCode? finalFareSubType = null;

            if (!string.IsNullOrWhiteSpace(corporate))
                finalCorporate = corporate;

            if (agentType.Is("InDirect"))
            {
                finalAgentType = "InDirect";
                finalOrgID2 = participatingOrgOrgID2;
            }

            if (offerRequestType.Is("PrivateFare"))
            {
                finalTaxonomy = taxonomy;
                finalFareTypeCode = fareTypeCode;
                finalFareSubType = fareSubType;
            }

            // Build the request with all combined overrides
            var airShoppingRequest = BuildRequest(customCorporate: finalCorporate, customAgentType: finalAgentType, customOrgID2: finalOrgID2,
                customTaxonomy: finalTaxonomy, customFareTypeCode: finalFareTypeCode, customFareSubType: finalFareSubType);

            // Load XML and remove <ServiceCriteria> node if exists
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);
            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();
            var nodeToRemove = updatedAirShoppingRequest.SelectSingleNode("//ct:ServiceCriteria", xmlNamespaceManager);
            nodeToRemove?.ParentNode?.RemoveChild(nodeToRemove);

            result.Add("airShoppingRQ", updatedAirShoppingRequest.OuterXml);

            // Post the Air Shopping request and get response
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml)
                .EnsureIsSuccessStatusCode();

            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            result.Add("airShoppingResult", airShoppingResult);

            var originalJourneyDate = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;

            string originalJourneyReturnDate = "";

            if (journeyType.Equals(JourneyType.Return) || journeyType.Equals(JourneyType.MultiCity))
            {
                originalJourneyReturnDate = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[2]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;
            }

            string originalJourneyMulticityDate = "";

            if (journeyType.Equals(JourneyType.MultiCity))
            {
                originalJourneyMulticityDate = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[3]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;
            }
            var offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, cabinName, flight, operatorName);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            var offerRefId = offerDetails["OfferRefID"];
            var totalAmount = offerDetails["TotalAmount"];
            var currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            var offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            result.Add("offerPriceRQ", offerPriceRequest);

            //Post Offer Price request
            var offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            result.Add("offerPriceResult", offerPriceResult);

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", xmlNamespaceManager).Count > 0, "No Priced offer returned in Offer Price response");

            var offerID = offerPriceResponseXmlDocument.SelectSingleNode("//ct:PricedOffer//ct:OfferID", xmlNamespaceManager).InnerText;

            string orderCreateRequest = "";
            string serviceListResult = "";
            string seatAvailabilityResult = "";

            var updatedOrderCreateRequest = new XmlDocument();

            //Order create with SSR
            if (serviceCount > 0 && seatType == SeatPriceType.None)
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);

                result.Add("serviceListRQ", serviceListRequest);

                //Return ServiceListRequest 
                if (apiResult.Equals(ApiResult.ServiceListRQ))
                {
                    return result;
                }

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(serviceListRequest).EnsureIsSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("serviceListResult", serviceListResult);

                if (apiResult.Equals(ApiResult.ServiceListRS))
                {
                    return result;
                }

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, paymentMethod: paymentType, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with seat
            if (seatType != SeatPriceType.None && serviceCount == 0)
            {

                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                if (apiResult.Equals(ApiResult.SeatAvailabilityRQ))
                {
                    return result;
                }

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("seatAvailabilityResult", seatAvailabilityResult);

                if (apiResult.Equals(ApiResult.SeatAvailabilityRS))
                {
                    return result;
                }

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with SSR and seat
            if (seatType != SeatPriceType.None && serviceCount > 0)
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);
                var updatedSerivceListRequest = new XmlDocument();
                updatedSerivceListRequest.LoadXml(serviceListRequest);

                result.Add("serviceListRQ", serviceListRequest);

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml).EnsureIsSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("serviceListResult", serviceListResult);

                var updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);
                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("seatAvailabilityResult", seatAvailabilityResult);

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Only Order Create 
            if (bookingType.Equals("InstantPurchase") && seatType == SeatPriceType.None && serviceCount == 0)
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order Create Hold booking
            if (bookingType.Equals("HoldBooking"))
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlNode nodeNextRemove = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);

                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            }

            result.Add("orderCreateRQ", updatedOrderCreateRequest.OuterXml);

            if (apiResult.Equals(ApiResult.OrderCreateRQ))
            {
                return result;
            }

            var orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, bookingType: bookingType, paymentType: paymentType, seatType: seatType.ToString()).EnsureIsSuccessStatusCode();

            var updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            var warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            if (!seatType.Equals(SeatPriceType.None))
            {
                Assert.True(OrderCreateAssertions.ValidateSeatBooking(orderCreateResult, out string errorMessage), errorMessage);
            }

            result.Add("pnr", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

            result.Add("orderCreateResult", orderCreateResult);

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            var bookingID = orderCreateResponseXmlDocument.GetElementsByTagName("BookingID");

            var orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            result.Add("orderRetrieveRQ", orderRetrieveRequest);

            if (apiResult.Equals(ApiResult.OrderRetrieveRQ))
            {
                return result;
            }

            HttpResponseMessage orderRetrieveResponse = null;

            RetryHelper.RunWithRetry(() =>
            {
                orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();
                Assert.True(true);
            }, 5);

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("orderRetrieveRS", orderRetrieveResult);

            if (apiResult.Equals(ApiResult.OrderRetrieveRS))
            {
                return result;
            }

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            string orderId = orderCreateResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;


            if (apiResult.Equals(ApiResult.OrderReshopCancelRQ) || apiResult.Equals(ApiResult.OrderReshopCancelRS))
            {

                var orderReshopCancelRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), 1, journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, flow: "cancel");

                result["orderReshopCancelRQ"] = orderReshopCancelRequest;

                if (apiResult.Equals(ApiResult.OrderReshopCancelRQ))
                {
                    return result;
                }

                //Post Order Reshop request
                var orderReshopCancelResponse = NDCTransformationEndpoints.OrderReshop(orderReshopCancelRequest);

                //Getting the Order Reshop response and converting to XML Document
                var orderReshopCancelResult = orderReshopCancelResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderReshoCancelResponseXmlDocument = new XmlDocument();
                orderReshoCancelResponseXmlDocument.LoadXml(orderReshopCancelResult);

                result["orderReshopCancelRS"] = orderReshopCancelResult;

                if (apiResult.Equals(ApiResult.OrderReshopCancelRS))
                {
                    return result;
                }

            }

            var countOrderItemID = orderCreateResponseXmlDocument.SelectNodes("//ct:Order//ct:OrderItem[ct:FareDetail]", xmlNamespaceManager).Count;

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
                    DateTime date = DateTime.ParseExact(journeyDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter);

                    string reshopJourneyDate = newDate.ToString("yyyy-MM-dd");

                    string reshopJourneyReturnDate = "";

                    string reshopJourneyMultiCityDate = "";

                    if (journeyType.Equals(JourneyType.Return) || journeyType.Equals(JourneyType.MultiCity))
                    {
                        DateTime returnDate = DateTime.ParseExact(originalJourneyReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime newreturnDate = returnDate.AddDays(counter);
                        reshopJourneyReturnDate = newreturnDate.ToString("yyyy-MM-dd");
                    }

                    if (journeyType.Equals(JourneyType.MultiCity))
                    {
                        DateTime multiCityDate = DateTime.ParseExact(originalJourneyMulticityDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime newmultiCityDate = multiCityDate.AddDays(counter);
                        reshopJourneyMultiCityDate = newmultiCityDate.ToString("yyyy-MM-dd");
                    }

                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourneyDate, dateTwo: reshopJourneyReturnDate, datethree: reshopJourneyMultiCityDate, flighttype: "reshopFlightType");

                    result["orderReshopRQ"] = orderReshopRequest;

                    if (apiResult.Equals(ApiResult.OrderReshopRQ))
                    {
                        return result;
                    }

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
                    DateTime date = DateTime.ParseExact(originalJourneyReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter++);

                    string reshopJourneyDate = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        JourneyType.OneWay, locationOne: locationOne, locationTwo: locationTwo, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourneyDate);

                    var updatedOrderReshopRequest = new XmlDocument();

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Get the count of segemnts for retaining segment

                    retainJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[2]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int reshopJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[1]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

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
                    DateTime date = DateTime.ParseExact(originalJourneyReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter++);

                    string reshopJourney2Date = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        JourneyType.OneWay, locationOne: locationThree, locationTwo: locationFour, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourney2Date);

                    var updatedOrderReshopRequest = new XmlDocument();

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Get the count of segemnts for retaining segment

                    retainJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[1]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int reshopJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[2]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

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

            var orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            result.Add("OrderChangeResult", orderChangeResult);

            return result;
        }
    }

    public static class TTLShoppingHelper
    {
        public static Dictionary<string, string> TTLShoppingRequest(int addDays = 100, int maximumDays = 200, string participatingOrgOrgID = AgentID.ParticipatingOrgIdTravelRepublic,
            int numberOfadults = 1, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string cabinCode = "5", CabinType cabinName = CabinType.EconomyClassicFlex,
            FlightScenarioType flight = FlightScenarioType.Virgin, JourneyType journeyType = JourneyType.OneWay, LocationType locationOne = LocationType.LHR, LocationType locationTwo = LocationType.ATL,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None,
            int serviceCount = 0, SeatPriceType seatType = SeatPriceType.None, string flightType = "", string corporate = "", string offerRequestType = "PrivateFare", ApiResult apiResult = ApiResult.None, string bookingType = "InstantPurchase",
            string paymentType = "CreditCard", string accountNumber = "", string agentType = "Direct", string participatingOrgOrgID2 = "", FlightType operatorName = FlightType.VS, string taxonomy = "", string fareTypeCode = "", FareSubTypeCode fareSubType = FareSubTypeCode.None, ChangeOrder changeOrder = RequestHelper.ChangeOrder.None, string reshopFlightType = "", string exchangeType = "EvenExchange", string orderChangePayment = "Cash", int timerange = 0)
        {
            var result = new Dictionary<string, string>();
            var deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maximumDays);

            var updatedAirShoppingRequest = new XmlDocument();

            // Local helper function to generate the AirshoppingRequest XML string with overrides
            string BuildRequest(
                string? customCorporate = null,
                string? customAgentType = null,
                string? customOrgID2 = null,
                string? customTaxonomy = null,
                string? customFareTypeCode = null,
                FareSubTypeCode? customFareSubType = null)
            {
                return GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID, journeyType: journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode,
                    numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: deptDateTime, flighttype: flightType, fareTypeCode: customFareTypeCode ?? fareTypeCode, subTypeCode: customFareSubType ?? fareSubType, taxonomy: customTaxonomy ?? taxonomy, corporate: customCorporate ?? corporate, agentType: customAgentType ?? agentType, participatingOrgOrgID2: customOrgID2 ?? participatingOrgOrgID2);
            }

            // Combine overrides based on inputs
            string? finalCorporate = null;
            string? finalAgentType = null;
            string? finalOrgID2 = null;
            string? finalTaxonomy = null;
            string? finalFareTypeCode = null;
            FareSubTypeCode? finalFareSubType = null;

            if (!string.IsNullOrWhiteSpace(corporate))
                finalCorporate = corporate;

            if (agentType.Is("InDirect"))
            {
                finalAgentType = "InDirect";
                finalOrgID2 = participatingOrgOrgID2;
            }

            if (offerRequestType.Is("PrivateFare"))
            {
                finalTaxonomy = taxonomy;
                finalFareTypeCode = fareTypeCode;
                finalFareSubType = fareSubType;
            }

            // Build the request with all combined overrides
            var airShoppingRequest = BuildRequest(customCorporate: finalCorporate, customAgentType: finalAgentType, customOrgID2: finalOrgID2,
                customTaxonomy: finalTaxonomy, customFareTypeCode: finalFareTypeCode, customFareSubType: finalFareSubType);

            // Load XML and remove <ServiceCriteria> node if exists
            updatedAirShoppingRequest.LoadXml(airShoppingRequest);
            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();
            var nodeToRemove = updatedAirShoppingRequest.SelectSingleNode("//ct:ServiceCriteria", xmlNamespaceManager);
            nodeToRemove?.ParentNode?.RemoveChild(nodeToRemove);

            result.Add("airShoppingRQ", updatedAirShoppingRequest.OuterXml);

            // Post the Air Shopping request and get response
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(updatedAirShoppingRequest.OuterXml)
                .EnsureIsSuccessStatusCode();

            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            result.Add("airShoppingResult", airShoppingResult);

            var originalJourneyDate = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;

            string originalJourneyReturnDate = "";

            if (journeyType.Equals(JourneyType.Return) || journeyType.Equals(JourneyType.MultiCity))
            {
                originalJourneyReturnDate = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[2]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;
            }

            string originalJourneyMulticityDate = "";

            if (journeyType.Equals(JourneyType.MultiCity))
            {
                originalJourneyMulticityDate = updatedAirShoppingRequest.SelectSingleNode("//ct:OriginDestCriteria[3]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;
            }

            TimeSpan fromTime = DateTime.Now.TimeOfDay;
            TimeSpan toTime = DateTime.Now.AddHours(timerange).TimeOfDay;

            var carrierName = operatorName switch { FlightType.AF => "Air France", FlightType.KL => "Royal Dutch Airlines", _ => "Virgin Atlantic Airways" };

            var offerDetails = FindAndReturnDataHelper.FindTTLOffersssss(airShoppingResult, deptDateTime, fromTime, toTime, carrierName);
            
            Assert.True(offerDetails.Any(), "OfferDetails did not return any Offers");

            var offer = offerDetails.First();

            var offerRefId = offer["OfferRefID"];
            var totalAmount = offer["TotalAmount"];
            var currencyCode = offer["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            var offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            result.Add("offerPriceRQ", offerPriceRequest);

            //Post Offer Price request
            var offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            result.Add("offerPriceResult", offerPriceResult);

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", xmlNamespaceManager).Count > 0, "No Priced offer returned in Offer Price response");

            var offerID = offerPriceResponseXmlDocument.SelectSingleNode("//ct:PricedOffer//ct:OfferID", xmlNamespaceManager).InnerText;

            string orderCreateRequest = "";
            string serviceListResult = "";
            string seatAvailabilityResult = "";

            var updatedOrderCreateRequest = new XmlDocument();

            //Order create with SSR
            if (serviceCount > 0 && seatType == SeatPriceType.None)
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);

                result.Add("serviceListRQ", serviceListRequest);

                //Return ServiceListRequest 
                if (apiResult.Equals(ApiResult.ServiceListRQ))
                {
                    return result;
                }

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(serviceListRequest).EnsureIsSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("serviceListResult", serviceListResult);

                if (apiResult.Equals(ApiResult.ServiceListRS))
                {
                    return result;
                }

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, paymentMethod: paymentType, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with seat
            if (seatType != SeatPriceType.None && serviceCount == 0)
            {

                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                if (apiResult.Equals(ApiResult.SeatAvailabilityRQ))
                {
                    return result;
                }

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("seatAvailabilityResult", seatAvailabilityResult);

                if (apiResult.Equals(ApiResult.SeatAvailabilityRS))
                {
                    return result;
                }

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with SSR and seat
            if (seatType != SeatPriceType.None && serviceCount > 0)
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);
                var updatedSerivceListRequest = new XmlDocument();
                updatedSerivceListRequest.LoadXml(serviceListRequest);

                result.Add("serviceListRQ", serviceListRequest);

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml).EnsureIsSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("serviceListResult", serviceListResult);

                var updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);
                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("seatAvailabilityResult", seatAvailabilityResult);

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Only Order Create 
            if (bookingType.Equals("InstantPurchase") && seatType == SeatPriceType.None && serviceCount == 0)
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order Create Hold booking
            if (bookingType.Equals("HoldBooking"))
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlNode nodeNextRemove = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);

                if (nodeNextRemove?.ParentNode != null)
                {
                    nodeNextRemove.ParentNode.RemoveChild(nodeNextRemove);
                }

            }

            result.Add("orderCreateRQ", updatedOrderCreateRequest.OuterXml);

            if (apiResult.Equals(ApiResult.OrderCreateRQ))
            {
                return result;
            }

            var orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, bookingType: bookingType, paymentType: paymentType, seatType: seatType.ToString()).EnsureIsSuccessStatusCode();

            var updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            var warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            if (!seatType.Equals(SeatPriceType.None))
            {
                Assert.True(OrderCreateAssertions.ValidateSeatBooking(orderCreateResult, out string errorMessage), errorMessage);
            }

            result.Add("pnr", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

            result.Add("orderCreateResult", orderCreateResult);

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            var bookingID = orderCreateResponseXmlDocument.GetElementsByTagName("BookingID");

            var orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            result.Add("orderRetrieveRQ", orderRetrieveRequest);

            if (apiResult.Equals(ApiResult.OrderRetrieveRQ))
            {
                return result;
            }

            HttpResponseMessage orderRetrieveResponse = null;

            RetryHelper.RunWithRetry(() =>
            {
                orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieveRequest(orderRetrieveRequest).EnsureSuccessStatusCode();
                Assert.True(true);
            }, 5);

            //Getting the Order retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("orderRetrieveRS", orderRetrieveResult);

            if (apiResult.Equals(ApiResult.OrderRetrieveRS))
            {
                return result;
            }

            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            string orderId = orderCreateResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;


            if (apiResult.Equals(ApiResult.OrderReshopCancelRQ) || apiResult.Equals(ApiResult.OrderReshopCancelRS))
            {

                var orderReshopCancelRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), 1, journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, flow: "cancel");

                result["orderReshopCancelRQ"] = orderReshopCancelRequest;

                if (apiResult.Equals(ApiResult.OrderReshopCancelRQ))
                {
                    return result;
                }

                //Post Order Reshop request
                var orderReshopCancelResponse = NDCTransformationEndpoints.OrderReshop(orderReshopCancelRequest);

                //Getting the Order Reshop response and converting to XML Document
                var orderReshopCancelResult = orderReshopCancelResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                XmlDocument orderReshoCancelResponseXmlDocument = new XmlDocument();
                orderReshoCancelResponseXmlDocument.LoadXml(orderReshopCancelResult);

                result["orderReshopCancelRS"] = orderReshopCancelResult;

                if (apiResult.Equals(ApiResult.OrderReshopCancelRS))
                {
                    return result;
                }

            }

            var countOrderItemID = orderCreateResponseXmlDocument.SelectNodes("//ct:Order//ct:OrderItem[ct:FareDetail]", xmlNamespaceManager).Count;

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
                    DateTime date = DateTime.ParseExact(journey1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter);

                    string reshopJourneyDate = newDate.ToString("yyyy-MM-dd");

                    string reshopJourneyReturnDate = "";

                    string reshopJourneyMultiCityDate = "";

                    if (journeyType.Equals(JourneyType.Return) || journeyType.Equals(JourneyType.MultiCity))
                    {
                        DateTime returnDate = DateTime.ParseExact(originalJourneyReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime newreturnDate = returnDate.AddDays(counter);
                        reshopJourneyReturnDate = newreturnDate.ToString("yyyy-MM-dd");
                    }

                    if (journeyType.Equals(JourneyType.MultiCity))
                    {
                        DateTime multiCityDate = DateTime.ParseExact(originalJourneyMulticityDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        DateTime newmultiCityDate = multiCityDate.AddDays(counter);
                        reshopJourneyMultiCityDate = newmultiCityDate.ToString("yyyy-MM-dd");
                    }

                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourneyDate, dateTwo: reshopJourneyReturnDate, datethree: reshopJourneyMultiCityDate, flighttype: reshopFlightType);

                    result["orderReshopRQ"] = orderReshopRequest;

                    if (apiResult.Equals(ApiResult.OrderReshopRQ))
                    {
                        return result;
                    }

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
                    DateTime date = DateTime.ParseExact(originalJourneyReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter++);

                    string reshopJourneyDate = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        JourneyType.OneWay, locationOne: locationOne, locationTwo: locationTwo, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourneyDate);

                    var updatedOrderReshopRequest = new XmlDocument();

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Get the count of segemnts for retaining segment

                    retainJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[2]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int reshopJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[1]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

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
                    DateTime date = DateTime.ParseExact(originalJourneyReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter++);

                    string reshopJourney2Date = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        JourneyType.OneWay, locationOne: locationThree, locationTwo: locationFour, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourney2Date);

                    var updatedOrderReshopRequest = new XmlDocument();

                    updatedOrderReshopRequest.LoadXml(orderReshopRequest);

                    //Get the count of segemnts for retaining segment

                    retainJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[1]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                    int reshopJourneySegmentCount = orderCreateResponseXmlDocument.SelectNodes("//ct:PaxJourney[2]//ct:PaxSegmentRefID", xmlNamespaceManager).Count;

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

            var orderChangeResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeRequest.OuterXml);

            Assert.True(orderChangeResponse.IsSuccessStatusCode, $"Success status code was not returned from orderChange Response, returned: {orderChangeResponse.StatusCode} with message: {orderChangeResponse.ReadResponseAsString()}" + $"OrderChange Request sent was {orderChangeRequest}");

            dynamic orderChangeResult = orderChangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderChangeResponse.LoadXml(orderChangeResult);

            result.Add("OrderChangeResult", orderChangeResult);

            return result;
        }

    }
}
