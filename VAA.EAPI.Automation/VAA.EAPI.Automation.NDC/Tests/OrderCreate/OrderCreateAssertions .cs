using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;


namespace VAA.EAPI.Automation.NDC.Tests.OrderCreate
{
    public class OrderCreateAssertions
    {
        public static bool ValidateOperatingLeg(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(requestXml);
            responseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateSeatBooking(string responseXml, out string message)
        {
            message = string.Empty;

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList warning = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Warning", xmlNamespaceManager);

            foreach (XmlNode node in warning)
            {
                string desc = node.SelectSingleNode("ct:DescText", xmlNamespaceManager).InnerText;

                if (desc.Has("Unable to Process Advance Seat Request") || desc.Has("Seat"))
                {
                    message = $"{desc}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateAPIs(string requestXml, string responseXml, out ValidationResultHelper result, string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", int gstPaxCount = 0)
        {
            result = new ValidationResultHelper();

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

            XmlNodeList expectedPassportCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:IdentityDocTypeCode='PT']", xmlNamespaceManager);

            XmlNodeList expectedGreenCardCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:IdentityDocTypeCode='GC']", xmlNamespaceManager);

            XmlNodeList expectedKnownTravellerCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:IdentityDocTypeCode='CR']", xmlNamespaceManager);

            XmlNodeList expectedRedressCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[ct:RedressCase]", xmlNamespaceManager);

            XmlNodeList expectedVisaCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:Visa]", xmlNamespaceManager);


            if (!string.IsNullOrEmpty(passport))
            {
                int actualPassportCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:IdentityDoc/ct:Birthdate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Birthdate and ct:IdentityDoc/ct:CitizenshipCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:CitizenshipCountryCode and ct:IdentityDoc/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ExpiryDate and ct:IdentityDoc/ct:GenderCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GenderCode  and ct:IdentityDoc/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GivenName and ct:IdentityDoc/ct:IdentityDocID = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocID and ct:IdentityDoc/ct:IdentityDocTypeCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocTypeCode and ct:IdentityDoc/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssuingCountryCode and ct:IdentityDoc/ct:ResidenceCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ResidenceCountryCode and ct:IdentityDoc/ct:Surname = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Surname]", xmlNamespaceManager).Count;

                if (expectedPassportCount.Count != actualPassportCount)
                {
                    result.Message.Add($"Incorrect Passport, was expecting {expectedPassportCount.Count} but found {actualPassportCount} instead.");
                }

            }

            if (!string.IsNullOrEmpty(visa) && string.IsNullOrEmpty(greencard))
            {
                int actualVisaCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:IdentityDoc/ct:Visa/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Visa/ct:ExpiryDate  and   ct:IdentityDoc/ct:Visa/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Visa/ct:IssuingCountryCode and ct:IdentityDoc/ct:Visa/ct:HostCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Visa/ct:HostCountryCode]", xmlNamespaceManager).Count;

                if (expectedVisaCount.Count != actualVisaCount)
                {
                    result.Message.Add($"Incorrect Visa, was expecting {expectedVisaCount.Count} but found {actualVisaCount} instead.");

                }
            }

            if (!string.IsNullOrEmpty(knowntraveller))
            {
                int actualKnowntraveller = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:IdentityDoc/ct:IdentityDocTypeCode = 'CR' and ct:IdentityDoc/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ExpiryDate and ct:IdentityDoc/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GivenName and ct:IdentityDoc/ct:IdentityDocID = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocID and ct:IdentityDoc/ct:IdentityDocTypeCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocTypeCode and ct:IdentityDoc/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssuingCountryCode and ct:IdentityDoc/ct:ResidenceCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ResidenceCountryCode and ct:IdentityDoc/ct:Surname = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Surname]", xmlNamespaceManager).Count;

                if (expectedKnownTravellerCount.Count != actualKnowntraveller)
                {
                    result.Message.Add($"Incorrect KnownTraveller, was expecting {expectedKnownTravellerCount.Count} but found {actualKnowntraveller} instead.");
                }
            }

            if (!string.IsNullOrEmpty(greencard))
            {
                int actualGreenCardCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ ct:IdentityDoc/ct:IdentityDocTypeCode = 'GC' and  ct:IdentityDoc/ct:Birthdate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Birthdate and ct:IdentityDoc/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ExpiryDate and ct:IdentityDoc/ct:GenderCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GenderCode and ct:IdentityDoc/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GivenName and ct:IdentityDoc/ct:IdentityDocID = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocID and ct:IdentityDoc/ct:IdentityDocTypeCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocTypeCode and ct:IdentityDoc/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssuingCountryCode and ct:IdentityDoc/ct:ResidenceCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ResidenceCountryCode and ct:IdentityDoc/ct:Surname = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Surname]", xmlNamespaceManager).Count;

                if (expectedGreenCardCount.Count != actualGreenCardCount)
                {
                    result.Message.Add($"Incorrect greencard, was expecting {expectedGreenCardCount.Count} but found {actualGreenCardCount} instead.");

                }
            }

            if (!string.IsNullOrEmpty(redress))
            {
                int actualRedressCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:RedressCase/ct:CountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:RedressCase/ct:CountryCode  and   ct:RedressCase/ct:RedressCaseID = ../../../Response/ct:PaxList/ct:Pax/ct:RedressCase/ct:RedressCaseID]", xmlNamespaceManager).Count;

                if (expectedRedressCount.Count != actualRedressCount)
                {
                    result.Message.Add($"Incorrect greencard, was expecting {expectedRedressCount.Count} but found {actualRedressCount} instead.");
                }
            }

            return result.IsValid;
        }

        public static bool OrderChangeRQValidateAPIs(string requestXml, string responseXml, out ValidationResultHelper result, string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", int gstPaxCount = 0)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            XmlNode expectedPaxList = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList", xmlNamespaceManager);
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

            XmlNodeList expectedPassportCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:IdentityDocTypeCode='PT']", xmlNamespaceManager);

            XmlNodeList expectedGreenCardCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:IdentityDocTypeCode='GC']", xmlNamespaceManager);

            XmlNodeList expectedKnownTravellerCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:IdentityDocTypeCode='CR']", xmlNamespaceManager);

            XmlNodeList expectedRedressCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[ct:RedressCase]", xmlNamespaceManager);

            XmlNodeList expectedVisaCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax/ct:IdentityDoc[ct:Visa]", xmlNamespaceManager);

            if (!string.IsNullOrEmpty(passport))
            {
                int actualPassportCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ ct:IdentityDoc/ct:CitizenshipCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:CitizenshipCountryCode and ct:IdentityDoc/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ExpiryDate  and ct:IdentityDoc/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GivenName and ct:IdentityDoc/ct:IdentityDocID = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocID and ct:IdentityDoc/ct:IdentityDocTypeCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocTypeCode and ct:IdentityDoc/ct:IssueDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssueDate and ct:IdentityDoc/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssuingCountryCode and ct:IdentityDoc/ct:ResidenceCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ResidenceCountryCode and ct:IdentityDoc/ct:Surname = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Surname]", xmlNamespaceManager).Count;

                if (expectedPassportCount.Count != actualPassportCount)
                {
                    result.Message.Add($"Incorrect Passport, was expecting {expectedPassportCount.Count} but found {actualPassportCount} instead.");
                }
            }

            if (!string.IsNullOrEmpty(visa))
            {
                int actualVisaCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:IdentityDoc/ct:Visa/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Visa/ct:ExpiryDate  and   ct:IdentityDoc/ct:Visa/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Visa/ct:IssuingCountryCode and ct:IdentityDoc/ct:Visa/ct:HostCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Visa/ct:HostCountryCode]", xmlNamespaceManager).Count;

                if (expectedVisaCount.Count != actualVisaCount)
                {
                    result.Message.Add($"Incorrect Visa, was expecting {expectedVisaCount.Count} but found {actualVisaCount} instead.");
                }
            }

            if (!string.IsNullOrEmpty(knowntraveller))
            {
                int actualKnowntraveller = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:IdentityDoc/ct:IdentityDocTypeCode = 'CR' and ct:IdentityDoc/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ExpiryDate and ct:IdentityDoc/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GivenName and ct:IdentityDoc/ct:IdentityDocID = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocID and ct:IdentityDoc/ct:IdentityDocTypeCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocTypeCode and ct:IdentityDoc/ct:IssueDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssueDate and ct:IdentityDoc/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssuingCountryCode and ct:IdentityDoc/ct:ResidenceCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ResidenceCountryCode and ct:IdentityDoc/ct:Surname = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Surname]", xmlNamespaceManager).Count;

                if (expectedKnownTravellerCount.Count != actualKnowntraveller)
                {
                    result.Message.Add($"Incorrect KnownTraveller, was expecting {expectedKnownTravellerCount.Count} but found {actualKnowntraveller} instead.");
                }
            }

            if (!string.IsNullOrEmpty(greencard))
            {
                int actualGreenCardCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ ct:IdentityDoc/ct:IdentityDocTypeCode = 'GC' and  ct:IdentityDoc/ct:Birthdate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Birthdate and ct:IdentityDoc/ct:ExpiryDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ExpiryDate and ct:IdentityDoc/ct:GenderCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GenderCode and ct:IdentityDoc/ct:GivenName = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:GivenName and ct:IdentityDoc/ct:IdentityDocID = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocID and ct:IdentityDoc/ct:IdentityDocTypeCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IdentityDocTypeCode and ct:IdentityDoc/ct:IssueDate = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssueDate and ct:IdentityDoc/ct:IssuingCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:IssuingCountryCode and ct:IdentityDoc/ct:ResidenceCountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:ResidenceCountryCode and ct:IdentityDoc/ct:Surname = ../../../Response/ct:PaxList/ct:Pax/ct:IdentityDoc/ct:Surname]", xmlNamespaceManager).Count;

                if (expectedGreenCardCount.Count != actualGreenCardCount)
                {
                    result.Message.Add($"Incorrect greencard, was expecting {expectedGreenCardCount.Count} but found {actualGreenCardCount} instead.");
                }
            }

            if (!string.IsNullOrEmpty(redress))
            {
                int actualRedressCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:RedressCase/ct:CountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:RedressCase/ct:CountryCode  and   ct:RedressCase/ct:RedressCaseID = ../../../Response/ct:PaxList/ct:Pax/ct:RedressCase/ct:RedressCaseID]", xmlNamespaceManager).Count;

                if (expectedRedressCount.Count != actualRedressCount)
                {
                    result.Message.Add($"Incorrect greencard, was expecting {expectedRedressCount.Count} but found {actualRedressCount} instead.");
                }
            }
            return result.IsValid;
        }
        public static bool ValidateContactList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList expectedContactInfoList = orderCreateRequestXmlDocument.SelectNodes(
                "msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:ContactInfoList/ct:ContactInfo",
                xmlNamespaceManager);

            foreach (XmlNode expectedContactInfo in expectedContactInfoList)
            {
                string contactInfoIDExpected = expectedContactInfo
                    .SelectSingleNode("ct:ContactInfoID", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                bool isInfantContact = orderCreateRequestXmlDocument.SelectSingleNode(
                    $"msg:IATA_OrderCreateRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax[ct:PTC='INF' and ct:ContactInfoRefID='{contactInfoIDExpected}']",
                    xmlNamespaceManager) != null;

                if (isInfantContact)
                {
                    continue;
                }

                XmlNode actualContactInfo = orderCreateResponseXmlDocument.SelectSingleNode(
                    $"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ContactInfoList/ct:ContactInfo[ct:ContactInfoID='{contactInfoIDExpected}']",
                    xmlNamespaceManager);

                if (actualContactInfo == null)
                {
                    result.Message.Add($"ContactInfoID {contactInfoIDExpected} is missing in response.");
                    continue;
                }

                string emailAddressExpected = expectedContactInfo
                    .SelectSingleNode("ct:EmailAddress/ct:EmailAddressText", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                string emailAddressActual = actualContactInfo
                    .SelectSingleNode("ct:EmailAddress/ct:EmailAddressText", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                if (!emailAddressExpected.Equals(emailAddressActual, StringComparison.OrdinalIgnoreCase))
                {
                    result.Message.Add($"Invalid emailaddress in response, was expecting {emailAddressExpected} but found {emailAddressActual} instead.");
                }

                string contactTypeTextExpected = expectedContactInfo
                    .SelectSingleNode("ct:Phone/ct:ContactTypeText", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                string contactTypeTextActual = actualContactInfo
                    .SelectSingleNode("ct:Phone/ct:ContactTypeText", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                if (!contactTypeTextExpected.Equals(contactTypeTextActual, StringComparison.OrdinalIgnoreCase))
                {
                    result.Message.Add($"Invalid contactTypeText in response, was expecting {contactTypeTextExpected} but found {contactTypeTextActual} instead.");
                }

                string countryDialingCodeExpected = expectedContactInfo
                    .SelectSingleNode("ct:Phone/ct:CountryDialingCode", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                string countryDialingCodeActual = actualContactInfo
                    .SelectSingleNode("ct:Phone/ct:CountryDialingCode", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                if (!countryDialingCodeExpected.Has(countryDialingCodeActual))
                {
                    result.Message.Add($"Invalid countryDialingCode in response, was expecting {countryDialingCodeExpected} but found {countryDialingCodeActual} instead.");
                }

                string phoneNumberExpected = expectedContactInfo
                    .SelectSingleNode("ct:Phone/ct:PhoneNumber", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                string phoneNumberActual = actualContactInfo
                    .SelectSingleNode("ct:Phone/ct:PhoneNumber", xmlNamespaceManager)
                    .InnerText
                    .Parse<string>();

                if (!phoneNumberExpected.Has(phoneNumberActual))
                {
                    result.Message.Add($"Invalid phoneNumber in response, was expecting {phoneNumberExpected} but found {phoneNumberActual} instead.");
                }
            }

            return result.IsValid;
        }
        //PaxList Assertion
        public static bool ValidatePaxList(string requestXml, string responseXml, out ValidationResultHelper result, string gst = "", int gstPaxCount = 0)
        {
            result = new ValidationResultHelper();

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


            if (!string.IsNullOrEmpty(gst) && gstPaxCount > 0 && !(gstPaxCount).Equals(validatedPaxListCount1))
            {
                result.Message.Add($"Invalid PaxList, was expecting {gstPaxCount} but found {validatedPaxListCount1} instead.");
            }

            if (!((expectedPaxListCount + actualPaxListCount) / 2).Equals(validatedPaxListCount))
            {
                result.Message.Add($"Invalid PaxList, was expecting {expectedPaxListCount} but found {validatedPaxListCount} instead.");
            }

            string contactInfoRefID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax/ct:ContactInfoRefID", xmlNamespaceManager).InnerText;

            if (!contactInfoRefID.Equals("CID_1"))
            {
                result.Message.Add($"Invalid contactInfoRefID in paxList, was expecting CID_1 but found {contactInfoRefID} instead.");
            }
            return result.IsValid;
        }

        //OriginDestination List
        public static bool ValidateOriginDestList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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
                result.Message.Add($"Invalid OriginDestinationList, was expecting {expectedOrgDestListCount} but found {validatedOrgDestListCount} instead.");
            }

            int paxJourneys = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest[ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!paxJourneys.Equals(expectedOrgDestListCount))
            {
                result.Message.Add("Incorrect origin destination count");
            }
            return result.IsValid;
        }

        //PaxJourneyList
        public static bool ValidatePaxJourneyList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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
                result.Message.Add($"Invalid PaxJourneys, was expecting {expectedPaxJourneyCount} but found {paxJourneys.Length} instead.");
            }

            bool duration = paxJourneys.All(x => x.All(y => y.SelectSingleNode("ct:Duration", xmlNamespaceManager).InnerText.Equals(null)));

            if (!duration == false)
            {
                result.Message.Add("columnIDRequest requested and colum ID in response are not same");
            }
            return result.IsValid;
        }

        //DatedMarketingAndOperatingList
        public static bool ValidateDatedMarketingAndOperatingList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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
                result.Message.Add($"Invalid datedMarketingSegments, was expecting {datedMarketingSegmentCount} but found {datedMarketingSegments.Length} instead.");
            }

            bool carrierDesigCode = datedMarketingSegments.All(x => x.All(y => y.SelectSingleNode("ct:CarrierDesigCode", xmlNamespaceManager).InnerText.Equals("VS", StringComparison.OrdinalIgnoreCase)));

            if (!carrierDesigCode == true)
            {
                result.Message.Add("columnIDRequest requested and colum ID in response are not same");
            }

            bool carrierName = datedMarketingSegments.All(x => x.All(y => y.SelectSingleNode("ct:CarrierName", xmlNamespaceManager).InnerText.Equals("Virgin Atlantic Airways", StringComparison.OrdinalIgnoreCase)));

            if (!carrierName == true)
            {
                result.Message.Add("columnIDRequest requested and colum ID in response are not same");
            }
            return result.IsValid;
        }

        //PaxSegmentList
        public static bool ValidatePaxSegmentList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int paxSegmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[not(contains(ct:PaxSegmentID, 'OLD'))]", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] paxSegments = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[ct:PaxSegmentID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxSegmentRefID and ct:DatedMarketingSegmentRefId=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:DatedMarketingSegmentId]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxSegmentID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxSegments.Length.Equals(paxSegmentCount))
            {
                result.Message.Add($"Invalid paxSegments, was expecting {paxSegmentCount} but found {paxSegments.Length} instead.");
            }

            XmlNode[] paxSegmentIdCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[not(contains(ct:PaxSegmentID, 'OLD'))]", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            int i = 0;

            foreach (XmlElement element in paxSegmentIdCount)
            {
                i++;

                string segID = element["PaxSegmentID"].InnerText;

                if (!segID.Equals("SEG" + i))
                {
                    result.Message.Add($"paxsegmentId is not as expected SEG{i} actual {segID}");
                }
            }
            return result.IsValid;
        }

        //ServiceDefinitionList
        public static bool ValidateServiceDefinitionList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateRequestXmlDocument.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int serviceDefinitionCout = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).Count;

            IEnumerable<XmlNode> serviceDefinitions = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:ServiceDefinitionAssociation/ct:BaggageAllowanceRef/ct:BaggageAllowanceRefID=../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:Name]", xmlNamespaceManager)
                                                            .OfType<XmlNode>();

            List<string> serviceDefinitionDuplicatesCheck = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager)
                                                                                 .OfType<XmlNode>()
                                                                                 .Select(node => node.SelectSingleNode("ct:ServiceDefinitionID", xmlNamespaceManager)?.InnerText)
                                                                                 .Where(value => !string.IsNullOrEmpty(value) && value.Has("SRV"))
                                                                                 .GroupBy(value => value)
                                                                                 .Where(group => group.Count() > 1)
                                                                                 .Select(group => group.Key)
                                                                                 .ToList();

            if (serviceDefinitionDuplicatesCheck.Count > 1)
            {
                result.Message.Add($"Duplicate serviceDefinitionID found");
            }

            return result.IsValid;
        }

        //OrderItemService
        public static bool ValidateOrderItemIDService(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            int expectedOrderItemCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[1]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            XmlNodeList actualOrderItem = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:FareDetail]", xmlNamespaceManager);

            string orderID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            int segmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int orderItemCounter = 0;

            if (expectedOrderItemCount != actualOrderItem.Count)
            {
                result.Message.Add($"Expected orderItem count{expectedOrderItemCount} but actual {actualOrderItem.Count} ");
            }

            foreach (XmlNode orderItem in actualOrderItem)
            {
                orderItemCounter++;
                string orderItemValue = orderItem.SelectSingleNode("ct:OrderItemID", xmlNamespaceManager).InnerText;
                int paxCount = orderItem.SelectNodes("ct:FareDetail/ct:PaxRefID", xmlNamespaceManager).Count;

                if (!orderItemValue.Has($"{orderID}-{orderItemCounter}"))
                {
                    result.Message.Add($"Expected orderItem count{expectedOrderItemCount} but actual {actualOrderItem.Count} ");
                }

                int totalOrderItemServiceCount = orderItem.SelectNodes("ct:Service", xmlNamespaceManager).Count;

                int segmentServiceCount = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[{orderItemCounter}]/ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:ServiceID]", xmlNamespaceManager).Count;

                int flightServiceCount = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[{orderItemCounter}]/ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:OrderFlightAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:ServiceID and ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:ServiceDefinitionRefID]", xmlNamespaceManager).Count;

                int serviceRefIDcount = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[{orderItemCounter}]/ct:Service[ct:ServiceRefID]", xmlNamespaceManager).Count;

                if (segmentServiceCount != segmentCount * paxCount)
                {
                    result.Message.Add($"Actual segmentServiceCount {segmentServiceCount} Expected segment service count{segmentCount * paxCount}");
                }

                if (flightServiceCount != totalOrderItemServiceCount - segmentServiceCount)
                {
                    result.Message.Add($"Actual flightServiceCount {flightServiceCount} Expected flight services count{segmentCount * paxCount}");
                }

                if (serviceRefIDcount != flightServiceCount)
                {
                    result.Message.Add($"Actual serviceRefID {serviceRefIDcount} Expected ServiceRefID count{segmentCount * paxCount}");
                }
            }

            return result.IsValid;
        }

        //Booking ID
        public static bool ValidateBookingID(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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
                result.Message.Add("Expected all bookings for all segments to be confirmed");
            }
            return result.IsValid;
        }

        //TicketDocument
        public static bool ValidateTicketDocument(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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

            int paxSegmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[not(contains(ct:PaxSegmentID, 'OLD'))]", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] tickets = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:Order/ct:OrderItem/ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID  and  ct:Ticket/ct:Coupon/ct:FareBasisCode]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxRefID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            XmlNodeList aircraftScheduledDateTimeNodes = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime", xmlNamespaceManager);

            string priceClassName = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:Name", xmlNamespaceManager).InnerText;

            List<string> deptAndArrivalDates = new List<string>{};

            foreach (XmlNode node in aircraftScheduledDateTimeNodes)
            {
                if (DateTime.TryParse(node.InnerText, out DateTime dt))
                {
                    node.InnerText = dt.ToString("yyyy-MM-dd");
                    deptAndArrivalDates.Add(node.InnerText);
                }
            }

            XmlNodeList nvanvbExpectedcount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo/ct:Ticket[ct:TicketDocTypeCode='T']/ct:Coupon", xmlNamespaceManager);

            int nvanvbActualcount = 0;
            foreach (XmlNode couponNode in nvanvbExpectedcount)
            {
                string effectiveDate = couponNode.SelectSingleNode("ct:CouponValidityPeriod/ct:EffectiveDate", xmlNamespaceManager)?.InnerText;
                string expirationDate = couponNode.SelectSingleNode("ct:CouponValidityPeriod/ct:ExpirationDate", xmlNamespaceManager)?.InnerText;

                if (!string.IsNullOrEmpty(effectiveDate) && !string.IsNullOrEmpty(expirationDate))
                {
                    if (deptAndArrivalDates.Contains(effectiveDate) && deptAndArrivalDates.Contains(expirationDate))
                    {
                        nvanvbActualcount++;
                    }
                }
            }

            if(nvanvbExpectedcount.Count!=nvanvbActualcount && !priceClassName.Has("Flex"))
            {
                result.Message.Add($"NVA and NVB are not returned in ticket document coupons, actual{nvanvbActualcount} but exepected{nvanvbExpectedcount}");
            }

            if (!expectedTicketDocCount.Equals(tickets.Length) && !priceClassName.Has("Flex"))
            {
                result.Message.Add($"Invalid tickets, actual{tickets.Length} but exepected{expectedTicketDocCount}");
            }

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            bool issueDates = tickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssueDate", xmlNamespaceManager).InnerText.Equals(todaysDate, StringComparison.OrdinalIgnoreCase)));

            if (!issueDates == true)
            {
                result.Message.Add("IssueDate date should be {todaysDate}");
            }

            bool airlineDesigCodes = tickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode", xmlNamespaceManager).InnerText.Equals("VS")));

            if (!airlineDesigCodes == true)
            {
                result.Message.Add("AirlineDesigCode should be VS");
            }

            bool bookletQtys = tickets.All(x => x.All(y => y.SelectSingleNode("ct:BookletQty", xmlNamespaceManager).InnerText.Equals("1")));

            if (!bookletQtys == true)
            {
                result.Message.Add("BookletQty should be 1");
            }

            bool ticketDocTypeCodes = tickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketDocTypeCode", xmlNamespaceManager).InnerText.Equals("T")));

            if (!ticketDocTypeCodes == true)
            {
                result.Message.Add("TicketDocTypeCode should be T");
            }

            bool ticketNumbers = tickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager).InnerText.Equals(null)));

            if (!ticketNumbers == false)
            {
                result.Message.Add("Every ticket doctInfo should have TicketNumber");
            }

            int couponsCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo/ct:Ticket/ct:Coupon[ct:FareBasisCode]", xmlNamespaceManager).Count;

            if (!couponsCount.Equals(expectedTicketDocCount * paxSegmentCount))
            {
                result.Message.Add($"Invalid tickets, actual{couponsCount} but exepected{expectedTicketDocCount}");
            }
            return result.IsValid;
        }

        //BaggaeAllowancelist
        public static bool ValidateBaggaeAllowancelist(string requestXml, string responseXml, out ValidationResultHelper result, string flow = "")
        {
            result = new ValidationResultHelper();

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
                    result.Message.Add($"Invalid Type Code Exepected{typeCode}");
                }

                XmlNodeList maximumDimensionAllowance = baggageAllowance.SelectNodes("ct:MaximumDimensionAllowance[ct:HeightMeasure and ct:LengthMeasure and ct:LengthUnitOfMeasurement and ct:LinearMeasure and ct:WidthMeasure]", xmlNamespaceManager);

                if (maximumDimensionAllowance.Count == 2)
                {
                    XmlNodeList weightAllowance = baggageAllowance.SelectNodes("ct:WeightAllowance[ct:MaximumWeightMeasure and ct:WeightUnitOfMeasurement and ct:TotalMaximumWeightMeasure]", xmlNamespaceManager);

                    if (weightAllowance.Count == 2)
                    {
                        int totalQty = int.Parse(baggageAllowance.SelectSingleNode("ct:PieceAllowance/ct:TotalQty", xmlNamespaceManager).InnerText);

                        int totalMaximumWeightMeasure1 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance/ct:TotalMaximumWeightMeasure", xmlNamespaceManager).InnerText);

                        int totalMaximumWeightMeasure2 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[2]/ct:TotalMaximumWeightMeasure", xmlNamespaceManager).InnerText);

                        int maximumWeightMeasure1 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[1]/ct:MaximumWeightMeasure", xmlNamespaceManager).InnerText);

                        int maximumWeightMeasure2 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[2]/ct:MaximumWeightMeasure", xmlNamespaceManager).InnerText);

                        if (totalQty == 0 && (totalMaximumWeightMeasure1 != 0 || totalMaximumWeightMeasure2 != 0))
                        {
                            result.Message.Add("TotalMaximumWeightMeasure should be 0 as total quantity is 0 ");
                        }

                        if (totalQty > 0 && (totalMaximumWeightMeasure1 != maximumWeightMeasure1 * totalQty))
                        {
                            result.Message.Add("TotalMaximumWeightMeasure1 is not as  expected ");
                        }

                        if (totalQty > 0 && (totalMaximumWeightMeasure2 != maximumWeightMeasure2 * totalQty))
                        {
                            result.Message.Add("TotalMaximumWeightMeasure2 is not as  expected ");
                        }
                    }
                    else
                    {
                        result.Message.Add("weightAllowances are not returned");
                    }
                }
            }

            return result.IsValid;
        }

        //OrderItemIDPrice
        public static bool ValidateOrderItemIdPrice(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int paymentFunctions = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:PaymentFunctions", xmlNamespaceManager).Count;
            double? expectedTotalPrice = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:PaymentFunctions/ct:PaymentProcessingDetails/ct:Amount", xmlNamespaceManager)?.InnerText.Parse<double>();
            double actualTotalPrice = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<double>();

            if (!expectedTotalPrice.Equals(actualTotalPrice) && paymentFunctions > 0)
            {
                result.Message.Add($"Invalid total price, was expecting {expectedTotalPrice} but found {actualTotalPrice} instead.");
            }

            int totalOrderItemCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID/ct:OrderItemID", xmlNamespaceManager).Count;

            string orderID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            int expectedOrderItemCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[1]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            int prices = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price[../ct:Service/ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ../ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID = ../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager).Count;

            int taxCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price/ct:TaxSummary/ct:Tax", xmlNamespaceManager).Count;

            int descText = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price/ct:TaxSummary/ct:Tax/ct:DescText", xmlNamespaceManager).Count;

            int taxCode = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price/ct:TaxSummary/ct:Tax/ct:TaxCode", xmlNamespaceManager).Count;

            if (taxCount != descText || taxCode != taxCount)
            {
                result.Message.Add($"Expected DescText and TaxCode count in Taxsummary {taxCount} but descText {descText} and taxCode {taxCode}.");
            }

            return result.IsValid;
        }

        //OrderItemIDSeat
        public static bool ValidteOrderItemIDSeat(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int expectedSeatCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[2]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            List<string> columnIDRequest = orderCreateRequestXmlDocument.GetElementsByTagName("ColumnID").Cast<XmlNode>()
                                                                            .Select(node => node.InnerText.Trim())
                                                                            .OrderBy(value => value)
                                                                            .ToList();


            List<string> columnIDResponse = orderCreateResponseXmlDocument.GetElementsByTagName("ColumnID").Cast<XmlNode>()
                                                                            .Select(node => node.InnerText.Trim())
                                                                            .OrderBy(value => value)
                                                                            .ToList();

            List<int> rowNumberRequest = orderCreateRequestXmlDocument.GetElementsByTagName("SeatRowNumber").Cast<XmlNode>()
                                                                .Select(node => int.Parse(node.InnerText.Trim()))
                                                                .OrderBy(num => num)
                                                                .ToList();

            List<int> rowNumberResponse = orderCreateResponseXmlDocument.GetElementsByTagName("RowNumber").Cast<XmlNode>()
                                                                .Select(node => int.Parse(node.InnerText.Trim()))
                                                                .OrderBy(num => num)
                                                                .ToList();

            for (int i = 0; i < columnIDRequest.Count; i++)
            {
                if (!columnIDRequest[i].Equals(columnIDResponse[i]))
                {
                    result.Message.Add($"columnIDRequest requested {columnIDRequest[i]} and colum ID in response {columnIDRequest[i]} are not same");
                }

                if (!rowNumberRequest[i].Equals(rowNumberResponse[i]))
                {
                    result.Message.Add($"row number requested {rowNumberRequest[i]} and colum ID in response{rowNumberResponse[i]} are not same");
                }
            }

            IGrouping<string, XmlNode>[] seatOrderItems = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:OrderServiceAssociation/ct:SeatOnLeg/ct:SeatAssignmentAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID =  ../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Service/ct:PaxRefID = ../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager)
                                                           .OfType<XmlNode>()
                                                           .GroupBy(x => x.SelectSingleNode("ct:OrderItemID", xmlNamespaceManager).InnerText)
                                                           .ToArray();

            if (!seatOrderItems.Length.Equals(expectedSeatCount))
            {
                result.Message.Add($"expected seat count{expectedSeatCount} but actual seat order item count {seatOrderItems.Length}");
            }

            bool segmentsConfrimed = seatOrderItems.All(x => x.All(y => y.SelectSingleNode("ct:Service/ct:StatusCode", xmlNamespaceManager).InnerText.Equals("CONFIRMED", StringComparison.OrdinalIgnoreCase)));

            if (!segmentsConfrimed == true)
            {
                result.Message.Add("Expected all bookings for all segments to be confirmed");
            }

            bool serviceIDs = seatOrderItems.All(x => x.All(y => y.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText.Equals(null)));

            if (!serviceIDs == false)
            {
                result.Message.Add("serviceID is expected in all services but not found");
            }
            return result.IsValid;
        }

        //TicketDocumentSeat
        public static bool ValidateTicketDocumentSeat(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int expectedSeatTicketDocCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[2]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] seattickets = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Ticket/ct:TicketDocTypeCode='J']", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!expectedSeatTicketDocCount.Equals(seattickets.Length))
            {
                result.Message.Add($"Invalid tickets, actual{seattickets.Length} but exepected{expectedSeatTicketDocCount}");
            }

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            bool issueDates = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssueDate", xmlNamespaceManager).InnerText.Equals(todaysDate, StringComparison.OrdinalIgnoreCase)));

            if (!issueDates == true)
            {
                result.Message.Add($"IssueDate date should be {todaysDate}");
            }

            bool airlineDesigCodes = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:IssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode", xmlNamespaceManager).InnerText.Equals("VS")));

            if (!airlineDesigCodes == true)
            {
                result.Message.Add("AirlineDesigCode should be VS");
            }

            bool bookletQtys = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:BookletQty", xmlNamespaceManager).InnerText.Equals("1")));

            if (!bookletQtys == true)
            {
                result.Message.Add("BookletQty should be 1");
            }

            bool ticketDocTypeCodes = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketDocTypeCode", xmlNamespaceManager).InnerText.Equals("J")));

            if (!ticketDocTypeCodes == true)
            {
                result.Message.Add("TicketDocTypeCode should be J");
            }

            bool ticketNumbers = seattickets.All(x => x.All(y => y.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager).InnerText.Equals(null)));

            if (!ticketNumbers == false)
            {
                result.Message.Add("Every ticket doctInfo should have TicketNumber");
            }

            return result.IsValid;
        }

        public static bool ValidatePriceClassList(string requestXml, string responseXml, string offerPriceResult, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            XmlDocument offerPriceResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add("priceClassList is not found in response");
            }

            string cabinName = offerPriceResponseXmlDocument.SelectSingleNode("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:Name", xmlNamespaceManager).InnerText;

            string cabinNameResponse = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:Name", xmlNamespaceManager)?.InnerText;

            if (!cabinNameResponse.Equals(cabinName, StringComparison.OrdinalIgnoreCase))
            {
                result.Message.Add("cabin Name returned in response is not as expected");
            }

            List<IGrouping<string, string>> priceClassFareBasisCode = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:FareBasisCode", xmlNamespaceManager).OfType<XmlNode>().Select(x => x.InnerText).GroupBy(x => x).ToList();

            if (!priceClassList.Count.Equals(priceClassFareBasisCode.Count))
            {
                result.Message.Add("cabin Name returned in response is not as expected");
            }

            int paxJourneysCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney", xmlNamespaceManager).Count;

            int segmentCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            int paxCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem", xmlNamespaceManager).Count;

            XmlNodeList fareComponent = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID]/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);

            if (fareComponent.Count != paxJourneysCount * paxCount)
            {
                result.Message.Add("fare component count is not as expected");
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
                    result.Message.Add($"priceClassRefID is incorrect in fare component");
                }

                int cancelRestrictionPriorToDeparture = node.SelectNodes("ct:CancelRestrictions[ct:JourneyStageCode='Prior To Departure']", xmlNamespaceManager).Count;

                int cancelRestrictionAfterDeparture = node.SelectNodes("ct:CancelRestrictions[ct:JourneyStageCode='After Departure']", xmlNamespaceManager).Count;

                int changeRestrictionsPriorToDeparture = node.SelectNodes("ct:ChangeRestrictions[ct:JourneyStageCode='Prior To Departure']", xmlNamespaceManager).Count;

                int changeRestrictionsAfterDeparture = node.SelectNodes("ct:ChangeRestrictions[ct:JourneyStageCode='After Departure']", xmlNamespaceManager).Count;

                int allowedModificationIndCancelRestrictions = node.SelectNodes("ct:CancelRestrictions[ct:AllowedModificationInd='true' and ct:Fee/ct:Amount]", xmlNamespaceManager).Count;

                int allowedModificationIndChangeRestrictions = node.SelectNodes("ct:ChangeRestrictions[ct:AllowedModificationInd='true' and ct:Fee/ct:Amount]", xmlNamespaceManager).Count;

                int allowedModificationIndCancelRestrictions1 = node.SelectNodes("ct:CancelRestrictions[ct:AllowedModificationInd='false']", xmlNamespaceManager).Count;

                int allowedModificationIndChangeRestrictions1 = node.SelectNodes("ct:ChangeRestrictions[ct:AllowedModificationInd='false']", xmlNamespaceManager).Count;

                if (cancelRestrictionPriorToDeparture + cancelRestrictionAfterDeparture + changeRestrictionsPriorToDeparture + changeRestrictionsAfterDeparture + allowedModificationIndChangeRestrictions + allowedModificationIndCancelRestrictions + allowedModificationIndCancelRestrictions1 + allowedModificationIndChangeRestrictions1 != 8)
                {
                    result.Message.Add($"Incorrect cancel and change restrictions inside fare component {node.OuterXml}");
                }

                //valida farebasiscode in farecomponent
                string fareBasisCode = node.SelectSingleNode("ct:FareBasisCode", xmlNamespaceManager).InnerText;

                if (string.IsNullOrEmpty(fareBasisCode))
                {
                    result.Message.Add($"FareBasisCode is incorrect in fare component");
                }

                //validate segments in farecomponent
                XmlNodeList segments = node.SelectNodes("ct:PaxSegmentRefID", xmlNamespaceManager);

                if (segments.Count > 1)
                {
                    foreach (XmlNode node2 in segments)
                    {
                        string segment1 = node2.InnerText;

                        if (!segment1.Has("SEG"))
                        {
                            result.Message.Add("segments are incorrect in fare component");
                        }

                        segmentflag++;
                    }
                    segmentflag--;
                }

                else
                {
                    string segment2 = node.SelectSingleNode("ct:PaxSegmentRefID", xmlNamespaceManager).InnerText;
                    if (!segment2.Equals("SEG" + segmentflag))
                    {
                        result.Message.Add("segments are incorrect in fare component");
                    }
                }

                if (segmentflag == segmentCount)
                    segmentflag = 0;
            }

            return result.IsValid;
        }

        //Airtype code and name validation
        public static bool ValidateAirCarftTypeCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderCreateResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList datedOperatingSegments = OrderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

            foreach (XmlNode datedOperatingSegment in datedOperatingSegments)
            {
                XmlNode AirCraftTypecodeNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeCode", xmlNamespaceManager);

                if (AirCraftTypecodeNode is null)
                {
                    result.Message.Add("AircrafttypeCode node is not found in one of the DatedOperatingSegments");
                }

                XmlNode AircraftTypeNameNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeName", xmlNamespaceManager);

                if (AircraftTypeNameNode is null)
                {
                    result.Message.Add("AircraftTypeName node is not found in one of the DatedOperatingSegments");
                }
            }

            return result.IsValid;
        }

        //Order create SSR validations
        public static bool ValidateServices(string requestXml, string responseXml, string serviceListRS, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument responseXmlDocument = new XmlDocument();
            XmlDocument serviceListResponseXmlDocument = new XmlDocument();

            requestXmlDocument.LoadXml(requestXml);
            responseXmlDocument.LoadXml(responseXml);
            serviceListResponseXmlDocument.LoadXml(serviceListRS);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList actualService = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition/ct:ServiceCode", xmlNamespaceManager);

            XmlNodeList expectedService = requestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer/ct:SelectedOfferItem[ct:SelectedALaCarteOfferItem/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID]", xmlNamespaceManager);

            if (actualService.Count != expectedService.Count)
            {
                result.Message.Add("all requested services are not returned in the service definition list");
            }

            XmlNodeList serviceOrderItemID = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[not(.//ct:FareDetail)]", xmlNamespaceManager);

            if (serviceOrderItemID.Count != actualService.Count)
            {
                result.Message.Add("all requested services order items are not returned in the order");
            }

            int selectedOfferItemCounter = 0;

            foreach (XmlNode item in serviceOrderItemID)
            {
                selectedOfferItemCounter++;

                int segmentCount = requestXmlDocument.SelectNodes($"msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer/ct:SelectedOfferItem[{selectedOfferItemCounter}]/ct:SelectedALaCarteOfferItem/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                int paxCount = requestXmlDocument.SelectNodes($"msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[2]/ct:SelectedOfferItem[{selectedOfferItemCounter}]/ct:PaxRefID", xmlNamespaceManager).Count;

                XmlNodeList actualServiceCount = item.SelectNodes("ct:Service[ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:ServiceDefinitionRefID=../../../ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition/ct:ServiceDefinitionID and ct:BookingRef/ct:BookingEntity/ct:Carrier/ct:AirlineDesigCode='VS' and  ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:StatusCode='CONFIRMED' and ct:ServiceID]", xmlNamespaceManager);

                if (actualServiceCount.Count != segmentCount * paxCount)
                {
                    result.Message.Add("Incorrect pax and segments in the SSR order item");
                }

            }

            return result.IsValid;
        }

        public static bool ValidateOperatingLegOrderCreate(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateTotalMaximumWeightMeasure(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

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
                    result.Message.Add($"Invalid Type Code Exepected{typeCode}");
                }

                XmlNodeList weightAllowance = baggageAllowance.SelectNodes("ct:WeightAllowance[ct:MaximumWeightMeasure and ct:WeightUnitOfMeasurement and ct:TotalMaximumWeightMeasure]", xmlNamespaceManager);

                if (weightAllowance.Count == 2)
                {
                    int totalQty = int.Parse(baggageAllowance.SelectSingleNode("ct:PieceAllowance/ct:TotalQty", xmlNamespaceManager).InnerText);

                    int TotalMaximumWeightMeasure1 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance/ct:TotalMaximumWeightMeasure[1]", xmlNamespaceManager).InnerText);

                    int TotalMaximumWeightMeasure2 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[2]/ct:TotalMaximumWeightMeasure", xmlNamespaceManager).InnerText);

                    int maximumWeightMeasure1 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[1]/ct:MaximumWeightMeasure", xmlNamespaceManager).InnerText);

                    int maximumWeightMeasure2 = int.Parse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[2]/ct:MaximumWeightMeasure", xmlNamespaceManager).InnerText);

                    if (totalQty == 0 && (TotalMaximumWeightMeasure1 != 0 || TotalMaximumWeightMeasure2 != 0))
                    {
                        result.Message.Add("TotalMaximumWeightMeasure should be 0 as total quantity is 0 ");
                    }

                    if (TotalMaximumWeightMeasure1 != maximumWeightMeasure1 * totalQty && totalQty > 0)
                    {
                        result.Message.Add("TotalMaximumWeightMeasure1 is not as  expected ");
                    }

                    if (TotalMaximumWeightMeasure2 != maximumWeightMeasure2 * totalQty && totalQty > 0)
                    {
                        result.Message.Add("TotalMaximumWeightMeasure2 is not as  expected ");
                    }
                }

            }

            return result.IsValid;
        }

        public static bool ValidateOrderStatus(string requestXml, string responseXml, out ValidationResultHelper result, string flow = "")
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateRequestXmlDocument.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            int serviceExpectedCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Service", xmlNamespaceManager).Count;

            int readyToProceedCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Service/ct:DeliveryStatusCode[text()='READY TO PROCEED']", xmlNamespaceManager).Count;

            int confirmedCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Service/ct:DeliveryStatusCode[text()='CONFIRMED']", xmlNamespaceManager).Count;

            int expectedOrderItemStatusCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem", xmlNamespaceManager).Count;

            int actualOrderItemStatusCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:DeliveryStatusCode='CONFIRMED' or ct:Service/ct:DeliveryStatusCode='READY TO PROCEED' and ct:StatusCode='ACTIVE']", xmlNamespaceManager).Count;

            string orderStatus = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:StatusCode", xmlNamespaceManager)?.InnerText;

            int removedStatusCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Service/ct:DeliveryStatusCode[text()='REMOVED']", xmlNamespaceManager).Count;

            int cancelledOrderItemStatus = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:DeliveryStatusCode='REMOVED' or ct:Service/ct:DeliveryStatusCode='CANCELLED' and ct:StatusCode='CLOSED']", xmlNamespaceManager).Count;

            if (flow.Equals("HoldBooking") && serviceExpectedCount != confirmedCount)
            {
                result.Message.Add("Delivery status code in all services is not returned as confirmed Hold booking flow ");
            }

            if (flow.Equals("Cancel") && serviceExpectedCount != removedStatusCount)
            {
                result.Message.Add("Delivery status code in all services is not returned as confirmed Hold booking flow ");
            }

            if (flow.Equals("InstantPurchase") && serviceExpectedCount != readyToProceedCount)
            {
                result.Message.Add("Delivery status code in all services is not returned as reday to proceed instant purchase flow ");
            }

            if (actualOrderItemStatusCount != expectedOrderItemStatusCount && (flow.Is("HoldBooking") || flow.Is("InstantPurchase")))
            {
                result.Message.Add("Order Item Status is not returned as active for all order items");
            }

            if (cancelledOrderItemStatus != expectedOrderItemStatusCount && flow.Is("Cancel"))
            {
                result.Message.Add("Order Item Status is not returned as active for all order items");
            }

            if ((flow.Is("HoldBooking") || flow.Is("InstantPurchase")) && !orderStatus.Is("OPENED"))
            {
                result.Message.Add("Order Status is not returned as active ");
            }

            if (flow.Is("Cancel") && !orderStatus.Is("CLOSED"))
            {
                result.Message.Add("Order Status is not returned as active ");
            }

            return result.IsValid;
        }
        public static bool ValidatePaymentFunctions(string requestXml, string responseXml, out ValidationResultHelper result, string paymentType = "",string accountNumber="")
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            // Get values from first SelectedOfferItem in request
            string reqTotalAmount = orderCreateRequestXmlDocument.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentProcessingDetails//ct:Amount", xmlNamespaceManager).InnerText;
            string agentID = orderCreateRequestXmlDocument.SelectSingleNode("//msg:DistributionChain/ct:DistributionChainLink[1]/ct:ParticipatingOrg/ct:OrgID", xmlNamespaceManager)?.InnerText;

            // Get all OrderItems with FareDetail (tickets)
            XmlNodeList orderItemNodes = orderCreateResponseXmlDocument.SelectNodes("//ct:Order/ct:OrderItem[ct:FareDetail]", xmlNamespaceManager);
            string orderID = orderCreateResponseXmlDocument.SelectSingleNode("//ct:Order/ct:OrderID", xmlNamespaceManager)?.InnerText;
            string totalAmount = orderCreateResponseXmlDocument.SelectSingleNode("//ct:Order/ct:TotalPrice/ct:TotalAmount", xmlNamespaceManager)?.InnerText;
            XmlNodeList paymentFunctionsNodes = orderCreateResponseXmlDocument.SelectNodes("//msg:PaymentFunctions", xmlNamespaceManager);

            if (paymentFunctionsNodes == null || paymentFunctionsNodes.Count == 0)
            {
                result.Message.Add($"No PaymentFunctions found in response. OrderID: {orderID}");
                return false;
            }

            foreach (XmlNode paymentFunction in paymentFunctionsNodes)
            {
                if (paymentType.Is("CA"))
                {
                    string iATA_Number = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummary/ct:PaymentProcessingSummaryPaymentMethod/ct:SettlementPlan/ct:IATA_Number", xmlNamespaceManager)?.InnerText;

                    if (!iATA_Number.Is(agentID))
                    {
                        result.Message.Add($"Incorrect IATA number returned in payment Function Expected -> {agentID} but actual is {iATA_Number}");
                    }

                    string paymentTypeCode = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummary/ct:PaymentProcessingSummaryPaymentMethod/ct:SettlementPlan/ct:PaymentTypeCode", xmlNamespaceManager)?.InnerText;

                    if (!paymentTypeCode.Is("CA"))
                    {
                        result.Message.Add($"Incorrect PaymentTypeCode returned in payment Function Expected -> {"CA"} but actual is {paymentTypeCode}");
                    }

                }

                if (paymentType.Is("EasyPay"))
                {
                    string approvalCode = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummary/ct:PaymentProcessingSummaryPaymentMethod/ct:IATA_EasyPay/ct:ApprovalCode", xmlNamespaceManager)?.InnerText;

                    if (string.IsNullOrEmpty(approvalCode))
                    {
                        result.Message.Add("Approval code is not returned payment Function Expected");
                    }
                }

                // 1. OrderItemRefID
                string orderItemRefID = paymentFunction.SelectSingleNode(".//ct:OrderAssociation/ct:OrderItemRefID", xmlNamespaceManager)?.InnerText;
                bool orderItemExists = false;
                foreach (XmlNode orderItem in orderItemNodes)
                {
                    string orderItemID = orderItem.SelectSingleNode("ct:OrderItemID", xmlNamespaceManager)?.InnerText;
                    if (orderItemID == orderItemRefID)
                    {
                        orderItemExists = true;
                        break;
                    }
                    orderItemExists = orderCreateResponseXmlDocument.SelectNodes($"//ct:Order/ct:OrderItem[ct:OrderItemID='{orderItemRefID}']", xmlNamespaceManager).Count > 0;
                }
                if (!orderItemExists)
                {
                    result.Message.Add($"OrderItemRefID {orderItemRefID} in PaymentFunctions not found in OrderItems.");
                }

                // 2. OrderRefID
                string orderRefID = paymentFunction.SelectSingleNode(".//ct:OrderAssociation/ct:OrderRefID", xmlNamespaceManager)?.InnerText;
                if (orderRefID != orderID)
                {
                    result.Message.Add($"OrderRefID {orderRefID} in PaymentFunctions does not match OrderID {orderID}.");
                }

                // 3. Amount
                string amount = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummary/ct:Amount", xmlNamespaceManager)?.InnerText;
                if (amount != reqTotalAmount && amount != totalAmount)
                {
                    result.Message.Add($"Amount {amount} in PaymentFunctions does not match expected {reqTotalAmount ?? totalAmount}.");
                }

                // 4. PaymentCommitmentDateTime
                string paymentCommitmentDateTime = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummary/ct:PaymentCommitmentDateTime", xmlNamespaceManager)?.InnerText;
                if (!string.IsNullOrEmpty(paymentCommitmentDateTime))
                {
                    if (!DateTime.TryParse(paymentCommitmentDateTime, out DateTime paymentDate))
                    {
                        result.Message.Add($"PaymentCommitmentDateTime {paymentCommitmentDateTime} is not a valid date.");
                    }
                    else
                    {
                        DateTime today = DateTime.UtcNow.Date;
                        if (paymentDate.Date != today)
                        {
                            result.Message.Add($"PaymentCommitmentDateTime {paymentCommitmentDateTime} is not today's date (UTC).");
                        }
                    }
                }
                else
                {
                    result.Message.Add("PaymentCommitmentDateTime is missing in PaymentFunctions.");
                }

                if(!(paymentType.Is("Cash")|| paymentType.Is("EasyPay")))
                {
                    string reqCardBrandCode = orderCreateRequestXmlDocument.SelectSingleNode("//msg:IATA_OrderCreateRQ/msg:Request/ct:PaymentFunctions/ct:PaymentProcessingDetails/ct:PaymentMethod/ct:PaymentCard/ct:CardTypeText", xmlNamespaceManager)?.InnerText;
                    string reqExpirationDate = orderCreateRequestXmlDocument.SelectSingleNode("//msg:IATA_OrderCreateRQ/msg:Request/ct:PaymentFunctions/ct:PaymentProcessingDetails/ct:PaymentMethod/ct:PaymentCard/ct:ExpirationDate", xmlNamespaceManager)?.InnerText;
                    // 5. CardBrandCode
                    string cardBrand = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummaryPaymentMethod/ct:PaymentCard/ct:CardBrandCode", xmlNamespaceManager)?.InnerText;
                    if (!cardBrand.Is(reqCardBrandCode))
                    {
                        result.Message.Add($"CardBrandCode {cardBrand} in PaymentFunctions does not match expected {reqCardBrandCode}.");
                    }
                }

                // 7. PaymentStatusCode
                string paymentStatus = paymentFunction.SelectSingleNode(".//ct:PaymentProcessingSummary/ct:PaymentStatusCode", xmlNamespaceManager)?.InnerText;
                if (!paymentStatus.Is("SUCCESSFUL"))
                {
                    result.Message.Add($"PaymentStatusCode {paymentStatus} in PaymentFunctions is not SUCCESSFUL.");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateTicketNumberWithLeastAmount(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            double minPrice = double.MaxValue;
            int minPriceIndex = -1;

            XmlNodeList orderItemNodes = orderCreateResponseXmlDocument.SelectNodes("//ct:OrderItem", xmlNamespaceManager);
            XmlNodeList ticketDocInfoNodes = orderCreateResponseXmlDocument.SelectNodes("//msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo", xmlNamespaceManager);

            if (orderItemNodes.Count != ticketDocInfoNodes.Count)
            {
                // Handle the case where the number of order items does not match the number of ticket doc info nodes
                result.Message.Add("Mismatch between the number of OrderItems and TicketDocInfo nodes.");
            }

            for (int i = 0; i < orderItemNodes.Count; i++)
            {
                XmlNode orderItemNode = orderItemNodes[i];
                XmlNode totalAmountNode = orderItemNode.SelectSingleNode("ct:Price/ct:TotalAmount", xmlNamespaceManager);
                if (totalAmountNode == null)
                {
                    continue;
                }

                double totalAmount = double.Parse(totalAmountNode.InnerText);

                if (totalAmount < minPrice)
                {
                    minPrice = totalAmount;
                    minPriceIndex = i;
                }
            }

            if (minPriceIndex >= 0)
            {
                XmlNode ticketDocInfoNode = ticketDocInfoNodes[minPriceIndex];
                XmlNode ticketNode = ticketDocInfoNode.SelectSingleNode("ct:Ticket", xmlNamespaceManager);

                if (ticketNode != null)
                {
                    string minPriceTicketNumber = ticketNode.InnerText;
                    return result.IsValid;
                }
                else
                {
                    // Handle case where corresponding TicketDocInfo does not have a Ticket node
                    result.Message.Add("Corresponding TicketDocInfo found but does not contain a Ticket node.");
                }
            }
            else
            {
                // Handle case where no minimum price was found
                result.Message.Add("No minimum price found among the OrderItems.");
            }
            return result.IsValid;
        }

        public static bool ValidateTicketNumberWithLeastAmountMulticard(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            orderCreateResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            double minPrice = double.MaxValue;
            int minPriceIndex = -1;

            XmlNodeList orderItemNodes = orderCreateResponseXmlDocument.SelectNodes("//ct:OrderItem", xmlNamespaceManager);
            XmlNodeList ticketDocInfoNodes = orderCreateResponseXmlDocument.SelectNodes("//msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo", xmlNamespaceManager);
            XmlNodeList paxNodes = orderCreateResponseXmlDocument.SelectNodes("//ct:pax", xmlNamespaceManager);
            XmlNodeList paymentFunctionNodes = orderCreateResponseXmlDocument.SelectNodes("//msg:PaymentFunctions", xmlNamespaceManager);

            if (orderItemNodes == null || ticketDocInfoNodes == null || paxNodes == null || paymentFunctionNodes == null)
            {
                result.Message.Add("XML structure is incorrect or namespaces are not properly defined.");
            }

            int paxCount = paxNodes.Count;

            if (orderItemNodes.Count + paxCount != ticketDocInfoNodes.Count)
            {
                result.Message.Add("Mismatch between the number of adjusted OrderItems and TicketDocInfo nodes.");
            }

            for (int i = 0; i < orderItemNodes.Count; i++)
            {
                XmlNode orderItemNode = orderItemNodes[i];
                XmlNode totalAmountNode = orderItemNode.SelectSingleNode("ct:Price/ct:TotalAmount", xmlNamespaceManager);
                if (totalAmountNode == null)
                {
                    continue;
                }

                if (!double.TryParse(totalAmountNode.InnerText, out double totalAmount))
                {
                    result.Message.Add($"Invalid total amount in OrderItem at index {i}.");
                }

                if (totalAmount < minPrice)
                {
                    minPrice = totalAmount;
                    minPriceIndex = i + paxCount;
                }
            }

            if (minPriceIndex >= 0 && minPriceIndex < ticketDocInfoNodes.Count)
            {
                XmlNode ticketDocInfoNode = ticketDocInfoNodes[minPriceIndex];
                XmlNode ticketNode = ticketDocInfoNode.SelectSingleNode("ct:Ticket/ct:TicketNumber", xmlNamespaceManager);

                if (ticketNode != null)
                {
                    string minPriceTicketNumber = ticketNode.InnerText;

                    // Fetch the PaymentFunction value
                    string paymentID = string.Empty;
                    string amount = string.Empty;

                    for (int i = 0; i < paymentFunctionNodes.Count; i++)
                    {
                        XmlNode paymentFunctionNode = paymentFunctionNodes[i];
                        XmlNode paymentIDNode = paymentFunctionNode.SelectSingleNode(".//ct:PaymentID", xmlNamespaceManager);
                        XmlNode amountNode = paymentFunctionNode.SelectSingleNode(".//ct:Amount", xmlNamespaceManager);

                        paymentID = paymentIDNode?.InnerText ?? "N/A";
                        amount = amountNode?.InnerText ?? "N/A";

                        // Check if PaymentID contains minPriceTicketNumber
                        if (paymentID.Has(minPriceTicketNumber))
                        {
                            result.Message.Add($"The ticket number with the least amount is: {minPriceTicketNumber}, Payment ID: {paymentID}, Amount: {amount}");
                            return result.IsValid;
                        }
                        continue;
                    }
                }
                else
                {
                    result.Message.Add("Corresponding TicketDocInfo found but does not contain a Ticket node.");
                }
            }
            else
            {
                result.Message.Add("No minimum price found among the OrderItems.");
            }
            return result.IsValid;
        }
        public static bool ValidateCardNumber(string CardNumber, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            string message;
            bool isValid = LuhnValidator.IsValidLuhn(CardNumber, out message);

            // Update ValidationResultHelper with the validation outcome
            if (!isValid)
            {
                result.Message.Add(message);
            }

            return result.IsValid;
        }

        public static bool ValidateOrderChangeSeatPrice(string requestXml, string responseXml, string changeResponseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderChangeRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            XmlDocument orderChangeSeatResponseXmlDocument = new XmlDocument();

            orderChangeRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = orderChangeRequestXmlDocument.CreateXmlNamespaceManager();

            orderChangeRequestXmlDocument.LoadXml(requestXml);
            orderCreateResponseXmlDocument.LoadXml(responseXml);
            orderChangeSeatResponseXmlDocument.LoadXml(changeResponseXml);

            double? expectedSeatPrice = orderChangeRequestXmlDocument.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:PaymentFunctions/ct:PaymentProcessingDetails/ct:Amount", xmlNamespaceManager)?.InnerText.Parse<double>();
            double expectedTicketPrice = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<double>();
            double actualtotalPrice = orderChangeSeatResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<double>();

            if (actualtotalPrice != expectedSeatPrice + expectedTicketPrice)
            {
                result.Message.Add($"Invalid total price, was expecting {actualtotalPrice} but found {expectedSeatPrice + expectedTicketPrice} instead.");
            }

            return result.IsValid;
        }

        public static bool ValidateChildPnrsInParentRetrieveRS(string responseXml, int splitCount, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            List<XmlNode> childOrderIds = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:AugmentationPoint/msg:Order/msg:OrderID", xmlNamespaceManager)?.Cast<XmlNode>()
                                                                                                                                                 .GroupBy(node => node.InnerText)
                                                                                                                                                 .Select(group => group.First())
                                                                                                                                                 .ToList();

            List<XmlNode> childPnrs = responseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:AugmentationPoint/msg:Order/msg:BookingRef/msg:BookingID", xmlNamespaceManager)?.Cast<XmlNode>()
                                                                                                                                                          .GroupBy(node => node.InnerText)
                                                                                                                                                          .Select(group => group.First())
                                                                                                                                                          .ToList();

            if (childOrderIds.Count != splitCount || childPnrs.Count != splitCount)
            {
                result.Message.Add($"Invalid number of child PNRs, was expecting {splitCount} but found {childOrderIds.Count} instead.");
            }


            return result.IsValid;
        }

        //OrderItemIDSeat
        public static bool ValidateSeatRetrieve(string creaateRS, string retrieveRS, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderRetrieveRSXmlDocument = new XmlDocument();
            XmlDocument orderCreateRSXmlDocument = new XmlDocument();

            orderCreateRSXmlDocument.LoadXml(creaateRS);
            orderRetrieveRSXmlDocument.LoadXml(retrieveRS);

            XmlNamespaceManager xmlNamespaceManager = orderCreateRSXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList createSeatOrdrItems = orderCreateRSXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:OrderServiceAssociation/ct:SeatOnLeg/ct:SeatAssignmentAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID =  ../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Service/ct:PaxRefID = ../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager);

            XmlNodeList retrieveSeatOrderItems = orderRetrieveRSXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:OrderServiceAssociation/ct:SeatOnLeg/ct:SeatAssignmentAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID ]", xmlNamespaceManager);

            if (retrieveSeatOrderItems.Count == 0)
            {
                result.Message.Add($"Seats are not returned in retrieve response Expected {createSeatOrdrItems.Count} but actual is 0.");
            }

            foreach (XmlNode node in retrieveSeatOrderItems)
            {
                string retrieveSeatItem = node.InnerText;

                if (!orderCreateRSXmlDocument.InnerText.Contains(retrieveSeatItem))
                {
                    result.Message.Add($"Incorrect seat order item in retrieve {node.InnerText}");
                }

                node.ParentNode.RemoveChild(node);

            }

            return result.IsValid;
        }

    }
}
