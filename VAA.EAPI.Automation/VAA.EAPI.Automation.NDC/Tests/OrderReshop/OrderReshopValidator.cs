using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.NDC.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.OrderReshop
{
    public class OrderReshopValidator
    {
        public static bool ValidateUpdatePaxnameAPIsWarning(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList warning = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:Warning[ct:DescText='Name Correction will remove your APIS information from the PNR. Please resubmit APIS after the correction' and ct:Code='142' and ct:LangCode='EN' and ct:TagText='Warning - APIS information will be removed, if Order Change is requested for Name Correction' and ct:TypeCode='Processed']", xmlNamespaceManager);

            if (warning.Count==0)
            {
                result.Message.Add($"incorrect APIs warning returned in reshop response {responseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:Warning",xmlNamespaceManager)?.InnerText}");
            }

            return result.IsValid;
        }

        public static bool ValidateOffersReturned(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopResponseXmlDocument.LoadXml(responseXml);

            int expectedserviceCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers", xmlNamespaceManager).Count;

            if (expectedserviceCount == 0)
            {
                result.Message.Add($"{responseXml}");

            }

            return result.IsValid;
        }

        public static bool ValidateOperatingLeg(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID ]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");

                }
            }

            return result.IsValid;
        }

        public static bool ValidatePaxList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocumentAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager reshopXmlnsManager = new XmlNamespaceManager(xmlDocumentAirShoppingRequest.NameTable);
            reshopXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            reshopXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            xmlDocumentAirShoppingRequest.LoadXml(requestXml);

            string paxListBlockRequest = xmlDocumentAirShoppingRequest.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/def:PaxList", reshopXmlnsManager)?.OuterXml;

            int? paxListBlockInfants = xmlDocumentAirShoppingRequest.SelectNodes("/msg:IATA_AirShoppingRQ/msg:Request/def:PaxList/def:Pax/def:PaxID[contains(text(), 'INFANT')]", reshopXmlnsManager)?.Count;

            XmlDocument xmlDocumentReshopResponse = new XmlDocument();

            xmlDocumentReshopResponse.LoadXml(responseXml);

            string paxListBlockResponse = xmlDocumentReshopResponse.SelectSingleNode("/msg:IATA_OrderReshopRS/msg:Response/def:DataLists/def:PaxList", reshopXmlnsManager)?.OuterXml;

            XmlNodeList paxListRefIds = xmlDocumentReshopResponse.SelectNodes("/msg:IATA_OrderReshopRS/msg:Response/def:DataLists/def:PaxList/def:Pax/def:PaxRefID", reshopXmlnsManager);

            if (paxListBlockInfants > 0)
            {
                if (paxListRefIds?.Count != paxListBlockInfants)
                {
                    result.Message.Add($"Count of Infants within Adult Pax is not as expected. Expected: {paxListBlockInfants} and returned: {xmlDocumentReshopResponse.SelectNodes("/msg:IATA_OrderReshopRS/msg:Response/def:DataLists/def:PaxList/def:Pax/def:PaxRefID", reshopXmlnsManager)?.Count}");

                }

                for (int i = paxListRefIds.Count - 1; i >= 0; i--)
                    paxListRefIds[i].ParentNode.RemoveChild(paxListRefIds[i]);

                string amendedPaxListResponse = xmlDocumentReshopResponse.SelectSingleNode("/msg:IATA_OrderReshopRS/msg:Response/def:DataLists/def:PaxList", reshopXmlnsManager)?.OuterXml; ;
                if (paxListBlockRequest != amendedPaxListResponse)
                {
                    result.Message.Add($"Pax List from AirShopping request: {paxListBlockRequest} did not match Pax List in Reshop response: {amendedPaxListResponse}");

                }
            }
            else
            {
                if (paxListBlockRequest != paxListBlockResponse)
                {
                    result.Message.Add($"Pax List from AirShopping request: {paxListBlockRequest} did not match Pax List in Reshop response: {paxListBlockResponse}");

                }
            }
            return result.IsValid;
        }

        public static bool ValidateCabinTypeFromAirShoppingRequest(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocumentAirShoppingRequest = new XmlDocument();

            xmlDocumentAirShoppingRequest.LoadXml(requestXml);

            XmlNamespaceManager reshopXmlnsManager = xmlDocumentAirShoppingRequest.CreateXmlNamespaceManager();

            string cabinTypeValueFromRequest = xmlDocumentAirShoppingRequest.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/def:FlightRequest/def:FlightRequestOriginDestinationsCriteria/def:OriginDestCriteria/def:CabinType/def:CabinTypeCode", reshopXmlnsManager)?.InnerText;

            XmlDocument xmlDocumentReshopResponse = new XmlDocument();

            xmlDocumentReshopResponse.LoadXml(responseXml);

            XmlNodeList cabinTypeResponseFilteredCount = xmlDocumentReshopResponse.SelectNodes($"/msg:IATA_OrderReshopRS/msg:Response/def:DataLists/def:PaxSegmentList/def:PaxSegment/def:CabinTypeAssociationChoice/def:SegmentCabinType/def:CabinTypeCode[.='{cabinTypeValueFromRequest}']", reshopXmlnsManager);

            if (cabinTypeResponseFilteredCount?.Count == 0)
            {
                result.Message.Add($"Cabin Type value from AirShopping Request: {cabinTypeValueFromRequest} was not returned for all CabinTypeCode nodes in the Reshop response being {cabinTypeResponseFilteredCount?.Count}");

            }

            return result.IsValid;
        }

        public static bool ValidateCabinTypeFromOrderReshopRequest(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocumentReshopRequest = new XmlDocument();

            XmlNamespaceManager reshopXmlnsManager = new XmlNamespaceManager(xmlDocumentReshopRequest.NameTable);
            reshopXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            reshopXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            xmlDocumentReshopRequest.LoadXml(requestXml);

            string cabinTypeValueFromRequest = xmlDocumentReshopRequest.SelectSingleNode("/msg:IATA_OrderReshopRQ/msg:Request/def:UpdateOrder/def:ReshopOrder/def:ReshopOrderChoice/def:ServiceOrder/def:AddOfferItems/def:FlightRequest/def:FlightRequestOriginDestinationsCriteria/def:OriginDestCriteria/def:CabinType/def:CabinTypeCode", reshopXmlnsManager)?.InnerText;

            XmlDocument xmlDocumentReshopResponse = new XmlDocument();

            xmlDocumentReshopResponse.LoadXml(responseXml);

            XmlNodeList cabinTypeResponseFilteredCount = xmlDocumentReshopResponse.SelectNodes($"/msg:IATA_OrderReshopRS/msg:Response/def:DataLists/def:PaxSegmentList/def:PaxSegment/def:CabinTypeAssociationChoice/def:SegmentCabinType/def:CabinTypeCode[.='{cabinTypeValueFromRequest}']", reshopXmlnsManager);

            if (cabinTypeResponseFilteredCount?.Count == 0)
            {
                result.Message.Add($"Cabin Type value from Reshop Request: {cabinTypeValueFromRequest} was not returned for all CabinTypeCode nodes in the Reshop response being {cabinTypeResponseFilteredCount?.Count}");

            }
            return result.IsValid;
        }

        public static bool ValidateOriginDestList(string requestXml, string responseXml, out ValidationResultHelper result, string cityCode = "")
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = reshopRequestXmlDocument.CreateXmlNamespaceManager();

            XmlDocument orgDestListXmlDocument = new XmlDocument();

            XmlNode expectedOrgDestList = reshopRequestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria", xmlNamespaceManager);
            XmlNode actualOrgDestList = reshopResponseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:OriginDestList", xmlNamespaceManager);

            XmlElement requestOrgDestList = orgDestListXmlDocument.CreateElement("Request");
            XmlElement responseOrgDestList = orgDestListXmlDocument.CreateElement("Response");

            requestOrgDestList.AppendChild(orgDestListXmlDocument.ImportNode(expectedOrgDestList.CloneNode(true), true));
            responseOrgDestList.AppendChild(orgDestListXmlDocument.ImportNode(actualOrgDestList.CloneNode(true), true));

            XmlElement datalistXmlNode = orgDestListXmlDocument.CreateElement("Datalist");

            datalistXmlNode.AppendChild(requestOrgDestList);
            datalistXmlNode.AppendChild(responseOrgDestList);

            orgDestListXmlDocument.AppendChild(datalistXmlNode);

            int? expectedOrgDestListCount = expectedOrgDestList?.SelectNodes("ct:OriginDestCriteria", xmlNamespaceManager)?.Count;
            int? actualOrgDestListCount = actualOrgDestList?.SelectNodes("ct:OriginDest", xmlNamespaceManager)?.Count;
            int? validatedOrgDestListCount = orgDestListXmlDocument?.SelectNodes("Datalist/Request/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[ct:OriginDepCriteria/ct:IATA_LocationCode = ../../../Response/ct:OriginDestList/ct:OriginDest/ct:OriginCode and ct:DestArrivalCriteria/ct:IATA_LocationCode = ../../../Response/ct:OriginDestList/ct:OriginDest/ct:DestCode]", xmlNamespaceManager)?.Count;

            if (!((expectedOrgDestListCount + actualOrgDestListCount) / 2).Equals(validatedOrgDestListCount) && actualOrgDestListCount < expectedOrgDestListCount)
            {
                result.Message.Add($"Invalid Origin Destination list, was expecting {expectedOrgDestListCount} but found {validatedOrgDestListCount} instead.");

            }

            int? paxJourneys = reshopResponseXmlDocument?.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest[ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID ]", xmlNamespaceManager)?.Count;

            if (!paxJourneys.Equals(expectedOrgDestListCount) && actualOrgDestListCount < expectedOrgDestListCount)
            {
                result.Message.Add($"Journey ID in Origin DestList is not found in PaxJOurneyList");

            }

            if (expectedOrgDestListCount != actualOrgDestListCount && actualOrgDestListCount < expectedOrgDestListCount)
            {
                result.Message.Add($"Incorrect origin destination list in the response");

            }

            return result.IsValid;
        }

        public static bool ValidatePaxJourneylist(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            int? expectedPaxJourneyCount = reshopResponseXmlDocument?.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney", xmlNamespaceManager)?.Count;

            IGrouping<string, XmlNode>[] paxJourneys = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney[ct:PaxJourneyID=../../../ct:DataLists/ct:OriginDestList/ct:OriginDest/ct:PaxJourneyRefID and ct:PaxSegmentRefID=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxJourneyID", xmlNamespaceManager)?.InnerText)
                                                            .ToArray();

            if (!paxJourneys.Length.Equals(expectedPaxJourneyCount))
            {
                result.Message.Add($"Invalid PaxJourneys, was expecting {expectedPaxJourneyCount} but found {paxJourneys.Length} instead.");

            }
            return result.IsValid;
        }

        public static bool ValidateDatedMarketingAndOperatingList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] datedMarketingSegments = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[ct:DatedMarketingSegmentId=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:DatedMarketingSegmentRefId and ct:DatedOperatingSegmentRefId=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingSegmentId and ct:Arrival/ct:IATA_LocationCode != ct:Dep/ct:IATA_LocationCode  and ct:Arrival/ct:AircraftScheduledDateTime != ct:Dep/ct:AircraftScheduledDateTime]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:DatedMarketingSegmentId", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!datedMarketingSegments.Length.Equals(datedMarketingSegmentCount))
            {
                result.Message.Add($"Invalid datedMarketingSegments, was expecting {datedMarketingSegmentCount} but found {datedMarketingSegments.Length} instead.");

            }
            return result.IsValid;
        }

        public static bool ValidateNameChangeReshopRS(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = reshopResponseXmlDocument.CreateXmlNamespaceManager();

            string paxRefID = reshopRequestXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:UpdatePaxName/ct:PaxRefID", xmlNamespaceManager)?.InnerText;

            //validate update pax name 
            string updatePaxNameRS = reshopResponseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferItem/ct:PaxNameChange", xmlNamespaceManager)?.InnerText;

            string updatePaxNameRQ = reshopRequestXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:UpdatePaxName", xmlNamespaceManager)?.InnerText;

            if (updatePaxNameRS!=updatePaxNameRQ)
            {
                result.Message.Add($"update pax name in response is not correct expected {updatePaxNameRQ} but found {updatePaxNameRS} instead.");

            }

            //validate servicedefinition
            int nameChangeCount = reshopResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:Name='Name Change' and ct:OwnerCode='VS' and ct:ServiceDefinitionID='SRVDEF1_NAMCHG']", xmlNamespaceManager).Count;

            if (nameChangeCount != 1)
            {
                result.Message.Add($"Incorrect service definition details returned in the response {reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).OuterXml}");
            }

            //validate offerExpirationTimeLimitDateTime
            string offerExpirationTimeLimitDateTime = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferExpirationTimeLimitDateTime", xmlNamespaceManager).InnerText;

            if (!DateTime.TryParseExact(offerExpirationTimeLimitDateTime, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime expirationDateTime))
            {
                result.Message.Add($"OfferExpirationTimeLimitDateTime '{offerExpirationTimeLimitDateTime}' could not be parsed as a valid UTC date/time.");
                return false;
            }

            DateTime nowUtc = DateTime.UtcNow;
            if (expirationDateTime.Date != nowUtc.Date)
            {
                result.Message.Add($"OfferExpirationTimeLimitDateTime date '{expirationDateTime.Date:yyyy-MM-dd}' is not today '{nowUtc.Date:yyyy-MM-dd}'.");
                return false;
            }

            if (expirationDateTime <= nowUtc.AddMinutes(30).AddSeconds(-nowUtc.Second))
            {
                result.Message.Add($"OfferExpirationTimeLimitDateTime '{offerExpirationTimeLimitDateTime}' is not at least 30 minutes ahead of current UTC time '{nowUtc:yyyy-MM-ddTHH:mm:ssZ}'.");
                return false;
            }

            //validate offer item ID
            string offerID = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferID", xmlNamespaceManager)?.InnerText;

            string offerItemID = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer[ct:OwnerCode='VS']/ct:OfferItem[ct:MandatoryInd='true' and ct:Price/ct:TotalAmount[@CurCode]='0.00']/ct:OfferItemID",xmlNamespaceManager )?.InnerText;

            if (offerItemID != offerID+"-1")
            {
                result.Message.Add($"Incorrect offerItemID returned .expected {offerID} actual {offerItemID}");
            }

            //validate NameChangeService
            XmlNode paxNameChange = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferItem/ct:NameChangeService[ct:PaxRefID='{paxRefID}' and ct:ServiceDefinitionRefID=../../../../../ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition/ct:ServiceDefinitionID and ct:ServiceID='SV1']", xmlNamespaceManager);

            if (string.IsNullOrEmpty(paxNameChange.OuterXml))
            {
                result.Message.Add($"Incorrect paxNameChange details {paxNameChange.OuterXml}");
            }

            return result.IsValid;
        }

        public static bool ValidateReshopDMSSegment(string orderCreateRS, string reshopRS, int retainSegmentCount, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new(orderCreateResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateResponseXmlDocument.LoadXml(orderCreateRS);
            reshopResponseXmlDocument.LoadXml(reshopRS);

            List<XmlNode> orderCreateDMS = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).OfType<XmlNode>().ToList();

            List<XmlNode> reshopDMS = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).OfType<XmlNode>().ToList();

            int dmsMatchedCount = 0;

            foreach (XmlNode node in orderCreateDMS)
            {
                foreach (XmlNode node2 in reshopDMS)
                {
                    if (node.InnerText == node2.InnerText)
                    {
                        dmsMatchedCount++;
                    }
                }
            }

            if (dmsMatchedCount < retainSegmentCount)
            {
                result.Message.Add($"Matched Dated Marketing Segment List count was expecting {retainSegmentCount} but found {dmsMatchedCount} instead.");

            }
            return result.IsValid;
        }

        public static bool validateExistingOrderItems(string orderCreateRS,string reshopRQ, string reshopRS, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();
            XmlDocument reshopRequestXmlDocument = new XmlDocument();

            orderCreateResponseXmlDocument.LoadXml(orderCreateRS);
            reshopResponseXmlDocument.LoadXml(reshopRS);
            reshopRequestXmlDocument.LoadXml(reshopRQ);

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList orderItems = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem", xmlNamespaceManager);

            XmlNodeList OfferItems = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem", xmlNamespaceManager);

            XmlNodeList retainedServices = reshopRequestXmlDocument.SelectNodes($"msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:ServiceOrder/ct:DeleteOrderItem/ct:RetainServiceID", xmlNamespaceManager);

            foreach (XmlNode node in OfferItems)
            {
                XmlNodeList existingOrderItems = node.SelectNodes("ct:ExistingOrderItem", xmlNamespaceManager);

                string paxRefID = node.SelectSingleNode("ct:Service/ct:PaxRefID", xmlNamespaceManager).InnerText;

                XmlNodeList paxOrderItems = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:PaxRefID='{paxRefID}']", xmlNamespaceManager);

                XmlNodeList expectedserviceIDs = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:PaxRefID='{paxRefID}']/ct:Service/ct:ServiceID", xmlNamespaceManager);

                XmlNodeList actualServiceIDs = node.SelectNodes("ct:ExistingOrderItem/ct:ExistingService/ct:ServiceRefID", xmlNamespaceManager);

                XmlNodeList unchangedInd = node.SelectNodes("ct:ExistingOrderItem/ct:ExistingService/ct:UnchangedInd[text()='true']", xmlNamespaceManager);

                XmlNodeList futureStatusCode = node.SelectNodes("ct:ExistingOrderItem/ct:ExistingService/ct:FutureStatusCode[text()='CANCELLED']", xmlNamespaceManager);

                if (paxOrderItems.Count != existingOrderItems.Count)
                {
                    result.Message.Add($"Count of Existing Order Items does not match Pax Order Items count. Expected: {paxOrderItems.Count} and returned: {existingOrderItems.Count}");
                }

                if (expectedserviceIDs.Count != actualServiceIDs.Count)
                {
                    result.Message.Add($"Count of Service IDs does not match Existing Order Items count. Expected: {expectedserviceIDs.Count} and returned: {actualServiceIDs.Count}");
                }

                if (retainedServices.Count > 0 && unchangedInd.Count + futureStatusCode.Count != actualServiceIDs.Count)
                {
                    result.Message.Add($"retained segments service status unchangedInd should be true");
                }

                if(retainedServices.Count == 0 && futureStatusCode.Count != actualServiceIDs.Count)
                {
                    result.Message.Add($"all segments service status should be cancelled for complete journey change");
                }
            }

            return result.IsValid;
        }

        public static bool ValidatePaxSegmentList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            IDictionary<int, string> cabinTypeCodeAndNames = new Dictionary<int, string>();
            cabinTypeCodeAndNames.Add(2, "Business");
            cabinTypeCodeAndNames.Add(4, "Premium Economy");
            cabinTypeCodeAndNames.Add(5, "Economy");

            string expectedCabinTypeCode = reshopRequestXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:ServiceOrder/ct:AddOfferItems/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int paxSegmentCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] paxSegments = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[ct:PaxSegmentID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxSegmentRefID and ct:DatedMarketingSegmentRefId=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:DatedMarketingSegmentId]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxSegmentID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxSegments.Length.Equals(paxSegmentCount))
            {
                result.Message.Add($"Invalid paxSegments, was expecting {paxSegmentCount} but found {paxSegments.Length} instead.");

            }

            int paxSegmentIdCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID", xmlNamespaceManager).Count;

            string cabinTypeValueFromRequest = reshopRequestXmlDocument.SelectSingleNode("/msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:ServiceOrder/ct:AddOfferItems/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager)?.InnerText;

            int cabinTypeCodes = reshopResponseXmlDocument.SelectNodes($"//ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode[.='{cabinTypeValueFromRequest}']", xmlNamespaceManager).Count;

            //Checked with PO and if 4 then we can get a miss match in the counts as we may have flights which cannot offer 4 and return 2 or 5
            if (cabinTypeCodes == 0)
            {
                result.Message.Add($"No cabin type codes returned for the expected Cabin");

            }

            bool marketingCarrierRBD_Codes = paxSegments.All(x => x.All(y => y.SelectSingleNode("ct:MarketingCarrierRBD_Code", xmlNamespaceManager).InnerText.Equals(null)));

            if (marketingCarrierRBD_Codes == true)
            {
                result.Message.Add($"Invalid/null marketingCarrierRBD_Codes");

            }

            for (int i = 1; i <= paxSegmentCount; i++)
            {
                string paxSegmentId = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{i}]/ct:PaxSegmentID", xmlNamespaceManager).InnerText;

                if (!paxSegmentId.Equals("SEG" + i))
                {
                    result.Message.Add($"paxsegmentId is not as expected SEG{i} actual {paxSegmentId}");

                }
            }
            return result.IsValid;
        }

        public static bool ValidateBaggageAllowancelist(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderReshopRequestXmlDocument = new XmlDocument();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            orderReshopRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderReshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderReshopRequestXmlDocument.LoadXml(requestXml);
            orderReshopResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList baggageAllowanceCount = orderReshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance", xmlNamespaceManager);

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

        public static bool ValidatePriceClassList(string requestXml, string requestXml1, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument airshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);
            airshopRequestXmlDocument.LoadXml(requestXml1);

            XmlNamespaceManager xmlNamespaceManager = reshopRequestXmlDocument.CreateXmlNamespaceManager();

            string requestedCabinTypeCodeReshop = reshopRequestXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:ServiceOrder/ct:AddOfferItems/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            string requestedCabinTypeCodeAirShopping = airshopRequestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int journeyCount = airshopRequestXmlDocument.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria", xmlNamespaceManager).Count;

            int priceClassCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            if (requestedCabinTypeCodeReshop.Equals(requestedCabinTypeCodeAirShopping))
            {
                for (int i = 1; i <= priceClassCount; i++)
                {
                    string name = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                    if (requestedCabinTypeCodeReshop.Equals("5"))
                    {
                        if (!name.Equals("Economy Classic") && !name.Equals("Economy Classic Flex") && !name.Equals("Economy Light") && !name.Equals("Economy Delight") && !name.Equals("Economy Delight Flex"))
                        {
                            result.Message.Add($"requested cabin type and cabin name in response are not same");

                        }
                    }
                    else if (requestedCabinTypeCodeReshop.Equals("4"))
                    {
                        if (!name.Equals("Premium") && !name.Equals("Premium Flex"))
                        {
                            result.Message.Add($"requested cabin type and cabin name in response are not same");

                        }
                    }
                    else
                    {
                        if (!name.Equals("Upper Class") && !name.Equals("Upper Class Flex"))
                        {
                            result.Message.Add($"requested cabin type and cabin name in response are not same");

                        }
                    }

                    string priceClassID = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                    if (!priceClassID.Equals("PC" + i))
                    {
                        result.Message.Add($"Price class ID is not as expected");

                    }
                }
            }
            else
            {
                for (int i = 1; i <= priceClassCount; i++)
                {
                    string name = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                    if (requestedCabinTypeCodeReshop.Equals("5") && requestedCabinTypeCodeAirShopping.Equals("4"))
                    {
                        if (!name.Equals("Economy Classic") && !name.Equals("Economy Classic Flex") && !name.Equals("Economy Light") && !name.Equals("Economy Delight") && !name.Equals("Economy Delight Flex") && !name.Equals("Premium") && !name.Equals("Premium Flex"))
                        {
                            result.Message.Add($"requested cabin type and cabin name in response are not same");

                        }
                    }
                    else if (requestedCabinTypeCodeReshop.Equals("4") && requestedCabinTypeCodeAirShopping.Equals("2") && !name.Equals("Upper Class") && !name.Equals("Upper Class Flex"))
                    {
                        if (!name.Equals("Premium") && !name.Equals("Premium Flex"))
                        {
                            result.Message.Add($"requested cabin type and cabin name in response are not same");

                        }
                    }
                    else
                    {
                        if (!name.Equals("Upper Class") && !name.Equals("Upper Class Flex") && !name.Equals("Economy Classic") && !name.Equals("Economy Classic Flex") && !name.Equals("Economy Light") && !name.Equals("Economy Delight") && !name.Equals("Economy Delight Flex"))
                        {
                            result.Message.Add($"requested cabin type and cabin name in response are not same");

                        }
                    }

                    string priceClassID = reshopResponseXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                    if (!priceClassID.Equals("PC" + i))
                    {
                        result.Message.Add($"Price class ID is not as expected");

                    }
                }

            }

            //validate services

            int serviceIDCounter = 0;

            XmlNodeList expectedserviceCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem", xmlNamespaceManager);

            foreach (XmlNode node in expectedserviceCount)
            {
                serviceIDCounter++;

                int paxjourneyRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID", xmlNamespaceManager).Count;

                if (paxjourneyRefIDs != journeyCount)
                {
                    result.Message.Add($"Incorrect paxjourneyRefIDs in services, actual{paxjourneyRefIDs} but exepected{journeyCount}");
                }

                string serviceID = node.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;

                if (!serviceID.Is("SVNEW" + serviceIDCounter))
                {
                    result.Message.Add($"Incorrect service ID in services, actual{serviceID} but exepected{"SVNEW" + serviceIDCounter}");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateBaggageAssociations(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            int expectedBaggageAssociationsCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:BaggageAssociations", xmlNamespaceManager).Count;

            int actualbaggageAssociations = reshopResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:BaggageAssociations[ct:BaggageAllowanceRefID=../../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID and ct:PaxRefID=../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager).Count;

            if (!expectedBaggageAssociationsCount.Equals(actualbaggageAssociations))
            {
                result.Message.Add($"Invalid BaggageAssociations, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            int expectedJourneyPriceClassCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass", xmlNamespaceManager).Count;

            int actualJourneyPriceClassCount = reshopResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass[ct:PriceClassRefID=../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID and ct:PaxJourneyRefID=../../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!expectedJourneyPriceClassCount.Equals(actualJourneyPriceClassCount))
            {
                result.Message.Add($"Invalid priceclass, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            return result.IsValid;
        }

        public static bool ValidateFareComponent(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            reshopResponseXmlDocument.LoadXml(responseXml);

            int expectedserviceCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem", xmlNamespaceManager).Count;

            int expectedCancelRestrictionCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions", xmlNamespaceManager).Count;

            int expectedChangeRestrictionCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions", xmlNamespaceManager).Count;

            int expectedfareComponentCount = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).Count;

            int actualfareComponentCount = reshopResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent[ct:PaxSegmentRefID=../../../../../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:PriceClassRefID=../../../../../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID]", xmlNamespaceManager).Count;

            if (!expectedfareComponentCount.Equals(actualfareComponentCount))
            {
                result.Message.Add($"Invalid farecomponent, actual{actualfareComponentCount} but exepected{expectedfareComponentCount}");

            }

            int actualCancelRestrictionCount = reshopResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions[ct:AllowedModificationInd ]", xmlNamespaceManager).Count;

            if (!actualCancelRestrictionCount.Equals(expectedCancelRestrictionCount))
            {
                result.Message.Add($"Invalid Cancel Restriction, actual{actualCancelRestrictionCount} but exepected{expectedCancelRestrictionCount}");

            }

            int actualChangeRestrictionCount = reshopResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions[ct:AllowedModificationInd ]", xmlNamespaceManager).Count;

            if (!actualChangeRestrictionCount.Equals(expectedChangeRestrictionCount))
            {
                result.Message.Add($"Invalid Change Restriction, actual{actualChangeRestrictionCount} but exepected{expectedChangeRestrictionCount}");

            }
            return result.IsValid;
        }

        public static bool ValidateExpirationTTL(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(reshopResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopResponseXmlDocument.LoadXml(responseXml);

            int offerIdCount = reshopResponseXmlDocument.SelectNodes("//ct:OfferID", xmlNamespaceManager).Count;
            int expirationTtlCount = reshopResponseXmlDocument.SelectNodes("//ct:OfferExpirationTimeLimitDateTime", xmlNamespaceManager).Count;
            int paymentTtlCount = reshopResponseXmlDocument.SelectNodes("//ct:PaymentTimeLimitDateTime", xmlNamespaceManager).Count;

            int experationTtlFilteredByTCount = reshopResponseXmlDocument.SelectNodes("//ct:OfferExpirationTimeLimitDateTime//text()[contains(., 'T')]", xmlNamespaceManager).Count;

            if (!offerIdCount.Equals(experationTtlFilteredByTCount))
            {
                result.Message.Add($"Offer count {offerIdCount} did not match experation TTL filtered by T Count {paymentTtlCount}");

            }
            return result.IsValid;
        }

        public static bool ValidatePriceClassListCabinType(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(requestXml);
            responseXmlDocument.LoadXml(responseXml);

            //var requestedCabinTypeCode = requestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int expectedpriceClassCount = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            XmlNodeList actualpriceclass = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[ct:CabinType/ct:CabinTypeCode=../../ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode and ct:CabinType/ct:CabinTypeName=../../ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeName]", xmlNamespaceManager);

            if (expectedpriceClassCount != actualpriceclass.Count)
            {
                result.Message.Add("Incorrect cabin code or name returned in the price class list");

            }

            foreach (XmlNode priceClass in actualpriceclass)
            {
                string cabinCode = priceClass.SelectSingleNode("ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;
                string cabinName = priceClass.SelectSingleNode("ct:CabinType/ct:CabinTypeName", xmlNamespaceManager).InnerText;

                if (cabinCode.Equals("5") && !cabinName.Equals("Economy", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Message.Add("Incorrect cabin name or code returned in the price class list");

                }

                if (cabinCode.Equals("4") && !cabinName.Equals("Premium Economy", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Message.Add("Incorrect cabin name or code returned in the price class list");

                }

                if (cabinCode.Equals("2") && !cabinName.Equals("Business", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.Message.Add("Incorrect cabin name or code returned in the price class list");

                }
            }

            return result.IsValid;
        }

        public static bool ValidateTaxSummary(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(requestXml);
            responseXmlDocument.LoadXml(responseXml);

            XmlNodeList taxCode = responseXmlDocument.GetElementsByTagName("TaxCode");

            foreach (XmlElement taxCodes in taxCode)
            {
                string taxCodeValue = taxCodes["TaxCode"].InnerText;

                if (string.IsNullOrEmpty(taxCodeValue))
                {
                    result.Message.Add(" tax code value cannot be null");
                }
            }

            XmlNodeList taxSegments = responseXmlDocument.SelectNodes("//ct:Tax", xmlNamespaceManager);

            foreach (XmlNode tax in taxSegments)
            {
                XmlNode amount = tax.SelectSingleNode("//ct:Tax/ct:Amount", xmlNamespaceManager);

                if (amount is null)
                {
                    result.Message.Add("Amount value cannot be null");
                }
            }

            return result.IsValid;
        }

        public static bool ValidatePricingDetails( string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            reshopResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = reshopResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList reshopOffers = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer", xmlNamespaceManager);

            foreach(XmlNode reshopOffer in reshopOffers)
            {
                XmlNodeList offerItems = reshopOffer.SelectNodes("ct:AddedOfferItem", xmlNamespaceManager);

                foreach (XmlNode offerItem in offerItems)
                {
                    decimal diffPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    decimal newPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    decimal oldPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    string differentialTypeCode = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DifferentialTypeCode", xmlNamespaceManager).InnerText;

                    decimal dueByAirlineAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueByAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    decimal dueToAirlineAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueToAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    if(differentialTypeCode.Is("EvenExchange") && (dueByAirlineAmount!=0 || dueToAirlineAmount != 0))
                    {
                        result.Message.Add("dueByAirlineAmount and dueToAirlineAmount should be 0 for even exchange offer");
                    }

                    if (differentialTypeCode.Is("Addcol") && (dueByAirlineAmount != 0 || dueToAirlineAmount != newPriceTotalAmount - oldPriceTotalAmount))
                    {
                        result.Message.Add($"Expected dueByAirlineAmount is 0 but actual is {dueByAirlineAmount}, expected dueToAirlineAmount is {newPriceTotalAmount - oldPriceTotalAmount} but actual {dueToAirlineAmount}");
                    }

                    if (differentialTypeCode.Is("Refund") && (dueToAirlineAmount != 0 || dueByAirlineAmount != newPriceTotalAmount - oldPriceTotalAmount))
                    {
                        result.Message.Add($"Expected dueToAirlineAmount is 0 but actual is {dueToAirlineAmount}, expected dueByAirlineAmount {newPriceTotalAmount - oldPriceTotalAmount} actual {dueByAirlineAmount}");
                    }

                    if (Math.Abs(diffPriceTotalAmount) != Math.Abs(newPriceTotalAmount - oldPriceTotalAmount))
                    {
                        result.Message.Add($"Incorrect Price  Differential amount {offerItem.OuterXml}");
                    }

                    if (((newPriceTotalAmount - oldPriceTotalAmount)==0 && !differentialTypeCode.Is("EvenExchange")) || ((newPriceTotalAmount - oldPriceTotalAmount) > 0 && !differentialTypeCode.Is("AddCol")) || ((newPriceTotalAmount - oldPriceTotalAmount) < 0 && differentialTypeCode.Is("AddCol")))
                    {
                        result.Message.Add($"Incorrect Price  Differential Type {differentialTypeCode} {offerItem.OuterXml}");
                    }

                }
            }

            return result.IsValid;
        }

        public static bool ValidateSeatBooking(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList warning = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:Warning", xmlNamespaceManager);

            foreach (XmlNode node in warning)
            {
                string desc = node.SelectSingleNode("ct:DescText", xmlNamespaceManager).InnerText;
                string errrorCode = node.SelectSingleNode("ct:Code", xmlNamespaceManager).InnerText;
                if (!desc.Is("Your selected seats will be cancelled due to flight segment changes. Please select new seats for your revised booking") || !errrorCode.Is("214"))
                {
                    result.Message.Add($"incorrect seat warning returned in reshop response {node.OuterXml}");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateNameCorrectionErrorRS(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList error = responseXmlDocument.SelectNodes("//msg:Error[ct:DescText='Full surname changes are only permitted in exceptional circumstances. Please refer to the Airline’s policy or contact the Airline directly' and ct:Code='137' and ct:TagText='Name Change Not Allowed' and ct:TypeCode='NotProcessed']", xmlNamespaceManager);

            if (error.Count == 0)
            {
                result.Message.Add($"incorrect error returned in reshop response {responseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:Warning", xmlNamespaceManager)?.InnerText}");
            }

            return result.IsValid;
        }

        public static bool ValidateReshopCancelRS(string airshoppingRQ, string responseXml, out ValidationResultHelper result,string flow="")
        {
            result = new ValidationResultHelper();

            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            XmlDocument airShopRQXmlDocument = new XmlDocument();

            airShopRQXmlDocument.LoadXml(airshoppingRQ);

            reshopResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = reshopResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNode expectedPaxList = airShopRQXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:PaxList", xmlNamespaceManager);

            XmlNode actualPaxList = reshopResponseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxList", xmlNamespaceManager);

            if (!expectedPaxList.InnerText.Is(actualPaxList.InnerText))
            {
                result.Message.Add($"Incorrect paxList in reshop cancel response, expected paxlist -->{expectedPaxList.OuterXml} but actual paxlist-->{actualPaxList.OuterXml}");
            }

            XmlNodeList offerItems = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:DeleteOrderItem", xmlNamespaceManager);

            foreach (XmlNode offerItem in offerItems)
            {
                XmlNodeList fareDeatilPax = offerItem.SelectNodes("ct:PriceDifferential//ct:NewPrice//ct:FareDetail[ct:PaxRefID]", xmlNamespaceManager);

                Dictionary<string, int> pax = FindAndReturnDataHelper.ReturnPaxCount(reshopResponseXmlDocument);

                if ((fareDeatilPax[0].InnerText.Is("Adult_1") && fareDeatilPax.Count != pax["Adult"]) || (fareDeatilPax[0].InnerText.Is("Young_1") && fareDeatilPax.Count != pax["YoungAdult"]) || (fareDeatilPax[0].InnerText.Is("Child_1") && fareDeatilPax.Count != pax["Child"]) || (fareDeatilPax[0].InnerText.Is("Infant_1") && fareDeatilPax.Count != pax["Infant"]))
                {
                    result.Message.Add($"Incorrect pax in faredeatil Expected {offerItem.OuterXml}");
                }

                //validate DiffPrice
                decimal diffPriceBaseAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:BaseAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal diffPriceDueByAirlineAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueByAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal diffPriceTotalTaxAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal diffPriceTotalAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                if (flow.Is("rfc") && (diffPriceDueByAirlineAmount != Math.Abs(diffPriceTotalTaxAmount + diffPriceBaseAmount)) || (diffPriceTotalAmount != diffPriceTotalTaxAmount + diffPriceBaseAmount))
                {
                    result.Message.Add($"Incorrect amount in Differential price {offerItem.OuterXml}");
                }

                if ((diffPriceTotalAmount != diffPriceTotalTaxAmount + diffPriceBaseAmount) || (diffPriceDueByAirlineAmount > Math.Abs(diffPriceTotalAmount)))
                {
                    result.Message.Add($"Incorrect amount in Differential price {offerItem.OuterXml}");
                }

                // validate newprice
                decimal newPriceBaseAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:NewPrice/ct:Price/ct:BaseAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal newPriceTotalTaxAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal newPriceTotalAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                if (newPriceBaseAmount != 0 || newPriceTotalTaxAmount != 0 || newPriceTotalAmount != 0)
                {
                    result.Message.Add($"Incorrect amount in New price price {offerItem.OuterXml}");
                }

                //validate oldprice
                decimal oldPriceBaseAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:BaseAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal oldPriceTotalTaxAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                XmlNodeList oldPriceTax = offerItem.SelectNodes("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TaxSummary/ct:Tax", xmlNamespaceManager);

                decimal oldPriceTotalAmount = offerItem.SelectSingleNode("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal expectedTotalTaxAmount = 0;

                foreach (XmlNode taxNode in oldPriceTax)
                {
                    decimal amount = taxNode.SelectSingleNode("ct:Amount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    expectedTotalTaxAmount = expectedTotalTaxAmount + amount;
                }

                if ((oldPriceTotalAmount != oldPriceBaseAmount + oldPriceTotalTaxAmount) || (expectedTotalTaxAmount != oldPriceTotalTaxAmount))
                {
                    result.Message.Add($"Incorrect amount in Old price {offerItem.OuterXml}");
                }

                if ((oldPriceTotalAmount != Math.Abs(diffPriceTotalAmount)) || (diffPriceDueByAirlineAmount > oldPriceTotalAmount))
                {
                    result.Message.Add($"Incorrect amount in Differential price {offerItem.OuterXml}");
                }
            }

            return result.IsValid;
        }
    }
}
