using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.NDC.Tests.OrderCreate;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class OrderCreateHelper
    {
        public static Dictionary<string, string> CreatePnr(int addDays = 100, int maximumDays = 200, string participatingOrgOrgID = AgentID.ParticipatingOrgIdTravelRepublic, int numberOfadults = 1, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string cabinCode = "5", CabinType cabinName = CabinType.EconomyClassicFlex, FlightScenarioType flight = FlightScenarioType.Virgin, JourneyType journeyType = JourneyType.OneWay, LocationType locationOne = LocationType.LHR, LocationType locationTwo = LocationType.ATL, LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, int serviceCount = 0, SeatPriceType seatType = SeatPriceType.None, string flightType = "", string corporate = "", string bookingType = "InstantPurchase", string paymentType = "Tokenized", string passport = "", string greencard = "", string visa = "", string knowntravller = "", string redress = "", ApiResult apiResult=ApiResult.None, string agentType = "Direct", string participatingOrgOrgID2 = "", FlightType operatorName = FlightType.VS,string accountNumber="",string agentMailId= "Bandaru.JeevanNagaSai@fly.virgin.com",string seatPaymentCardType = "CPDVisa", string requiredFlightNumber = "")
        {
            string deptDateTime = DateTime.UtcNow.AddDays(addDays).ToString("yyyy-MM-dd");

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maximumDays);

            Dictionary<string, string> result = new Dictionary<string, string>();

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
            journeyType,
            locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
            cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date, flighttype: flightType);

            if (agentType.Is("InDirect"))
            {
                airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                journeyType,
                locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
                cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date, flighttype: flightType, agentType: agentType, participatingOrgOrgID2: participatingOrgOrgID2);
            }

            if(!string.IsNullOrWhiteSpace(corporate))
            {
                airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: participatingOrgOrgID,
                journeyType,
                locationOne: locationOne, locationTwo: locationTwo, locationThree: locationThree, locationFour: locationFour, locationFive: locationFive, locationSix: locationSix,
                cabinTypeCodeOne: cabinCode, cabinTypeCodeTwo: cabinCode, cabinTypeCodeThree: cabinCode, numberOfadults: numberOfadults, numberOfchildren: numberOfchildren, numberOfyouths: numberOfyouths, numberOfinfants: numberOfinfants, deptDate: journey1Date, flighttype: flightType, corporate: corporate);
            }

            result.Add("airShoppingRQ", airShoppingRequest);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            result.Add("airShoppingResult", airShoppingResult);

            XmlNamespaceManager xmlNamespaceManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio

            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, cabinName, flight, operatorName, requiredFlightNumber);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            result.Add("offerPriceRQ", offerPriceRequest);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest).EnsureIsSuccessStatusCode();

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            result.Add("offerPriceResult", offerPriceResult);

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", xmlNamespaceManager).Count > 0, "No Priced offer returned in Offer Price response");

            string offerID = offerPriceResponseXmlDocument.SelectSingleNode("//ct:PricedOffer//ct:OfferID", xmlNamespaceManager).InnerText;

            string orderCreateRequest = "";
            string serviceListResult = "";
            string seatAvailabilityResult = "";

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            //Order create with SSR
            if (serviceCount > 0 && seatType == SeatPriceType.None)
            {
                string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);

                result.Add("serviceListRQ", serviceListRequest);

                //Return ServiceListRequest 
                if (apiResult.Equals(ApiResult.ServiceListRQ))
                {
                    return result;
                }

                HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(serviceListRequest).EnsureIsSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("serviceListResult", serviceListResult);

                if (apiResult.Equals(ApiResult.ServiceListRS))
                {
                    return result;
                }

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress,accountNumber:accountNumber,agentMailID:agentMailId);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with seat
            if (seatType != SeatPriceType.None && serviceCount == 0)
            {

                string seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                if (apiResult.Equals(ApiResult.SeatAvailabilityRQ))
                {
                    return result;
                }

                //Running XSD validation against the request for SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityRequest(seatAvailabilityRequest, out string[] seatAvailabilityRequestExceptions), string.Join(System.Environment.NewLine, seatAvailabilityRequestExceptions));

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("seatAvailabilityResult", seatAvailabilityResult);

                if (apiResult.Equals(ApiResult.SeatAvailabilityRS))
                {
                    return result;
                }

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, accountNumber:accountNumber, agentMailID: agentMailId);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Order create with SSR and seat
            if (seatType != SeatPriceType.None && serviceCount > 0)
            {
                string serviceListRequest = RequestHelper.GenerateServiceListRequest(airShoppingRequest, offerID);
                XmlDocument updatedSerivceListRequest = new XmlDocument();
                updatedSerivceListRequest.LoadXml(serviceListRequest);

                result.Add("serviceListRQ", serviceListRequest);

                HttpResponseMessage serviceListResponse = NDCTransformationEndpoints.ServiceListRequest(updatedSerivceListRequest.OuterXml).EnsureIsSuccessStatusCode();

                serviceListResult = serviceListResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("serviceListResult", serviceListResult);

                XmlDocument updatedSerivceListResponse = new XmlDocument();

                updatedSerivceListResponse.LoadXml(serviceListResult);
                string seatAvailabilityRequest = GenerateSeatAvailabilityRequest(airShoppingRequest, offerRefId);

                result.Add("seatAvailabilityRQ", seatAvailabilityRequest);

                HttpResponseMessage seatAvailabilityResponse = NDCTransformationEndpoints.SeatAvailability(seatAvailabilityRequest).EnsureIsSuccessStatusCode();

                seatAvailabilityResult = seatAvailabilityResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                result.Add("seatAvailabilityResult", seatAvailabilityResult);

                //Running XSD validation against the response from SeatAvailability
                Assert.True(XsdValidator.ValidateSeatAvailabilityResponse(seatAvailabilityResult, out string[] seatAvailabilityResultExceptions), string.Join(System.Environment.NewLine, seatAvailabilityResultExceptions));

                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, seat: seatType, seatAvailabilityXml: seatAvailabilityResult, serviceList: "service", serviceListRS: serviceListResult, serviceCount: serviceCount, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, accountNumber: accountNumber, agentMailID: agentMailId);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Only Order Create 
            if (bookingType.Equals("InstantPurchase") && seatType == SeatPriceType.None && serviceCount == 0)
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, accountNumber: accountNumber, agentMailID: agentMailId);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);
            }

            //Cash Paid Seat Request
            if (bookingType.Is("InstantPurchase") && seatType.Equals(SeatPriceType.Paid) && paymentType.IsAny("Cash", "EasyPay"))
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: seatPaymentCardType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, agentMailID: agentMailId,seatPaymentCardType:seatPaymentCardType);

                XmlDocument orderCreateCreditCardRQ = new XmlDocument();

                orderCreateCreditCardRQ.LoadXml(orderCreateRequest);

                XmlNode seatPaymentFunctionNode = orderCreateCreditCardRQ.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);

                XmlNode selectNode = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);

                XmlNode importedNode = updatedOrderCreateRequest.ImportNode(seatPaymentFunctionNode, true);

                selectNode.ParentNode.AppendChild(importedNode);

                updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions[1]//ct:PaymentProcessingDetails//ct:Amount", xmlNamespaceManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[1]//ct:TotalOfferPriceAmount", xmlNamespaceManager)?.InnerText;

                updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions[2]//ct:PaymentProcessingDetails//ct:Amount", xmlNamespaceManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:TotalOfferPriceAmount", xmlNamespaceManager)?.InnerText;

                updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions[2]//ct:OfferAssociation//ct:OfferRefID", xmlNamespaceManager).InnerText = updatedOrderCreateRequest.SelectSingleNode("//ct:CreateOrder//ct:AcceptSelectedQuotedOfferList//ct:SelectedPricedOffer[2]//ct:OfferRefID", xmlNamespaceManager)?.InnerText;
            }

            //Order Create Hold booking
            if (bookingType.Equals("HoldBooking"))
            {
                orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: paymentType, visa: visa, passport: passport, greencard: greencard, knowntraveller: knowntravller, redress: redress, accountNumber: accountNumber, agentMailID: agentMailId);

                updatedOrderCreateRequest.LoadXml(orderCreateRequest);

                XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions", xmlNamespaceManager);

                nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
            }

            result.Add("orderCreateRQ", updatedOrderCreateRequest.OuterXml);

            if (apiResult.Equals(ApiResult.OrderCreateRQ))
            {
                return result;
            }

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, bookingType: bookingType, paymentType: paymentType, seatType: seatType.ToString()).EnsureIsSuccessStatusCode();

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            XmlNodeList warning = updatedOrderCreateResponse.GetElementsByTagName("Warning");

            if (!seatType.Equals(SeatPriceType.None))
            {
                Assert.True(OrderCreateAssertions.ValidateSeatBooking(orderCreateResult, out string errorMessage), errorMessage);
            }

            string pnr = FindAndReturnDataHelper.ReturnBookingId(orderCreateResult);
            result.Add("pnr", pnr);

            result.Add("orderCreateResult", orderCreateResult);

            return result;
        }
    }

}
