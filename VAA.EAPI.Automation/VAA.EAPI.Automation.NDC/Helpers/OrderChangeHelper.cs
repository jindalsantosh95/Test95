using System.Globalization;
using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class OrderChangeHelper
    {

        public static Dictionary<string, string> ChangeOrder(int addDays = 100, int maximumDays = 200, string participatingOrgOrgID = "91281035", int numberOfadults = 1, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string cabinCode = "5", CabinType cabinName = CabinType.EconomyClassicFlex, FlightScenarioType flight = FlightScenarioType.Virgin, JourneyType journeyType = JourneyType.OneWay, LocationType locationOne = LocationType.LHR, LocationType locationTwo = LocationType.ATL, LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, int serviceCount = 0, SeatPriceType seatType = SeatPriceType.None, string flightType = "", string corporate = "", string bookingType = "InstantPurchase", string paymentType = "Tokenized", string passport = "", string greencard = "", string visa = "", string knowntravller = "", string redress = "", ChangeOrder changeOrder = RequestHelper.ChangeOrder.None, string reshopFlightType = "", string agentType = "Direct", string participatingOrgOrgID2 = "", ApiResult apiResult = ApiResult.None, string exchangeType = "EvenExchange", string flow = "", string orderChangePayment = null, FlightType operatorName = FlightType.VS,string agentEmailId= "Bandaru.JeevanNagaSai@fly.virgin.com",string accountNumber="")
        {
            var deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maximumDays);

            //Generate Air Shopping request
            var airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
            journeyType,
            locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
            cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date, flighttype: flightType, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2, corporate: corporate);
            var updatedAirShoppingRequest = new XmlDocument();

            updatedAirShoppingRequest.LoadXml(airShoppingRequest);

            XmlNamespaceManager xmlNamespaceManager = updatedAirShoppingRequest.CreateXmlNamespaceManager();

            //Post Air Shopping request
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

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
            //Find offer based on Cabin Type and Flight Scenraio
            var offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, cabinName, flight, operatorName);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            var offerRefId = offerDetails["OfferRefID"];
            var totalAmount = offerDetails["TotalAmount"];
            var currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            var offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            var offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", xmlNamespaceManager).Count > 0, "No Priced offer returned in Offer Price response");

            var offerID = offerPriceResponseXmlDocument.SelectSingleNode("//ct:PricedOffer//ct:OfferID", xmlNamespaceManager).InnerText;

            string orderCreateRequest = "";
            string serviceListResult = "";
            string seatAvailabilityResult = "";

            var updatedOrderCreateRequest = new XmlDocument();

            var result = new Dictionary<string, string>();

            if (bookingType.Equals("InstantPurchase") && !seatType.Equals(SeatPriceType.None) && serviceCount == 0 && (apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRS) || apiResult.Equals(ApiResult.OrderChangeSeatRS) || apiResult.Equals(ApiResult.OrderChangeSeatRQ)))
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress,agentMailID:agentEmailId, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with SSR
            if (serviceCount > 0 && seatType == SeatPriceType.None)
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(serviceListRequest).EnsureSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);

                result.Add("serviceListRQ", serviceListRequest);

                result.Add("serviceListRS", serviceListResult);

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, agentMailID: agentEmailId, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with seat
            if (seatType != SeatPriceType.None && serviceCount == 0 && !apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRS) && !apiResult.Equals(ApiResult.OrderChangeSeatRQ) && !apiResult.Equals(ApiResult.OrderChangeSeatRS))
            {
                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(Environment.NewLine, seatAvailabilityRequestExceptions));

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(Environment.NewLine, seatAvailabilityResultExceptions));

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                result.Add("seatAvailabilityRS", seatAvailabilityResult);

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, agentMailID: agentEmailId, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with SSR and seat
            if (seatType != SeatPriceType.None && serviceCount > 0)
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);
                var updatedSerivceListRequest = new XmlDocument();
                updatedSerivceListRequest.LoadXml(serviceListRequest);

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml);

                Assert.True(serviceListResponse.IsSuccessStatusCode, $"Success status code was not returned from ServiceList Response, returned: {serviceListResponse.StatusCode} with message: {serviceListResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {serviceListRequest}");

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);

                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(Environment.NewLine, seatAvailabilityRequestExceptions));

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest);

                Assert.True(seatAvailabilityResponse.IsSuccessStatusCode,
                    $"Success status code was not returned for Seat Availability. Status code returned was {seatAvailabilityResponse.StatusCode} and the response was {seatAvailabilityResponse.ReadResponseAsString()} \r\n " +
                    $"Seat Availability Request sent was {seatAvailabilityResponse.ReadResponseAsString()}");

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Only Order Create 
            if (bookingType.Equals("InstantPurchase") && seatType.Equals(SeatPriceType.None) && serviceCount == 0)
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, agentMailID: agentEmailId, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order Create Hold booking
            if (bookingType.Equals("HoldBooking"))
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, agentMailID: agentEmailId, accountNumber: accountNumber);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);

                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            }

            var orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, bookingType: bookingType, paymentType: paymentType, seatType: seatType.ToString()).EnsureIsSuccessStatusCode();

            var updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            result.Add("pnr", FindAndReturnDataHelper.ReturnBookingId(orderCreateResult));

            result.Add("airShoppingRQ", airShoppingRequest);

            result.Add("airShoppingRS", airShoppingResult);

            result.Add("offerPriceRQ", offerPriceRequest);

            result.Add("offerPriceRS", offerPriceResult);

            result.Add("orderCreateResult", orderCreateResult);

            result.Add("orderCreateRQ", updatedOrderCreateRequest.OuterXml);

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            var bookingID = orderCreateResponseXmlDocument.GetElementsByTagName("BookingID");

            var warning = orderCreateResponseXmlDocument.GetElementsByTagName("Warning");

            if (!seatType.Equals(SeatPriceType.None) && !apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRS) && !apiResult.Equals(ApiResult.OrderChangeSeatRQ) && !apiResult.Equals(ApiResult.OrderChangeSeatRS))
            {
                Assert.True(warning.Count == 0, $"seats are not booked");
            }

            var orderRetrieveRequest = RequestHelper.GenerateOrderRetrieveRequest(airShoppingRequest, orderCreateResult);

            if (apiResult.Equals(ApiResult.OrderRetrieveRQ))
            {
                result.Add("orderRetrieveRQ", orderRetrieveRequest);

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

            string orderId = orderRetrieveResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;

            if (apiResult.Equals(ApiResult.OrderQuoteHoldBookingRQ) || apiResult.Equals(ApiResult.OrderQuoteHoldBookingRS) || apiResult.Equals(ApiResult.OrderChangeConifrmHeldBookingRQ) || apiResult.Equals(ApiResult.OrderChangeConifrmHeldBookingRS))
            {
                var orderQuoteHoldBookingRequest = RequestHelper.GenerateOrderQuoteRequestHoldBooking(airShoppingRequest, airShoppingResult, orderId);

                result.Add("orderQuoteHoldBookingRQ", orderQuoteHoldBookingRequest);

                if (apiResult.Equals(ApiResult.OrderQuoteHoldBookingRQ))
                {
                    return result;
                }

                //Post Order Quote request
                var orderQuoteHoldBookingResponse = NDCTransformationEndpoints.OrderQuote(orderQuoteHoldBookingRequest).EnsureSuccessStatusCode();

                //Getting the Order Quote response and converting to XML Document
                string orderQuoteHoldBookingResult = orderQuoteHoldBookingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (apiResult.Equals(ApiResult.OrderQuoteHoldBookingRS))
                {
                    result.Add("orderQuoteHoldBookingRS", orderQuoteHoldBookingResult);
                    return result;
                }

                string amount = orderCreateResponseXmlDocument.SelectSingleNode("//ct:Order//ct:TotalPrice//ct:TotalAmount", xmlNamespaceManager).InnerText;

                //Generating the Order Change Request
                var orderChangeHoldBookingRequest = GenerateOrderChangeHoldBookingRequest(airShoppingRequest, orderQuoteHoldBookingResult, currencyCode, amount, bookingID[0].InnerText, orderId, paymentType: paymentType);

                var updatedOrderChangeConfirmHeldBookingRequest = new XmlDocument();
                updatedOrderChangeConfirmHeldBookingRequest.LoadXml(orderChangeHoldBookingRequest);

                XmlNode nodeRemove = updatedOrderChangeConfirmHeldBookingRequest.SelectSingleNode("//ct:ChangeOrderChoice", xmlNamespaceManager);
                nodeRemove.ParentNode.RemoveChild(nodeRemove);

                XmlNode nodeRemove2 = updatedOrderChangeConfirmHeldBookingRequest.SelectSingleNode("//ct:ContactInfoList", xmlNamespaceManager);
                nodeRemove2.ParentNode.RemoveChild(nodeRemove2);

                if (apiResult.Equals(ApiResult.OrderChangeConifrmHeldBookingRQ))
                {
                    result.Add("OrderChangeConifrmHeldBookingRQ", orderQuoteHoldBookingResult);
                    return result;
                }

                var orderChangeHoldBookingResponse = NDCTransformationEndpoints.OrderChange(updatedOrderChangeConfirmHeldBookingRequest.OuterXml).EnsureIsSuccessStatusCode();

                //Getting the Order Quote response and converting to XML Document
                string orderChangeHoldBookingResult = orderChangeHoldBookingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (apiResult.Equals(ApiResult.OrderChangeConifrmHeldBookingRS))
                {
                    result.Add("OrderChangeConifrmHeldBookingRS", orderChangeHoldBookingResult);
                    return result;
                }

                return result;
            }

            if (apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRS) || apiResult.Equals(ApiResult.PostSaleSeatAvailabilityRQ) || apiResult.Equals(ApiResult.OrderChangeSeatRQ) || apiResult.Equals(ApiResult.OrderChangeSeatRS))
            {
                var seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, orderId: orderId);

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(Environment.NewLine, seatAvailabilityRequestExceptions));

                var seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(Environment.NewLine, seatAvailabilityResultExceptions));

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

                var orderChangeSeatRequest = RequestHelper.GenerateOrderChangeSeatRequest(airShoppingRequest, seatAvailabilityResult, currencyCode: currencyCode, totalAmount: "0.00", bookingId: FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderId: orderId, seat: seatType, paymentMethod: paymentType);

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
            }

            //Post sale service list
            if (apiResult.Equals(ApiResult.PostSaleServiceListRQ) || apiResult.Equals(ApiResult.PostSaleServiceListRS))
            {
                var serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, orderID: orderId);

                result.Add("postSaleServiceListRQ", serviceListRequest);

                if (apiResult.Equals(ApiResult.PostSaleServiceListRQ))
                {
                    return result;
                }

                var serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(serviceListRequest).EnsureSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);

                result.Add("postSaleServiceListRS", serviceListResult);

                if (apiResult.Equals(ApiResult.PostSaleServiceListRS))
                {
                    return result;
                }
            }


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

            var countOrderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:Order//ct:OrderItem[ct:FareDetail]", xmlNamespaceManager).Count;

            int counter = 5;
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

                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        journeyType, locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix, newCabinTypeCodeOne: cabinCode, newCabinTypeCodeTwo: cabinCode, deptDate: reshopJourney1Date, dateTwo: reshopJourney2Date, datethree: reshopJourney3Date, flighttype: reshopFlightType);

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

                } while (counter < 10);
            }

            if (changeOrder.Equals(RequestHelper.ChangeOrder.OutBoundJourney))
            {
                counter = 5;
                do
                {
                    DateTime date = DateTime.ParseExact(originalJourney1Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateTime newDate = date.AddDays(counter);

                    string reshopJourney1Date = newDate.ToString("yyyy-MM-dd");
                    //Generating the Order Reshop Request
                    orderReshopRequest = RequestHelper.GenerateOrderReshopRequest(airShoppingRequest, airShoppingResult, orderId, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), countOrderItemID,
                        JourneyType.OneWay, locationOne: locationOne, locationTwo: locationTwo, newCabinTypeCodeOne: cabinCode, deptDate: reshopJourney1Date);

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
                    counter--;
                    if (reshopOfferDetails.Count > 0)
                    {
                        break;
                    }

                } while (counter > 0);

            }

            if (changeOrder.Equals(RequestHelper.ChangeOrder.InBoundJourney))
            {
                counter = 1;
                do
                {
                    DateTime date = DateTime.ParseExact(originalJourney2Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
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

            var orderChangeRequest = RequestHelper.GenerateOrderChangeRequest(airShoppingRequest, orderQuoteResult, reshopOfferRefId, currencyCode, reshopTotalAmount, FindAndReturnDataHelper.ReturnBookingId(orderCreateResult), orderId, orderChangePayment: orderChangePayment, accountNumber: accountNumber);

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

    }
}
