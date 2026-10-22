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
    [Collection("OrderCreateRequestContactInfoAndEmailIDValidations")]
    public class OrderCreateRequestContactInfoAndEmailIDValidations : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public OrderCreateRequestContactInfoAndEmailIDValidations(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }


        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithoutAnyPaxEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                itemNodes1[i].RemoveChild(itemNodes1[i].LastChild);

            }

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "Missing fields - Minimum 1 ContactInfo details needed either in Paxs/Pax/ContactInfo or PaymentInfo/PayerContactInfo";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order create error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("912"),
            $"Error Code for Order create error message is not as expected. Expected 912 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithYouthEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);

            int count = itemNodes.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                string contactInfoID = itemNodes[i].FirstChild.InnerText;
                if (contactInfoID != "CID_5")
                {
                    itemNodes[i].ParentNode.RemoveChild(itemNodes[i]);
                }
            }

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:PaxList//def:Pax", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                if (contactInfoID1 != "CID_5")
                {
                    itemNodes1[i].RemoveChild(itemNodes1[i].FirstChild);
                }
            }

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "One Email address is mandatory in the CreateOrderRQ";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order create error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("316"),
            $"Error Code for Order create error message is not as expected. Expected 316 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithChildEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);
            int count = itemNodes.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                string contactInfoID = itemNodes[i].FirstChild.InnerText;
                if (contactInfoID != "CID_5")
                {
                    itemNodes[i].ParentNode.RemoveChild(itemNodes[i]);
                }
            }

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:PaxList//def:Pax", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                if (contactInfoID1 != "CID_5")
                {
                    itemNodes1[i].RemoveChild(itemNodes1[i].FirstChild);
                }
            }

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "One Email address is mandatory in the CreateOrderRQ";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order create error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("316"),
            $"Error Code for Order create error message is not as expected. Expected 316 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithInfantEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);
            int count = itemNodes.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                string contactInfoID = itemNodes[i].FirstChild.InnerText;
                if (contactInfoID != "CID_7")
                {
                    itemNodes[i].ParentNode.RemoveChild(itemNodes[i]);
                }
            }

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:PaxList//def:Pax", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                if (contactInfoID1 != "CID_7")
                {
                    itemNodes1[i].RemoveChild(itemNodes1[i].FirstChild);
                }
            }

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            string code = "";
            string responseText = "";

            XmlNodeList nodeList = xmlDocument_1.GetElementsByTagName("Code");
            XmlNodeList errorTextNode = xmlDocument_1.GetElementsByTagName("DescText");

            if (nodeList != null && nodeList.Count > 0)
            {
                foreach (XmlNode xn in nodeList)
                {
                    code = xn.InnerText;
                }
            }

            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }

            string expectedResponse = "One Email address is mandatory in the CreateOrderRQ";

            Assert.True(responseText.Has(expectedResponse),
            $"Text value for Order create error message is not as expected. Expected {expectedResponse} " +
            $"but was {responseText}");

            Assert.True(code.Equals("316"),
            $"Error Code for Order create error message is not as expected. Expected 316 " +
            $"but was {code}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithFirstAdultEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            string Emaildrequest = updatedOrderCreateRequest.SelectSingleNode("//def:ContactInfoList//def:ContactInfo[1]//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText;

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList node1 = xmlDocument_1.GetElementsByTagName("EmailAddressText");

            string EmailIDResponse = node1[0].InnerText;

            XmlNodeList node2 = xmlDocument_1.GetElementsByTagName("ContactInfoID");

            string contactinContactList = node2[0].InnerText;

            XmlNodeList node3 = xmlDocument_1.GetElementsByTagName("ContactInfoRefID");

            string contactinPaxList = node3[0].InnerText;

            Assert.True(EmailIDResponse.Equals(Emaildrequest, StringComparison.InvariantCultureIgnoreCase),
           $"The Email ID in the request and response are not same " + $"Response {EmailIDResponse}" +
           $"but was {Emaildrequest}");

            Assert.True(contactinContactList.Equals(contactinPaxList),
           $"The Contact info in PaxList and ContactList are not same " + $"Response {contactinPaxList}" +
           $"but was {contactinContactList}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithSecondAdultEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);
            int count = itemNodes.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                string contactInfoID = itemNodes[i].FirstChild.InnerText;
                if (contactInfoID == "CID_1")
                {
                    itemNodes[i].ParentNode.RemoveChild(itemNodes[i]);
                }
            }

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:PaxList//def:Pax", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                if (contactInfoID1 == "CID_1")
                {
                    itemNodes1[i].RemoveChild(itemNodes1[i].FirstChild);
                }
            }

            string Emaildrequest = updatedOrderCreateRequest.SelectSingleNode("//def:ContactInfoList//def:ContactInfo[1]//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText;

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList node1 = xmlDocument_1.GetElementsByTagName("EmailAddressText");

            string EmailIDResponse = node1[0].InnerText;

            XmlNodeList node2 = xmlDocument_1.GetElementsByTagName("ContactInfoID");

            string contactinContactList = node2[0].InnerText;

            XmlNodeList node3 = xmlDocument_1.GetElementsByTagName("ContactInfoRefID");

            string contactinPaxList = node3[0].InnerText;

            Assert.True(EmailIDResponse.Equals(Emaildrequest, StringComparison.InvariantCultureIgnoreCase),
           $"The Email ID in the request and response are not same " + $"Response {EmailIDResponse}" +
           $"but was {Emaildrequest}");

            Assert.True(contactinContactList.Equals(contactinPaxList),
           $"The Contact info in PaxList and ContactList are not same " + $"Response {contactinPaxList}" +
           $"but was {contactinContactList}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithThirdAdultEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);
            int count = itemNodes.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                string contactInfoID = itemNodes[i].FirstChild.InnerText;
                if (contactInfoID == "CID_1" || contactInfoID == "CID_2")
                {
                    itemNodes[i].ParentNode.RemoveChild(itemNodes[i]);
                }
            }

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:PaxList//def:Pax", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                if (contactInfoID1 == "CID_1" || contactInfoID1 == "CID_2")
                {
                    itemNodes1[i].RemoveChild(itemNodes1[i].FirstChild);
                }
            }

            string Emaildrequest = updatedOrderCreateRequest.SelectSingleNode("//def:ContactInfoList//def:ContactInfo[1]//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText;

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList node1 = xmlDocument_1.GetElementsByTagName("EmailAddressText");

            string EmailIDResponse = node1[0].InnerText;

            XmlNodeList node2 = xmlDocument_1.GetElementsByTagName("ContactInfoID");

            string contactinContactList = node2[0].InnerText;

            XmlNodeList node3 = xmlDocument_1.GetElementsByTagName("ContactInfoRefID");

            string contactinPaxList = node3[0].InnerText;

            Assert.True(EmailIDResponse.Equals(Emaildrequest, StringComparison.InvariantCultureIgnoreCase),
           $"The Email ID in the request and response are not same " + $"Response {EmailIDResponse}" +
           $"but was {Emaildrequest}");

            Assert.True(contactinContactList.Equals(contactinPaxList),
           $"The Contact info in PaxList and ContactList are not same " + $"Response {contactinPaxList}" +
           $"but was {contactinContactList}");
        }

        [Trait("Category", "OrderCreateAllRegression")]
        [Fact]
        public void OrderCreateRequestWithFourthAdultEmailID()
        {
            string orderCreateRequest = NDC_OrderCreate();

            XmlDocument updatedOrderCreateRequest = new XmlDocument();

            XmlNamespaceManager orderCreateRequestXmlnsManager = new XmlNamespaceManager(updatedOrderCreateRequest.NameTable);
            orderCreateRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            updatedOrderCreateRequest.LoadXml(orderCreateRequest);

            XmlNodeList itemNodes = updatedOrderCreateRequest.SelectNodes("//def:ContactInfoList//def:ContactInfo", orderCreateRequestXmlnsManager);
            int count = itemNodes.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                string contactInfoID = itemNodes[i].FirstChild.InnerText;
                if (contactInfoID == "CID_1" || contactInfoID == "CID_2" || contactInfoID == "CID_3")
                {
                    itemNodes[i].ParentNode.RemoveChild(itemNodes[i]);
                }
            }

            XmlNodeList itemNodes1 = updatedOrderCreateRequest.SelectNodes("//def:PaxList//def:Pax", orderCreateRequestXmlnsManager);
            int count1 = itemNodes1.Count;

            for (int i = count1 - 1; i >= 0; i--)
            {
                string contactInfoID1 = itemNodes1[i].FirstChild.InnerText;
                if (contactInfoID1 == "CID_1" || contactInfoID1 == "CID_2" || contactInfoID1 == "CID_3")
                {
                    itemNodes1[i].RemoveChild(itemNodes1[i].FirstChild);
                }
            }

            string Emaildrequest = updatedOrderCreateRequest.SelectSingleNode("//def:ContactInfoList//def:ContactInfo[1]//def:EmailAddress//def:EmailAddressText", orderCreateRequestXmlnsManager).InnerText;

            XmlDocument xmlDocument_1 = new XmlDocument();

            HttpResponseMessage orderCreateResponse = NDCTransformationEndpoints.OrderCreate(updatedOrderCreateRequest.OuterXml);

            Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned from OrderCreate Response, returned: {orderCreateResponse.StatusCode} with message: {orderCreateResponse.ReadResponseAsString()}" + $"OrderCreate Request sent was {orderCreateRequest}");

            dynamic orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            xmlDocument_1.LoadXml(orderCreateResult.ToString());

            XmlNodeList node1 = xmlDocument_1.GetElementsByTagName("EmailAddressText");

            string EmailIDResponse = node1[0].InnerText;

            XmlNodeList node2 = xmlDocument_1.GetElementsByTagName("ContactInfoID");

            string contactinContactList = node2[0].InnerText;

            XmlNodeList node3 = xmlDocument_1.GetElementsByTagName("ContactInfoRefID");

            string contactinPaxList = node3[0].InnerText;

            Assert.True(EmailIDResponse.Equals(Emaildrequest, StringComparison.InvariantCultureIgnoreCase),
           $"The Email ID in the request and response are not same " + $"Response {EmailIDResponse}" +
           $"but was {Emaildrequest}");

            Assert.True(contactinContactList.Equals(contactinPaxList),
           $"The Contact info in PaxList and ContactList are not same " + $"Response {contactinPaxList}" +
           $"but was {contactinContactList}");
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
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/00/2021.3/AirShoppingRS");

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
            XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
            offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "No Priced offer returned in Offer Price response");

            //Generating the Order Create Request
            string orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, paymentMethod: "Untokenized");

            //Post Order Create request
            return orderCreateRequest;
        }
    }
}
