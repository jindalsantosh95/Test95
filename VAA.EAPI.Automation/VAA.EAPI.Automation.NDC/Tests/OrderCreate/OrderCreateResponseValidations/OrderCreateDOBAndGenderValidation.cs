using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderCreate.OrderCreateResponseValidations
{
    public class OrderCreateDOBAndGenderValidation : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public OrderCreateDOBAndGenderValidation(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }


        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void NDC_OrderCreateRequestWithoutPayerPhoneAndAddress()
        {
            string orderCreateRequest = NDC_OrderCreate(1, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Create - response was {orderCreateResult}");

        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void NDC_3DS2GenderCodeAndDOBValidation5Pax()
        {
            string orderCreateRequest = NDC_OrderCreate(2, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);


            Console.WriteLine(orderCreateRequest);

            XmlNodeList requestDOB = updatedOrderCreateRequest.GetElementsByTagName("Birthdate");

            XmlNodeList requestGender = updatedOrderCreateRequest.GetElementsByTagName("GenderCode");

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(orderCreateResult);

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList responseDOB = xmlDocument_1.GetElementsByTagName("Birthdate");

            XmlNodeList responseGender = xmlDocument_1.GetElementsByTagName("GenderCode");

            for (int i = requestDOB.Count - 1; i >= 0; i--)
            {
                Console.WriteLine(requestDOB[i].InnerText);
                Console.WriteLine(responseDOB[i].InnerText);

                //Comparing the date of birth in request and response

                Assert.True(requestDOB[i].InnerText.Equals(responseDOB[i].InnerText),
                 $"The DOB in the request and response are not same " + $"Response {requestDOB[i].InnerText}" +
                 $"but was {responseDOB[i].InnerText}");

                //Comparing the gendercode in request and response

                Console.WriteLine(requestGender[i].InnerText);
                Console.WriteLine(responseGender[i].InnerText);

                Assert.True(requestGender[i].InnerText.Equals(responseGender[i].InnerText),
                 $"The Gender in the request and response are not same " + $"Response {requestGender[i].InnerText}" +
                 $"but was {responseGender[i].InnerText}");

            }
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void NDC_3DS2GenderCodeAndDOBValidation4Pax()
        {
            string orderCreateRequest = NDC_OrderCreate(1, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);


            Console.WriteLine(orderCreateRequest);

            XmlNodeList requestDOB = updatedOrderCreateRequest.GetElementsByTagName("Birthdate");

            XmlNodeList requestGender = updatedOrderCreateRequest.GetElementsByTagName("GenderCode");

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(orderCreateResult);

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList responseDOB = xmlDocument_1.GetElementsByTagName("Birthdate");

            XmlNodeList responseGender = xmlDocument_1.GetElementsByTagName("GenderCode");

            for (int i = requestDOB.Count - 1; i >= 0; i--)
            {
                Console.WriteLine(requestDOB[i].InnerText);
                Console.WriteLine(responseDOB[i].InnerText);

                //Comparing the date of birth in request and response

                Assert.True(requestDOB[i].InnerText.Equals(responseDOB[i].InnerText),
                 $"The DOB in the request and response are not same " + $"Response {requestDOB[i].InnerText}" +
                 $"but was {responseDOB[i].InnerText}");

                //Comparing the gendercode in request and response

                Console.WriteLine(requestGender[i].InnerText);
                Console.WriteLine(responseGender[i].InnerText);

                Assert.True(requestGender[i].InnerText.Equals(responseGender[i].InnerText),
                 $"The Gender in the request and response are not same " + $"Response {requestGender[i].InnerText}" +
                 $"but was {responseGender[i].InnerText}");

            }
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void NDC_HoldBookingGenderCodeAndDOBValidation5Pax()
        {
            string orderCreateRequest = NDC_OrderCreate(2, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode paymentFunctionsNode = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            paymentFunctionsNode.ParentNode.RemoveChild(paymentFunctionsNode);

            Console.WriteLine(updatedOrderCreateRequest.OuterXml);

            XmlNodeList requestDOB = updatedOrderCreateRequest.GetElementsByTagName("Birthdate");

            XmlNodeList requestGender = updatedOrderCreateRequest.GetElementsByTagName("GenderCode");

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(orderCreateResult);

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList responseDOB = xmlDocument_1.GetElementsByTagName("Birthdate");

            XmlNodeList responseGender = xmlDocument_1.GetElementsByTagName("GenderCode");

            for (int i = requestDOB.Count - 1; i >= 0; i--)
            {
                Console.WriteLine(requestDOB[i].InnerText);
                Console.WriteLine(responseDOB[i].InnerText);

                //Comparing the date of birth in request and response

                Assert.True(requestDOB[i].InnerText.Equals(responseDOB[i].InnerText),
                 $"The DOB in the request and response are not same " + $"Response {requestDOB[i].InnerText}" +
                 $"but was {responseDOB[i].InnerText}");

                //Comparing the gendercode in request and response

                Console.WriteLine(requestGender[i].InnerText);
                Console.WriteLine(responseGender[i].InnerText);

                Assert.True(requestGender[i].InnerText.Equals(responseGender[i].InnerText),
                 $"The Gender in the request and response are not same " + $"Response {requestGender[i].InnerText}" +
                 $"but was {responseGender[i].InnerText}");

            }
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void NDC_HoldBookingGenderCodeAndDOBValidation4Pax()
        {
            string orderCreateRequest = NDC_OrderCreate(1, 1, 1, 1);

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNode paymentFunctionsNode = updatedOrderCreateRequest.SelectSingleNode("//def:PaymentFunctions", orderCreateRequestXmlnsManager);
            paymentFunctionsNode.ParentNode.RemoveChild(paymentFunctionsNode);

            Console.WriteLine(updatedOrderCreateRequest.OuterXml);

            XmlNodeList requestDOB = updatedOrderCreateRequest.GetElementsByTagName("Birthdate");

            XmlNodeList requestGender = updatedOrderCreateRequest.GetElementsByTagName("GenderCode");

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Console.WriteLine(orderCreateResult);

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList responseDOB = xmlDocument_1.GetElementsByTagName("Birthdate");

            XmlNodeList responseGender = xmlDocument_1.GetElementsByTagName("GenderCode");

            for (int i = requestDOB.Count - 1; i >= 0; i--)
            {
                Console.WriteLine(requestDOB[i].InnerText);
                Console.WriteLine(responseDOB[i].InnerText);

                //Comparing the date of birth in request and response

                Assert.True(requestDOB[i].InnerText.Equals(responseDOB[i].InnerText),
                 $"The DOB in the request and response are not same " + $"Response {requestDOB[i].InnerText}" +
                 $"but was {responseDOB[i].InnerText}");

                //Comparing the gendercode in request and response

                Console.WriteLine(requestGender[i].InnerText);
                Console.WriteLine(responseGender[i].InnerText);

                Assert.True(requestGender[i].InnerText.Equals(responseGender[i].InnerText),
                 $"The Gender in the request and response are not same " + $"Response {requestGender[i].InnerText}" +
                 $"but was {responseGender[i].InnerText}");

            }
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
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
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
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult}");

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, "+44");

            //Post Order Create request
            return orderCreateRequest;
        }
    }
}