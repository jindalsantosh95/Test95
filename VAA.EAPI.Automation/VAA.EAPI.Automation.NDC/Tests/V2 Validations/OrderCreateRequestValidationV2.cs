using System.Xml;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Constants;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;

namespace VAA.EAPI.Automation.NDC.Tests.OrderCreate
{
    public class OrderCreateRequestValidationV2 : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        public OrderCreateRequestValidationV2(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;

            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdIndiaTestAgent}-OcpSubID"));
            memoryStoreFixture.SetOnce(AgentID.ParticipatingOrgIdCheapflightsUKTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{AgentID.ParticipatingOrgIdCheapflightsUKTestAgent}-OcpSubID"));

        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OrdinalValueZero()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//ct:Ordinal", orderCreateRequestXmlnsManager).InnerText = "0";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid ordinal number - should start from 1"), $"Error code expected is Invalid ordinal number - should start from 1 but actual is {descText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OrdinalValueAlphabet()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:Ordinal", orderCreateRequestXmlnsManager).InnerText = "ABC";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            dynamic orderCreateResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string errorcode = orderCreateResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderCreateResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:Ordinal' element is invalid - The value 'ABC' is invalid according to its datatype 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes:OrdinalType' - The string 'ABC' is not a valid Integer value.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OrdinalValueIncorrectOrder()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            XmlNode nodeToRemove = updatedOrderCreateRequest.SelectSingleNode("//ct:Ordinal", orderCreateRequestXmlnsManager);

            XmlNode nodeToSwap = updatedOrderCreateRequest.SelectSingleNode("//ct:Ordinal", orderCreateRequestXmlnsManager);

            nodeToRemove.ParentNode.RemoveChild(nodeToRemove);

            XmlNode selectNode = updatedOrderCreateRequest.SelectSingleNode("//ct:OrgRole", orderCreateRequestXmlnsManager);

            selectNode.ParentNode.AppendChild(nodeToSwap);

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            dynamic orderCreateResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string errorcode = orderCreateResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderCreateResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'DistributionChainLink' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has invalid child element 'OrgRole' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'. List of possible elements expected: 'Ordinal' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");

        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_ContactInfoId_ContactInfo()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            updatedOrderCreateRequest.SelectSingleNode("//ct:ContactInfoID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Distribution Chain - Contact Info ID missing in the RQ"), $"Error code expected is Distribution Chain - Contact Info ID missing in the RQ but actual is {descText}");

        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_EmailAddressText()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:EmailAddressText", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("368"), $"Error code expected is 368 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Seller's email address is mandatory"), $"Seller's email address is mandatory but actual is {descText}");

        }


        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OrgId()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:OrgID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Please complete all mandatory fields as per given Specifications"), $"Please complete all mandatory fields as per given Specifications but actual is {descText}");

        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OfferRefId()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:OfferRefID", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Please complete all mandatory fields as per given Specifications"), $"Please complete all mandatory fields as per given Specifications but actual is {descText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OwnerCode()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:OwnerCode", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Owner Code is mandatory in the request"), $"Owner Code is mandatory in the request but actual is {descText[0].InnerText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_SelectedOfferItem()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:SelectedOfferItem  ", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            dynamic orderCreateResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string errorcode = orderCreateResult.statusCode?.ToString();

            Assert.True(errorcode.Equals("400"), $"Error code expected is 400 but actual is {errorcode}");

            string errordesc = orderCreateResult.message?.ToString();

            string expectedDesc = "Body of the request does not conform to the definition which is associated with the content type application/xml. The element 'SelectedOfferItem' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes' has incomplete content. List of possible elements expected: 'OfferItemRefID' in namespace 'http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes'.";

            Assert.True(errordesc.Has(expectedDesc), $"Error code expected is {expectedDesc} but actual is {errordesc}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OfferRefID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:OfferRefID  ", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderCreateRQ)"), $"Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderCreateRQ) but actual is {descText[0].InnerText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_OfferItemRefID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:OfferItemRefID ", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("913"), $"Error code expected is 913 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderCreateRQ)"), $"Invalid OfferItemID for the SelectedOffer/OfferID (Call again OrderCreateRQ) but actual is {descText[0].InnerText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_PaxRefID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:PaxRefID ", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("PaxRefID is mandatory in the request"), $"PaxRefID is mandatory in the request but actual is {descText[0].InnerText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_DataListContactInfoID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList//ct:ContactInfo//ct:ContactInfoID  ", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("912"), $"Error code expected is 912 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("Missing fields - Minimum 1 ContactInfo details needed either in Paxs/Pax/ContactInfo or PaymentInfo/PayerContactInfo"), $"Missing fields - Minimum 1 ContactInfo details needed either in Paxs/Pax/ContactInfo or PaymentInfo/PayerContactInfo but actual is {descText[0].InnerText}");
        }

        [Trait("Category", Traits.NDCOrderCreateAllRegressionV2)]
        [Fact]
        public void OrderCreate_DataListContactInfoEmail()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.SelectSingleNode("//ct:DataLists//ct:ContactInfoList//ct:ContactInfo//ct:EmailAddress//ct:EmailAddressText  ", orderCreateRequestXmlnsManager).InnerText = "";

            HttpResponseMessage response = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            XmlDocument resultDoc = new XmlDocument();

            resultDoc.LoadXml(result);

            XmlNodeList code = resultDoc.GetElementsByTagName("Code");

            Assert.True(code[0].InnerText.Equals("316"), $"Error code expected is 316 but actual is {code}");

            XmlNodeList descText = resultDoc.GetElementsByTagName("DescText");

            Assert.True(descText[0].InnerText.Equals("One Email address is mandatory in the CreateOrderRQ"), $"One Email address is mandatory in the CreateOrderRQ but actual is {descText[0].InnerText}");
        }

        private string NDC_OrderCreate()
        {
            //Generate Air Shopping request

            string deptDateTime = DateTime.UtcNow.AddDays(100).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 200;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Skyscanner", participatingOrgOrgID: "91281035",
                JourneyType.OneWay,
                locationOne: LocationType.LHR, locationTwo: LocationType.DEL,
                cabinTypeCodeOne: "5", cabinTypeCodeTwo: "5", numberOfadults: 4, numberOfchildren: 1, numberOfyouths: 1, numberOfinfants: 1);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest).EnsureIsSuccessStatusCode();

            if (!airShoppingResponse.IsSuccessStatusCode)
            {
                int retry = 5;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

                    retry--;

                } while (retry > 0 && !airShoppingResponse.IsSuccessStatusCode);
            }

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from Air Shopping - response was {airShoppingResult}");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

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

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from Offer Price - response was {offerPriceResult}");

            //Adding namespace manager

            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            return orderCreateRequest;
        }

        // Luhn Check Validation
        [Trait("Category", "Local-Run")]
        [Fact]
        public void ValidateCardNumber()
        {
            string[] creditCardNumbers = CreditCards.CreditCard;

            foreach (string card in creditCardNumbers)
            {
                string message;
                bool isValid = LuhnValidator.IsValidLuhn(card, out message);

                // Assert based on the expected outcome for each card
                if (isValid)
                {
                    Assert.True(isValid, $"Expected card number {card} to be valid.");
                    Assert.Equal($"Card number is valid.", message);
                }
                else
                {
                    Assert.False(isValid, $"Expected card number {card} to be invalid.");
                    Assert.Equal($"Card number is invalid.", message);
                }
            }
        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderCreateAPIs_InvalidPaymentCard()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "tokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            string CardNumber = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions//ct:CardNumber", orderCreateRequestXmlnsManager).InnerText;

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateCardNumber(CardNumber, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            softAssert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            updatedOrderCreateResponse.LoadXml(orderCreateResult);

            string code = updatedOrderCreateResponse.SelectSingleNode("//ct:Code", orderCreateRequestXmlnsManager).InnerText;

            string descText = updatedOrderCreateResponse.SelectSingleNode("//ct:DescText", orderCreateRequestXmlnsManager).InnerText;

            Assert.True(code.Equals("708"), $"Expected 708 but actual was {code}");

            Assert.True(descText.Equals("Incorrect credit card information"), $"Incorrect credit card information but actual was {descText}");

        }

        [Trait("Category", "Local-Run")]
        [Fact]
        public void OrderCreateAPIs_ValidPaymentCard()
        {
            string deptDateTime = DateTime.UtcNow.AddDays(250).ToString("yyyy-MM-dd");

            int maxNumberOfDays = 60;

            string journey1Date = GenerateDataHelper.RandomDate(deptDateTime, maxNumberOfDays);

            //Generate Air Shopping request
            string airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal: "0", orgRole: "Seller", participatingOrgName: "Travel Republic.co.uk", participatingOrgOrgID: AgentID.ParticipatingOrgIdTravelRepublic,
                JourneyType.Return,
                locationOne: LocationType.LHR, locationTwo: LocationType.JFK, locationThree: LocationType.JFK, locationFour: LocationType.LHR,
                cabinTypeCodeOne: "5", numberOfadults: 1, deptDate: journey1Date);

            //Post Air Shopping request
            HttpResponseMessage airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(airShoppingRequest);

            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned from AirShopping Response, returned: {airShoppingResponse.StatusCode} with message: {airShoppingResponse.ReadResponseAsString()} \r\n " + $"AirShopping Request sent was {airShoppingRequest}");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            //Adding namespace manager
            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = airShoppingResponseXmlDocument.CreateXmlNamespaceManager();

            //Find offer based on Cabin Type and Flight Scenraio
            Dictionary<string, string> offerDetails = FindAndReturnDataHelper.FindOffer(airShoppingResult, CabinType.EconomyClassic, FlightScenarioType.VirginAndNonVirgin);

            Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

            string offerRefId = offerDetails["OfferRefID"];
            string totalAmount = offerDetails["TotalAmount"];
            string shoppingRefId = offerDetails["ShoppingResponseRefID"];
            string currencyCode = offerDetails["CurrencyCode"];

            //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
            string offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

            //Post Offer Price request
            HttpResponseMessage offerPriceResponse = NDCTransformationEndpoints.OfferPrice(offerPriceRequest);

            Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned from OfferPrice Response, returned: {offerPriceResponse.StatusCode} with message: {offerPriceResponse.ReadResponseAsString()}\r\n " + $"OfferPrice Request sent was {offerPriceRequest}");

            //Getting the Offer Price response and converting to XML Document
            string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            //Adding namespace manager
            XmlNamespaceManager offerPriceResponseXmlnsManager = offerPriceResponseXmlDocument.CreateXmlNamespaceManager();

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//ct:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = updatedOrderCreateRequest.CreateXmlNamespaceManager();

            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            string CardNumber = updatedOrderCreateRequest.SelectSingleNode("//ct:PaymentFunctions//ct:CardNumber", orderCreateRequestXmlnsManager).InnerText;

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(OrderCreateAssertions.ValidateCardNumber(CardNumber, out ValidationResultHelper result), "Validation failed. Reasons: " + string.Join(", ", result.Message));

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml, memoryStoreFixture.Get<string>(AgentID.ParticipatingOrgIdTravelRepublic));

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            XmlDocument updatedOrderCreateResponse = new XmlDocument();

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();


        }
    }
}
