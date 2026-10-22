using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;


namespace VAA.EAPI.Automation.NDC.Tests.OrderRetrieve
{
    public class OrderRetrieveAssertions : IClassFixture<MemoryStoreFixture>
    {
        private readonly MemoryStoreFixture memoryStoreFixture;

        
        public static string participatingOrgOrgIdTravelRepublic = "91281035";
        public static string participatingOrgOrgIdKayakUsa = "00003333";
        public static string participatingOrgOrgIdIndiaTestAgent = "14369880";

        public OrderRetrieveAssertions(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce(participatingOrgOrgIdTravelRepublic, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdTravelRepublic}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdKayakUsa, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdKayakUsa}-OcpSubID"));
            memoryStoreFixture.SetOnce(participatingOrgOrgIdIndiaTestAgent, () => Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-{participatingOrgOrgIdIndiaTestAgent}-OcpSubID"));
        }

        public static bool ValidatePriceClassList(string requestXml, string responseXml, string cabinName, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                message = "priceClassList is not found in response";
                return false;
            }

            string cabinNameResponse = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:Name", xmlNamespaceManager).InnerText;

            if (!cabinNameResponse.Equals(cabinName, StringComparison.OrdinalIgnoreCase))
            {
                message = "cabin Name returned in response is not as expected";
                return false;
            }

            List<IGrouping<string, string>> priceClassFareBasisCode = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:FareBasisCode", xmlNamespaceManager).OfType<XmlNode>().Select(x => x.InnerText).GroupBy(x => x).ToList();

            if (!priceClassList.Count.Equals(priceClassFareBasisCode.Count))
            {
                message = "cabin Name returned in response is not as expected";
                return false;
            }


            int paxJourneysCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney", xmlNamespaceManager).Count;

            int segmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            int paxCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNodeList fareComponent = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);

            if (fareComponent.Count != paxJourneysCount * paxCount)
            {
                message = "fare component count is not as expected";
                return false;
            }

            int segmentflag = 0;

            foreach (XmlNode node in fareComponent)
            {
                segmentflag++;
                //validate price class in farecomponent
                string priceClassRefID = node.SelectSingleNode("ct:PriceClassRefID", xmlNamespaceManager).InnerText;

                XmlNodeList priceClass = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[(ct:PriceClassID='{priceClassRefID}')]", xmlNamespaceManager);

                if (!priceClass.Count.Equals(1))
                {
                    message = $"priceClassRefID is incorrect in fare component";
                    return false;
                }

                //valida farebasiscode in farecomponent
                string fareBasisCode = node.SelectSingleNode("ct:FareBasisCode", xmlNamespaceManager).InnerText;

                if (string.IsNullOrEmpty(fareBasisCode))
                {
                    message = $"FareBasisCode is incorrect in fare component";
                    return false;
                }

                //validate segments in farecomponent
                XmlNodeList segments = node.SelectNodes("ct:PaxSegmentRefID", xmlNamespaceManager);

                if (segments.Count > 1)
                {
                    foreach (XmlNode node2 in segments)
                    {
                        string segment1 = node2.InnerText;

                        if (!segment1.Equals("SEG" + segmentflag))
                        {
                            message = "segments are incorrect in fare component";
                            return false;
                        }
                    }
                }

                else
                {
                    string segment2 = node.SelectSingleNode("ct:PaxSegmentRefID", xmlNamespaceManager).InnerText;
                    if (!segment2.Equals("SEG" + segmentflag))
                    {
                        message = "segments are incorrect in fare component";
                        return false;
                    }
                }

                if (segmentflag == segmentCount)
                    segmentflag = 0;
            }

            return true;
        }

        public static bool ValidateContactList(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            string contactInfoID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:ContactInfoID", xmlNamespaceManager).InnerText.Parse<string>();

            if (!contactInfoID.Equals("CI1"))
            {
                message = $"Invalid contactInfoID in response, was expecting CI1 but found {contactInfoID} instead.";
                return false;
            }

            string emailAddressExpected = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:EmailAddress/ct:EmailAddressText", xmlNamespaceManager).InnerText.Parse<string>();

            string emailAddressActual = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:EmailAddress/ct:EmailAddressText", xmlNamespaceManager).InnerText.Parse<string>();

            if (!emailAddressExpected.Equals(emailAddressActual, StringComparison.OrdinalIgnoreCase))
            {
                message = $"Invalid emailaddress in response, was expecting {emailAddressExpected} but found {emailAddressActual} instead.";
                return false;
            }

            string contactTypeTextExpected = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:Phone/ct:ContactTypeText", xmlNamespaceManager).InnerText.Parse<string>();

            string contactTypeTextActual = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:Phone/ct:ContactTypeText", xmlNamespaceManager).InnerText.Parse<string>();

            if (!contactTypeTextExpected.Equals(contactTypeTextActual, StringComparison.OrdinalIgnoreCase))
            {
                message = $"Invalid contactTypeText in response, was expecting {contactTypeTextExpected} but found {contactTypeTextActual} instead.";
                return false;
            }

            string countryDialingCodeExpected = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:Phone/ct:CountryDialingCode", xmlNamespaceManager).InnerText.Parse<string>();

            string countryDialingCodeActual = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:Phone/ct:CountryDialingCode", xmlNamespaceManager).InnerText.Parse<string>();

            if (!countryDialingCodeExpected.Has(countryDialingCodeActual))
            {
                message = $"Invalid countryDialingCode in response, was expecting {countryDialingCodeExpected} but found {countryDialingCodeActual} instead.";
                return false;
            }

            string phoneNumberExpected = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:Phone/ct:PhoneNumber", xmlNamespaceManager).InnerText.Parse<string>();

            string phoneNumberActual = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ContactInfoList/ct:ContactInfo/ct:Phone/ct:PhoneNumber", xmlNamespaceManager).InnerText.Parse<string>();

            if (!phoneNumberExpected.Has(phoneNumberActual))
            {
                message = $"Invalid phoneNumber in response, was expecting {phoneNumberExpected} but found {phoneNumberActual} instead.";
                return false;
            }

            return true;
        }

        //PaxList Assertion
        public static bool ValidatePaxList(string requestXml, string responseXml, out string message, string gst = "", int gstPaxCount = 0)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            XmlNode expectedPaxList = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList", xmlNamespaceManager);
            XmlNode actualPaxList = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList", xmlNamespaceManager);

            XmlElement requestPaxList = paxListXmlDocument.CreateElement("Request");
            XmlElement responsePaxList = paxListXmlDocument.CreateElement("Response");

            requestPaxList.AppendChild(paxListXmlDocument.ImportNode(expectedPaxList.CloneNode(true), true));
            responsePaxList.AppendChild(paxListXmlDocument.ImportNode(actualPaxList.CloneNode(true), true));

            XmlElement datalistXmlNode = paxListXmlDocument.CreateElement("Datalist");

            datalistXmlNode.AppendChild(requestPaxList);
            datalistXmlNode.AppendChild(responsePaxList);

            paxListXmlDocument.AppendChild(datalistXmlNode);

            int expectedPaxListCount = expectedPaxList.SelectNodes("ct:Pax", xmlNamespaceManager).Count;
            int actualPaxListCount = actualPaxList.SelectNodes("ct:Pax", xmlNamespaceManager).Count;
            int validatedPaxListCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:PaxID = ../../../Response/ct:PaxList/ct:Pax/ct:PaxID and ct:Individual/ct:Birthdate = ../../../Response/ct:PaxList/ct:Pax/ct:Individual/ct:Birthdate and ct:Individual/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:Individual/ct:GivenName and ct:Individual/ct:Surname= ../../../Response/ct:PaxList/ct:Pax/ct:Individual/ct:Surname]", xmlNamespaceManager).Count;

            int validatedPaxListCount1 = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:Employer/ct:Name = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:Name and ct:Employer/ct:SalesTaxRegistration/ct:CityName = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:CityName and  ct:Employer/ct:SalesTaxRegistration/ct:CountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:CountryCode and ct:Employer/ct:SalesTaxRegistration/ct:CountryDialingCode = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:CountryDialingCode and ct:Employer/ct:SalesTaxRegistration/ct:EmailAddressText = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:EmailAddressText and ct:Employer/ct:SalesTaxRegistration/ct:PostalCode = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:PostalCode and ct:Employer/ct:SalesTaxRegistration/ct:SalesTaxRegistrationID = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:SalesTaxRegistrationID and ct:Employer/ct:SalesTaxRegistration/ct:StreetText = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:StreetText and ct:Employer/ct:TaxDeductedatSourceCapabilityInd = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:TaxDeductedatSourceCapabilityInd]", xmlNamespaceManager).Count;


            if (!string.IsNullOrEmpty(gst) && gstPaxCount > 0 && !gstPaxCount.Equals(validatedPaxListCount1))
            {
                message = $"Invalid PaxList, was expecting {gstPaxCount} but found {validatedPaxListCount1} instead.";
                return false;
            }

            if (!((expectedPaxListCount + actualPaxListCount) / 2).Equals(validatedPaxListCount))
            {
                message = $"Invalid PaxList, was expecting {expectedPaxListCount} but found {validatedPaxListCount} instead.";
                return false;
            }

            string contactInfoRefID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax/ct:ContactInfoRefID", xmlNamespaceManager).InnerText;

            if (!contactInfoRefID.Equals("CI1"))
            {
                message = $"Invalid contactInfoRefID in paxList, was expecting CI1 but found {contactInfoRefID} instead.";
                return false;
            }

            return true;
        }

        //OriginDestination List
        public static bool ValidateOriginDestList(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlDocument orgDestListXmlDocument = new XmlDocument();

            XmlNode expectedOrgDestList = airShoppingRequestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria", xmlNamespaceManager);
            XmlNode actualOrgDestList = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:OriginDestList", xmlNamespaceManager);

            XmlElement requestOrgDestList = orgDestListXmlDocument.CreateElement("Request");
            XmlElement responseOrgDestList = orgDestListXmlDocument.CreateElement("Response");

            requestOrgDestList.AppendChild(orgDestListXmlDocument.ImportNode(expectedOrgDestList.CloneNode(true), true));
            responseOrgDestList.AppendChild(orgDestListXmlDocument.ImportNode(actualOrgDestList.CloneNode(true), true));

            XmlElement datalistXmlNode = orgDestListXmlDocument.CreateElement("Datalist");

            datalistXmlNode.AppendChild(requestOrgDestList);
            datalistXmlNode.AppendChild(responseOrgDestList);

            orgDestListXmlDocument.AppendChild(datalistXmlNode);

            int expectedOrgDestListCount = expectedOrgDestList.SelectNodes("ct:OriginDestCriteria", xmlNamespaceManager).Count;
            int actualOrgDestListCount = actualOrgDestList.SelectNodes("ct:OriginDest", xmlNamespaceManager).Count;
            int validatedOrgDestListCount = orgDestListXmlDocument.SelectNodes("Datalist/Request/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[ct:OriginDepCriteria/ct:IATA_LocationCode = ../../../Response/ct:OriginDestList/ct:OriginDest/ct:OriginCode and ct:DestArrivalCriteria/ct:IATA_LocationCode = ../../../Response/ct:OriginDestList/ct:OriginDest/ct:DestCode]", xmlNamespaceManager).Count;

            if (!((expectedOrgDestListCount + actualOrgDestListCount) / 2).Equals(validatedOrgDestListCount))
            {
                message = $"Invalid PaxList, was expecting {expectedOrgDestListCount} but found {validatedOrgDestListCount} instead.";
                return false;
            }

            int paxJourneys = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest[ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!paxJourneys.Equals(expectedOrgDestListCount))
            {
                message = "columnID Request requested and column ID in response are not same";
                return false;
            }

            return true;
        }

        //PaxJourneyList
        public static bool ValidatePaxJourneyList(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int expectedPaxJourneyCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest", xmlNamespaceManager).Count;

            int actualPaxJourneyCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney", xmlNamespaceManager).Count;

            Assert.True(expectedPaxJourneyCount.Equals(actualPaxJourneyCount), $"Invalid actualPaxJourneyCount in response, was expecting {expectedPaxJourneyCount} but found {actualPaxJourneyCount} instead.");

            IGrouping<string, XmlNode>[] paxJourneys = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney[ct:PaxJourneyID=../../../ct:DataLists/ct:OriginDestList/ct:OriginDest/ct:PaxJourneyRefID and ct:PaxSegmentRefID=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxJourneyID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxJourneys.Length.Equals(expectedPaxJourneyCount))
            {
                message = $"Invalid PaxJourneys, was expecting {expectedPaxJourneyCount} but found {paxJourneys.Length} instead.";
                return false;
            }

            bool duration = paxJourneys.All(x => x.All(y => y.SelectSingleNode("ct:Duration", xmlNamespaceManager).InnerText.Equals(null)));

            if (!duration == false)
            {
                message = "columnIDRequest requested and colum ID in response are not same";
                return false;
            }

            return true;
        }

        //DatedMarketingAndOperatingList
        public static bool ValidateDatedMarketingAndOperatingList(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] datedMarketingSegments = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[ct:DatedMarketingSegmentId=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:DatedMarketingSegmentRefId and ct:DatedOperatingSegmentRefId=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingSegmentId and ct:Arrival/ct:IATA_LocationCode != ct:Dep/ct:IATA_LocationCode  and ct:Arrival/ct:AircraftScheduledDateTime != ct:Dep/ct:AircraftScheduledDateTime]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:DatedMarketingSegmentId", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!datedMarketingSegments.Length.Equals(datedMarketingSegmentCount))
            {
                message = $"Invalid datedMarketingSegments, was expecting {datedMarketingSegmentCount} but found {datedMarketingSegments.Length} instead.";
                return false;
            }

            bool carrierDesigCode = datedMarketingSegments.All(x => x.All(y => y.SelectSingleNode("ct:CarrierDesigCode", xmlNamespaceManager).InnerText.Equals("VS", StringComparison.OrdinalIgnoreCase)));

            if (!carrierDesigCode == true)
            {
                message = "columnIDRequest requested and colum ID in response are not same";
                return false;
            }

            bool carrierName = datedMarketingSegments.All(x => x.All(y => y.SelectSingleNode("ct:CarrierName", xmlNamespaceManager).InnerText.Equals("Virgin Atlantic Airways", StringComparison.OrdinalIgnoreCase)));

            if (!carrierName == true)
            {
                message = "columnIDRequest requested and colum ID in response are not same";
                return false;
            }

            return true;
        }

        //PaxSegmentList
        public static bool ValidatePaxSegmentList(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int paxSegmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] paxSegments = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[ct:PaxSegmentID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxSegmentRefID and ct:DatedMarketingSegmentRefId=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:DatedMarketingSegmentId]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxSegmentID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxSegments.Length.Equals(paxSegmentCount))
            {
                message = $"Invalid paxSegments, was expecting {paxSegmentCount} but found {paxSegments.Length} instead.";
                return false;
            }

            int paxSegmentIdCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID", xmlNamespaceManager).Count;

            for (int i = 1; i <= paxSegmentCount; i++)
            {
                string paxSegmentId = orderCreateResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{i}]/ct:PaxSegmentID", xmlNamespaceManager).InnerText;

                if (!paxSegmentId.Equals("SEG" + i))
                {
                    message = $"paxsegmentId is not as expected SEG{i} actual {paxSegmentId}";
                    return false;
                }
            }

            return true;
        }

        //ServiceDefinitionList
        public static bool ValidateServiceDefinitionList(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int serviceDefinitionCout = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).Count;

            IEnumerable<XmlNode> serviceDefinitions = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:ServiceDefinitionAssociation/ct:BaggageAllowanceRef/ct:BaggageAllowanceRefID=../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID]", xmlNamespaceManager)
                                                            .OfType<XmlNode>();

            XmlNodeList serviceDefinitionAmenities = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:Desc]", xmlNamespaceManager);

            XmlNodeList baggageAllowance = orderCreateResponseXmlDocument.GetElementsByTagName("BaggageAllowanceID");

            int serviceIDFlag = 0;

            foreach (XmlNode serviceDefinition in serviceDefinitions)
            {
                serviceIDFlag++;

                string serviceID = serviceDefinition.SelectSingleNode("ct:ServiceDefinitionID", xmlNamespaceManager).InnerText;
                if (!serviceID.Equals("SRV" + serviceIDFlag))
                {
                    message = $"serviceDefinitionId is Invalid Expected SRV+ {serviceIDFlag} but actual is {serviceID} ";
                    return false;
                }
            }

            return true;
        }

        //OrderItemService
        public static bool ValidateOrderItemIDService(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            XmlNode expectedPaxList = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList", xmlNamespaceManager);
            XmlNode actualPaxList = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList", xmlNamespaceManager);

            XmlElement requestPaxList = paxListXmlDocument.CreateElement("Request");
            XmlElement responsePaxList = paxListXmlDocument.CreateElement("Response");

            requestPaxList.AppendChild(paxListXmlDocument.ImportNode(expectedPaxList.CloneNode(true), true));
            responsePaxList.AppendChild(paxListXmlDocument.ImportNode(actualPaxList.CloneNode(true), true));

            XmlElement datalistXmlNode = paxListXmlDocument.CreateElement("Datalist");

            datalistXmlNode.AppendChild(requestPaxList);
            datalistXmlNode.AppendChild(responsePaxList);

            paxListXmlDocument.AppendChild(datalistXmlNode);

            int expectedPaxListCount = expectedPaxList.SelectNodes("ct:Pax", xmlNamespaceManager).Count;
            int actualPaxListCount = actualPaxList.SelectNodes("ct:Pax", xmlNamespaceManager).Count;
            int validatedPaxListCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:PaxID = ../../../Response/ct:PaxList/ct:Pax/ct:PaxID and ct:Individual/ct:Birthdate = ../../../Response/ct:PaxList/ct:Pax/ct:Individual/ct:Birthdate and ct:Individual/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:Individual/ct:GivenName and ct:Individual/ct:Surname= ../../../Response/ct:PaxList/ct:Pax/ct:Individual/ct:Surname]", xmlNamespaceManager).Count;

            if (!((expectedPaxListCount + actualPaxListCount) / 2).Equals(validatedPaxListCount))
            {
                message = $"Invalid PaxList, was expecting {expectedPaxListCount} but found {validatedPaxListCount} instead.";
                return false;
            }

            int paxSegmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] paxServices = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxRefID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxServices.Length.Equals(validatedPaxListCount))
            {
                message = $"Invalid PaxList, was expecting {expectedPaxListCount} but found {validatedPaxListCount} instead.";
                return false;
            }

            bool segmentsConfrimed = paxServices.All(x => x.All(y => y.SelectSingleNode("ct:StatusCode", xmlNamespaceManager).InnerText.Equals("CONFIRMED", StringComparison.OrdinalIgnoreCase)));

            if (!segmentsConfrimed == true)
            {
                message = "columnIDRequest requested and colum ID in response are not same";
                return false;
            }

            bool serviceIDs = paxServices.All(x => x.All(y => y.SelectSingleNode("ct:ServiceID", xmlNamespaceManager).InnerText.Equals(null)));

            if (!serviceIDs == false)
            {
                message = "columnIDRequest requested and colum ID in response are not same";
                return false;
            }

            return true;
        }

        //Booking ID
        public static bool ValidateBookingID(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            string bookingID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", xmlNamespaceManager).InnerText;


            IGrouping<string, XmlNode>[] paxServices = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager)
                                                           .OfType<XmlNode>()
                                                           .GroupBy(x => x.SelectSingleNode("ct:PaxRefID", xmlNamespaceManager).InnerText)
                                                           .ToArray();

            bool bookingIDs = paxServices.All(x => x.All(y => y.SelectSingleNode("ct:BookingRef/ct:BookingID", xmlNamespaceManager).InnerText.Equals(bookingID, StringComparison.OrdinalIgnoreCase)));

            if (!bookingIDs == true)
            {
                message = "Expected all bookings for all segments to be confirmed";
                return false;
            }

            return true;
        }

        //TicketDocument
        public static bool ValidateTicketDocument(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            int expectedTicketDocCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            int paxSegmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] tickets = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID  and  ct:Ticket/ct:Coupon/ct:FareBasisCode]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxRefID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!expectedTicketDocCount.Equals(tickets.Length))
            {
                message = "Invalid tickets, actual{tickets.Length} but exepected{expectedTicketDocCount}";
                return false;
            }

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            bool issueDates = tickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssueDate", xmlNamespaceManager).InnerText.Equals(todaysDate, StringComparison.OrdinalIgnoreCase)));

            if (!issueDates == true)
            {
                message = "IssueDate date should be {todaysDate}";
                return false;
            }

            bool airlineDesigCodes = tickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode", xmlNamespaceManager).InnerText.Equals("VS")));

            if (!airlineDesigCodes == true)
            {
                message = "AirlineDesigCode should be VS";
                return false;
            }

            bool bookletQtys = tickets.All(x => x.All(y => y.SelectSingleNode("ct:BookletQty", xmlNamespaceManager).InnerText.Equals("1")));

            if (!bookletQtys == true)
            {
                message = "BookletQty should be 1";
                return false;
            }

            bool ticketDocTypeCodes = tickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketDocTypeCode", xmlNamespaceManager).InnerText.Equals("T")));

            if (!ticketDocTypeCodes == true)
            {
                message = "TicketDocTypeCode should be T";
                return false;
            }

            bool ticketNumbers = tickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager).InnerText.Equals(null)));

            if (!ticketNumbers == false)
            {
                message = "Every ticket doctInfo should have TicketNumber";
                return false;
            }

            int couponsCount = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo/ct:Ticket/ct:Coupon[ct:FareBasisCode]", xmlNamespaceManager).Count;

            if (!couponsCount.Equals(expectedTicketDocCount * paxSegmentCount))
            {
                message = $"Invalid tickets, actual{couponsCount} but exepected{expectedTicketDocCount}";
                return false;
            }

            return true;
        }

        //BaggaeAllowancelist
        public static bool ValidateBaggaeAllowancelist(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList baggageAllowanceCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance", xmlNamespaceManager);

            foreach (XmlNode baggageAllowance in baggageAllowanceCount)
            {
                string baggageAllowanceID = baggageAllowance.SelectSingleNode("ct:BaggageAllowanceID", xmlNamespaceManager).InnerText;

                string typeCode = baggageAllowance.SelectSingleNode("ct:TypeCode", xmlNamespaceManager).InnerText;

                if ((!baggageAllowanceID.Has("BAGA") && typeCode.Equals("Checked")) || (!baggageAllowanceID.Has("CAB") && typeCode.Equals("CarryOn")))
                {
                    message = $"Invalid Type Code Exepected{typeCode}";
                    return false;
                }

                XmlNodeList maximumDimensionAllowance = baggageAllowance.SelectNodes("ct:MaximumDimensionAllowance[ct:HeightMeasure and ct:LengthMeasure and ct:LengthUnitOfMeasurement and ct:LinearMeasure and ct:WidthMeasure]", xmlNamespaceManager);

                if (maximumDimensionAllowance.Count != 2)
                {
                    message = "maximumDimensionAllowances are not returned";
                    return false;
                }

                XmlNodeList weightAllowance = baggageAllowance.SelectNodes("ct:WeightAllowance[ct:MaximumWeightMeasure and ct:WeightUnitOfMeasurement]", xmlNamespaceManager);

                if (weightAllowance.Count != 2 && typeCode.Equals("CarryOn"))
                {
                    message = "weightAllowances are not returned";
                    return false;
                }
            }

            return true;
        }

        //OrderItemIDPrice
        public static bool ValidateOrderItemIdPrice(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            double expectedTotalPrice = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:PaymentFunctions/ct:PaymentProcessingDetails/ct:Amount", xmlNamespaceManager).InnerText.Parse<double>();
            double actualTotalPrice = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<double>();

            if (!expectedTotalPrice.Equals(actualTotalPrice))
            {
                message = $"Invalid total price, was expecting {expectedTotalPrice} but found {actualTotalPrice} instead.";
                return false;
            }

            int totalOrderItemCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID/ct:OrderItemID", xmlNamespaceManager).Count;

            string orderID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            int expectedOrderItemCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            int prices = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price[../ct:Service/ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ../ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager).Count;

            if (!prices.Equals(expectedOrderItemCount))
            {
                message = $"Invalid PaxList, was expecting {expectedOrderItemCount} but found {prices} instead.";
                return false;
            }

            XmlNodeList descText = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price[../ct:Service/ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ../ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]//DescText", xmlNamespaceManager);
            XmlNodeList taxCode = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price[../ct:Service/ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ../ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]//DescText", xmlNamespaceManager);

            for (int i = descText.Count - 1; i >= 0; i--)
            {
                if (descText[1].InnerText.IsNullOrEmpty())
                {
                    message = $"DescText should not be null or empty";
                    return false;
                }
            }

            for (int i = taxCode.Count - 1; i >= 0; i--)
            {
                if (taxCode[1].InnerText.IsNullOrEmpty())
                {
                    message = $"Every price tax should have a TaxCode";
                    return false;
                }
            }

            return true;
        }

        //OrderItemIDSeat
        public static bool ValidteOrderItemIDSeat(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int expectedSeatCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[2]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            XmlNodeList columnIDRequest = orderCreateRequestXmlDocument.GetElementsByTagName("ColumnID");

            XmlNodeList columnIDResponse = orderCreateResponseXmlDocument.GetElementsByTagName("ColumnID");

            XmlNodeList rowNumberRequest = orderCreateRequestXmlDocument.GetElementsByTagName("SeatRowNumber");

            XmlNodeList rowNumberResponse = orderCreateResponseXmlDocument.GetElementsByTagName("RowNumber");

            for (int i = 0; i < columnIDRequest.Count; i++)
            {
                if (!columnIDRequest[i].InnerText.Equals(columnIDResponse[i].InnerText))
                {
                    message = "columnIDRequest requested and colum ID in response are not same";
                    return false;
                }

                if (!rowNumberRequest[i].InnerText.Equals(rowNumberResponse[i].InnerText))
                {
                    message = "columnIDRequest requested and colum ID in response are not same";
                    return false;
                }
            }

            IGrouping<string, XmlNode>[] seatOrderItems = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:OrderServiceAssociation/ct:SeatOnLeg/ct:SeatAssignmentAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID =  ../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Service/ct:PaxRefID = ../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager)
                                                           .OfType<XmlNode>()
                                                           .GroupBy(x => x.SelectSingleNode("ct:OrderItemID", xmlNamespaceManager).InnerText)
                                                           .ToArray();

            if (!seatOrderItems.Length.Equals(expectedSeatCount))
            {
                message = "Number of seats requested are not same as in response";
                return false;
            }

            bool segmentsConfrimed = seatOrderItems.All(x => x.All(y => y.SelectSingleNode("ct:Service/ct:StatusCode", xmlNamespaceManager).InnerText.Equals("CONFIRMED", StringComparison.OrdinalIgnoreCase)));

            if (!segmentsConfrimed == true)
            {
                message = "Expected all bookings for all segments to be confirmed";
                return false;
            }

            bool serviceIDs = seatOrderItems.All(x => x.All(y => y.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText.Equals(null)));

            if (!serviceIDs == false)
            {
                message = "serviceID is expected in all services but not found";
                return false;
            }

            return true;
        }

        //TicketDocumentSeat
        public static bool ValidateTicketDocumentSeat(string requestXml, string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int expectedSeatTicketDocCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[2]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] seattickets = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Ticket/ct:TicketDocTypeCode='J']", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!expectedSeatTicketDocCount.Equals(seattickets.Length))
            {
                message = $"Invalid tickets, actual{seattickets.Length} but exepected{expectedSeatTicketDocCount}";
                return false;
            }

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            bool issueDates = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssueDate", xmlNamespaceManager).InnerText.Equals(todaysDate, StringComparison.OrdinalIgnoreCase)));

            if (!issueDates == true)
            {
                message = $"IssueDate date should be {todaysDate}";
                return false;
            }

            bool airlineDesigCodes = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode", xmlNamespaceManager).InnerText.Equals("VS")));

            if (!airlineDesigCodes == true)
            {
                message = "AirlineDesigCode should be VS";
                return false;
            }

            bool bookletQtys = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:BookletQty", xmlNamespaceManager).InnerText.Equals("1")));

            if (!bookletQtys == true)
            {
                message = "BookletQty should be 1";
                return false;
            }

            bool ticketDocTypeCodes = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketDocTypeCode", xmlNamespaceManager).InnerText.Equals("J")));

            if (!ticketDocTypeCodes == true)
            {
                message = "TicketDocTypeCode should be J";
                return false;
            }

            bool ticketNumbers = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager).InnerText.Equals(null)));

            if (!ticketNumbers == false)
            {
                message = "Every ticket doctInfo should have TicketNumber";
                return false;
            }

            return true;
        }

        //Aircraft type code and Name
        public static bool ValidateAirCarftTypeCode(string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument OrderQuoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList datedOperatingSegments = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

            foreach (XmlNode datedOperatingSegment in datedOperatingSegments)
            {
                XmlNode AirCraftTypecodeNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeCode", xmlNamespaceManager);

                if (AirCraftTypecodeNode is null)
                {
                    message = "AircrafttypeCode node is not found in one of the DatedOperatingSegments";
                    return false;
                }

                XmlNode AircraftTypeNameNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeName", xmlNamespaceManager);

                if (AircraftTypeNameNode is null)
                {
                    message = "AircraftTypeName node is not found in one of the DatedOperatingSegments";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateOperatingLeg(string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName and ct:IATA_AircraftType/ct:IATA_AircraftTypeCode]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    message = $"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.";
                    return false;
                }
            }

            return true;
        }
    }
}
