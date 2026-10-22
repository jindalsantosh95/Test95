using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderRetrieve
{
    public class OrderRetrieveIndividualIdValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;
        private readonly TokenFixture tokenFixture;

        

        public OrderRetrieveIndividualIdValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.tokenFixture = tokenFixture;

        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void TC001_NDC_OrderRetrieve_3DS2IndividualIdValidation4Pax()
        {
            string orderCreateRequest = NDC_OrderCreate(1, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList orderCreateRequestDOB = updatedOrderCreateRequest.GetElementsByTagName("Birthdate");
            XmlNodeList orderCreateRequestGender = updatedOrderCreateRequest.GetElementsByTagName("GenderCode");

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Create - response was {orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            //Getting the Order Create response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderCreateResponseXmlDocument = new();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList orderCreateResponseIndividualID = orderCreateResponseXmlDocument.GetElementsByTagName("IndividualID");


            //Validate IndividualID            
            Assert.True(orderCreateResponseIndividualID.Count == 0, $"The IndividualId count in the response are {orderCreateResponseIndividualID.Count}");

            //Order Retrieve 
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingId);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            XmlNodeList orderRetrieveResponseIndividualID = orderRetrieveResponseXmlDocument.GetElementsByTagName("IndividualID");


            //Validate IndividualID
            Assert.True(orderCreateResponseIndividualID.Count == 0, $"The IndividualId count in the response are {orderCreateResponseIndividualID.Count}");
        }

        [Trait("Category", "OrderRetrieveAllRegression")]
        [Fact]
        public void TC002_NDC_OrderRetrieve_HoldBookingIndividualIdValidation4Pax()
        {
            string orderCreateRequest = NDC_OrderCreate(1, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode paymentFunctionsNode = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            paymentFunctionsNode.ParentNode.RemoveChild(paymentFunctionsNode);

            XmlNodeList orderCreateRequestDOB = updatedOrderCreateRequest.GetElementsByTagName("Birthdate");
            XmlNodeList orderCreateRequestGender = updatedOrderCreateRequest.GetElementsByTagName("GenderCode");

            //Post Order Create request
            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Create - response was {orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            //Getting the Order Create response and converting to XML Document
            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument orderCreateResponseXmlDocument = new();
            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            //Adding namespace manager
            XmlNamespaceManager orderCreateResponseXmlnsManager = new(orderCreateResponseXmlDocument.NameTable);
            orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;

            XmlNodeList orderCreateResponseIndividualID = orderCreateResponseXmlDocument.GetElementsByTagName("IndividualID");

            //Validate Gender & DOB after Order create response
            Assert.True(orderCreateResponseIndividualID.Count == 0, $"The IndividualId count in the response are {orderCreateResponseIndividualID.Count}");

            //Order Retrieve 
            HttpResponseMessage orderRetrieveResponse = NDCTransformationEndpoints.OrderRetrieve(bookingId);

            Assert.True(orderRetrieveResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Retrieve - response was {orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()}");

            //Getting the Order Retrieve response and converting to XML Document
            string orderRetrieveResult = orderRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument orderRetrieveResponseXmlDocument = new();
            orderRetrieveResponseXmlDocument.LoadXml(orderRetrieveResult);

            XmlNodeList orderRetrieveResponseIndividualID = orderCreateResponseXmlDocument.GetElementsByTagName("IndividualID");

            //Validate Gender & DOB after Order create response
            Assert.True(orderCreateResponseIndividualID.Count == 0, $"The IndividualId count in the response are {orderCreateResponseIndividualID.Count}");
        }

        private string NDC_OrderCreate(int adult, int youth, int child, int infant)
        {
            //Generate Air Shopping request
            string deptDateTime = DateTime.UtcNow.AddDays(169).ToString("yyyy-MM-dd");

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "91281035",
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "4", numberOfadults: adult, numberOfchildren: child, numberOfyouths: youth, numberOfinfants: infant, deptDate: deptDateTime);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.Virgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44", paymentMethod: "UnsecureUntokenized");

            //Post Order Create request
            return orderCreateRequest;
        }
    }
}
