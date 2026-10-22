using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.NDC.Constants;

namespace VAA.EAPI.Automation.NDC.Tests.OfferPrice
{
    internal class OfferPriceValidator
    {
        public static string code = "";
        public static string responseText = "";

        public static bool ValidateErrorXmlResponse(XmlDocument responseXml, string expectedErrorCode, string expectedDesxText, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = responseXml.CreateXmlNamespaceManager();

            string actualErrorcode = responseXml.SelectSingleNode("//ct:Code", xmlNamespaceManager).InnerText;

            string actualDescText = responseXml.SelectSingleNode("//ct:DescText", xmlNamespaceManager).InnerText;

            if (!actualErrorcode.Is(expectedErrorCode))
            {
                result.Message.Add($"Expected error code is {expectedErrorCode} but actual is {actualErrorcode}");
            }

            if (!actualDescText.Is(expectedDesxText))
            {
                result.Message.Add($"Expected error DescText is {expectedDesxText} but actual is {actualDescText}");
            }

            return result.IsValid;
        }

        public static bool ErrorCodeAssertion(XmlNodeList code_xml, string expected_Code)
        {
            //string code;
            if (code_xml != null && code_xml.Count > 0)
            {
                foreach (XmlNode xn in code_xml)
                {
                    code = xn.InnerText;
                }
            }
            Assert.True(code.Equals(expected_Code),
           $"Error Code for Order Change error message is not as expected. Expected {expected_Code} " +
           $"but was {code}");

            return true;
        }

        public static bool ErrorMsgAssertion(XmlNodeList errorTextNode, string expected_Msg)
        {
            //string code;
            if (errorTextNode != null && errorTextNode.Count > 0)
            {
                foreach (XmlNode xn in errorTextNode)
                {
                    responseText = xn.InnerText;
                }
            }
            Assert.True(responseText.Equals(expected_Msg),
           $"Error Code for Order Change error message is not as expected. Expected {expected_Msg} " +
           $"but was {responseText}");

            return true;
        }

        public static bool ValidatePaxList(string requestXml, dynamic responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocumentAirShoppingRequest = new XmlDocument();

            XmlNamespaceManager airShoppingXmlnsManager = new XmlNamespaceManager(xmlDocumentAirShoppingRequest.NameTable);
            airShoppingXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            xmlDocumentAirShoppingRequest.LoadXml(requestXml);

            string paxListBlockRequest = xmlDocumentAirShoppingRequest.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/def:PaxList", airShoppingXmlnsManager)?.OuterXml;

            int? paxListBlockInfants = xmlDocumentAirShoppingRequest.SelectNodes("/msg:IATA_AirShoppingRQ/msg:Request/def:PaxList/def:Pax/def:PaxID[contains(text(), 'INFANT')]", airShoppingXmlnsManager)?.Count;

            XmlDocument xmlDocumentOfferPriceResponse = new XmlDocument();

            xmlDocumentOfferPriceResponse.LoadXml(responseXml);

            string paxListBlockResponse = xmlDocumentOfferPriceResponse.SelectSingleNode("/msg:IATA_OfferPriceRS/msg:Response/def:DataLists/def:PaxList", airShoppingXmlnsManager)?.OuterXml;

            XmlNodeList paxListRefIds = xmlDocumentOfferPriceResponse.SelectNodes("/msg:IATA_OfferPriceRS/msg:Response/def:DataLists/def:PaxList/def:Pax/def:PaxRefID", airShoppingXmlnsManager);

            if (paxListBlockInfants > 0)
            {
                if (paxListRefIds?.Count != paxListBlockInfants)
                {
                    result.Message.Add($"Count of Infants within Adult Pax is not as expected. Expected: {paxListBlockInfants} and returned: {xmlDocumentOfferPriceResponse.SelectNodes(" /msg:IATA_AirShoppingRS/msg:Response/def:DataLists/def:PaxList/def:Pax/def:PaxRefID", airShoppingXmlnsManager)?.Count}");
                }

                for (int i = paxListRefIds.Count - 1; i >= 0; i--)
                    paxListRefIds[i].ParentNode.RemoveChild(paxListRefIds[i]);

                string amendedPaxListResponse = xmlDocumentOfferPriceResponse.SelectSingleNode("/msg:IATA_OfferPriceRS/msg:Response/def:DataLists/def:PaxList", airShoppingXmlnsManager)?.OuterXml;
                if (paxListBlockRequest != amendedPaxListResponse)
                {
                    result.Message.Add($"Pax List from AirShopping request: {paxListBlockRequest} did not match Pax List in AirShopping response: {amendedPaxListResponse}");
                }
            }
            else
            {
                if (paxListBlockRequest != paxListBlockResponse)
                {
                    result.Message.Add($"Pax List from AirShopping request: {paxListBlockRequest} did not match Pax List in AirShopping response: {paxListBlockResponse}");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateServiceDefinitionList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            int serviceDefinitionCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).Count;

            if (serviceDefinitionCount == 0)
            {
                result.Message.Add("Service Definition is not returned in offer price response");
            }

            XmlNodeList serviceDefinition = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager);

            List<string> desctext = [];

            foreach (XmlNode node in serviceDefinition)
            {
                XmlNode desc =node.SelectSingleNode("ct:ServiceDefinitionID", xmlNamespaceManager);

                desc.ParentNode.RemoveChild(desc);

                if(desctext.Contains(node.InnerText))
                {
                    result.Message.Add($"Duplicate service definition list found {node.OuterXml}");
                }
                else
                {
                    desctext.Add(node.InnerText);
                }

            }

            return result.IsValid;
        }

        public static bool ValidateOriginDestList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            XmlDocument orgDestListXmlDocument = new XmlDocument();

            XmlNode expectedOrgDestList = airShoppingRequestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria", xmlNamespaceManager);
            XmlNode actualOrgDestList = offerPriceXmlDocument.SelectSingleNode("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:OriginDestList", xmlNamespaceManager);

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
                result.Message.Add($"Invalid PaxList, was expecting {expectedOrgDestListCount} but found {validatedOrgDestListCount} instead.");
            }

            int? paxJourneys = offerPriceXmlDocument?.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest[ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID ]", xmlNamespaceManager)?.Count;

            if (!paxJourneys.Equals(expectedOrgDestListCount) && actualOrgDestListCount < expectedOrgDestListCount)
            {
                result.Message.Add("Journey ID in Origin DestList is not found in PaxJOurneyList");
            }

            return result.IsValid;
        }

        public static bool ValidateDatedMarketingAndOperatingList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] datedMarketingSegments = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[ct:DatedMarketingSegmentId=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:DatedMarketingSegmentRefId and ct:DatedOperatingSegmentRefId=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingSegmentId and ct:Arrival/ct:IATA_LocationCode != ct:Dep/ct:IATA_LocationCode  and ct:Arrival/ct:AircraftScheduledDateTime != ct:Dep/ct:AircraftScheduledDateTime]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:DatedMarketingSegmentId", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            int secureFlightIndCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment[ct:SecureFlightInd='true']", xmlNamespaceManager).Count;

            if (secureFlightIndCount != datedMarketingSegmentCount)
            {
                result.Message.Add("SecureFlightInd is not returned as true in DatedOperatingSegmentList");
            }

            if (!datedMarketingSegments.Length.Equals(datedMarketingSegmentCount))
            {
                result.Message.Add($"Invalid datedMarketingSegments, was expecting {datedMarketingSegmentCount} but found {datedMarketingSegments.Length} instead.");
            }
            return result.IsValid;
        }

        public static bool ValidatePaxSegmentList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            IDictionary<int, string> cabinTypeCodeAndNames = new Dictionary<int, string>();
            cabinTypeCodeAndNames.Add(2, "Business");
            cabinTypeCodeAndNames.Add(4, "Premium Economy");
            cabinTypeCodeAndNames.Add(5, "Economy");

            string expectedCabinTypeCode = airShoppingRequestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int paxSegmentCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] paxSegments = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[ct:PaxSegmentID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxSegmentRefID and ct:DatedMarketingSegmentRefId=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:DatedMarketingSegmentId]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxSegmentID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxSegments.Length.Equals(paxSegmentCount))
            {
                result.Message.Add($"Invalid paxSegments, was expecting {paxSegmentCount} but found {paxSegments.Length} instead.");
            }

            int paxSegmentIdCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID", xmlNamespaceManager).Count;

            string cabinTypeValueFromRequest = airShoppingRequestXmlDocument.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager)?.InnerText;

            int cabinTypeCodes = offerPriceXmlDocument.SelectNodes($"//ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode[.='{cabinTypeValueFromRequest}']", xmlNamespaceManager).Count;

            //Checked with PO and if 4 then we can get a miss match in the counts as we may have flights which cannot offer 4 and return 2 or 5
            if (cabinTypeCodes == 0)
            {
                result.Message.Add("No cabin type codes returned for the expected Cabin");
            }

            bool marketingCarrierRBD_Codes = paxSegments.All(x => x.All(y => y.SelectSingleNode("ct:MarketingCarrierRBD_Code", xmlNamespaceManager).InnerText.Equals(null)));

            if (marketingCarrierRBD_Codes == true)
            {
                result.Message.Add("Invalid/null marketingCarrierRBD_Codes");
            }

            for (int i = 1; i <= paxSegmentCount; i++)
            {
                string paxSegmentId = offerPriceXmlDocument.SelectSingleNode($"msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{i}]/ct:PaxSegmentID", xmlNamespaceManager).InnerText;

                if (!paxSegmentId.Equals("SEG" + i))
                {
                    result.Message.Add("paxsegmentId is not as expected SEG{i} actual {paxSegmentId}");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateBaggageAllowancelist(string requestXml, string responseXml, out ValidationResultHelper result)
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

            XmlNodeList baggageAllowanceCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance", xmlNamespaceManager);

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

        public static bool ValidatePriceClassList(string requestXml, string responseXml, string cabinType, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            int priceClassCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            for (int i = 1; i <= priceClassCount; i++)
            {
                string name = offerPriceXmlDocument.SelectSingleNode($"msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                if (cabinType.Equals("Economy Light", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Economy Light", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Classic", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Classic Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Delight", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Delight Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else if (cabinType.Equals("Economy Classic", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Economy Classic", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Classic Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Delight", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Delight Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else if (cabinType.Equals("Economy Classic Flex", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Economy Classic Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Delight Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else if (cabinType.Equals("Economy Delight", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Economy Delight", StringComparison.OrdinalIgnoreCase) && !name.Equals("Economy Delight Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else if (cabinType.Equals("Economy Delight Flex", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Economy Delight Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }

                }
                else if (cabinType.Equals("Premium", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Premium", StringComparison.OrdinalIgnoreCase) && !name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }

                }
                else if (cabinType.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Premium Flex", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }

                }
                else if (cabinType.Equals("Upper Class", StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals("Upper Class", StringComparison.OrdinalIgnoreCase) && !name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }

                }
                else if (cabinType.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                {
                    int otherOffersCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer", xmlNamespaceManager).Count;
                    if (otherOffersCount > 0)
                    {
                        result.Message.Add("Other offers are not expected when selected offer is Upper Flex");
                    }
                    if (!name.Equals("Upper Class Flex", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }

                }

                string priceClassID = offerPriceXmlDocument.SelectSingleNode($"msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                if (!priceClassID.Equals("PC" + i))
                {
                    result.Message.Add("Price class ID is not as expected");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateOtherOffersBaggageAssociations(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = airShoppingRequestXmlDocument.CreateXmlNamespaceManager();

            int journeyCount = airShoppingRequestXmlDocument.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria", xmlNamespaceManager).Count;

            int segmentCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int expectedBaggageAssociationsCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:BaggageAssociations", xmlNamespaceManager).Count;

            int actualbaggageAssociations = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:BaggageAssociations[ct:BaggageAllowanceRefID=../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID and ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager).Count;

            int offerCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer", xmlNamespaceManager).Count;

            int baggageJourneyCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer[ct:BaggageAssociations/ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID = ct:JourneyOverview/ct:JourneyPriceClass/ct:PaxJourneyRefID]", xmlNamespaceManager).Count;

            XmlNodeList otherOfferServices = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem", xmlNamespaceManager);

            XmlNodeList pricedOfferServices = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem", xmlNamespaceManager);

            if (offerCount != baggageJourneyCount)
            {
                result.Message.Add("Incorrect baggage journeys returned in offer");
            }

            if (!expectedBaggageAssociationsCount.Equals(actualbaggageAssociations))
            {
                result.Message.Add($"Invalid BaggageAssociations, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");
            }

            int expectedJourneyPriceClassCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass", xmlNamespaceManager).Count;

            int actualJourneyPriceClassCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass[ct:PriceClassRefID=../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID and ct:PaxJourneyRefID=../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!expectedJourneyPriceClassCount.Equals(actualJourneyPriceClassCount))
            {
                result.Message.Add($"Invalid priceclass, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");
            }

            XmlNodeList otherOffers = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer", xmlNamespaceManager);

            foreach (XmlNode node in otherOffers)
            {
                decimal actualTotalAmount = 0m;
                XmlNodeList offerItems = node.SelectNodes("ct:OfferItem", xmlNamespaceManager);

                foreach (XmlNode offerItemNode in offerItems)
                {
                    int paxCount = offerItemNode.SelectNodes("ct:FareDetail/ct:PaxRefID", xmlNamespaceManager).Count;
                    string baseAmount = offerItemNode.SelectSingleNode("ct:Price/ct:BaseAmount", xmlNamespaceManager).InnerText;
                    string taxAmount = offerItemNode.SelectSingleNode("ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlNamespaceManager).InnerText;
                    string totalamount = offerItemNode.SelectSingleNode("ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText;

                    if (Math.Round(Convert.ToDecimal(totalamount), 2) != Math.Round(Convert.ToDecimal(baseAmount), 2) + Math.Round(Convert.ToDecimal(taxAmount), 2))
                    {
                        result.Message.Add($"Incorrect Base amount, tax amount and total amount in offeritem , Base amount{baseAmount} taxamount {taxAmount} total amount {totalamount} ");
                    }

                    actualTotalAmount = (Convert.ToDecimal(totalamount) * paxCount) + actualTotalAmount;
                }
            }

            //service ID validation other offers
            int serviceIDCounter = 1;

            foreach (XmlNode node in pricedOfferServices)
            {
                int paxjourneyRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID", xmlNamespaceManager).Count;

                int paxSegmentRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:ServiceDefinitionRef[ct:ServiceDefinitionRefID]/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                int serviceCount = node.SelectNodes("ct:Service", xmlNamespaceManager).Count;

                if (paxjourneyRefIDs != journeyCount && paxjourneyRefIDs > 0)
                {
                    result.Message.Add($"Incorrect paxjourneyRefIDs in services, actual{paxjourneyRefIDs} but exepected{journeyCount}");
                }
                
                if (paxSegmentRefIDs != segmentCount && paxSegmentRefIDs > 0)
                {
                    result.Message.Add($"Incorrect paxSegmentRefID in services, actual{paxSegmentRefIDs} but exepected{segmentCount}");
                }

                string serviceID = node.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;

                if (!serviceID.Is("SV" + serviceIDCounter))
                {
                    result.Message.Add($"Incorrect service ID in services, actual{serviceID} but exepected{"SV" + serviceIDCounter}");
                }

                serviceIDCounter = serviceIDCounter + serviceCount;
            }

            //service ID validation other offers

            foreach (XmlNode node in otherOfferServices)
            {
                int paxjourneyRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID", xmlNamespaceManager).Count;

                int paxSegmentRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:ServiceDefinitionRef[ct:ServiceDefinitionRefID]/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                int serviceCount = node.SelectNodes("ct:Service", xmlNamespaceManager).Count;

                if (paxjourneyRefIDs != journeyCount && paxjourneyRefIDs > 0)
                {
                    result.Message.Add($"Incorrect paxjourneyRefIDs in services, actual{paxjourneyRefIDs} but exepected{journeyCount}");
                }

                if (paxSegmentRefIDs != segmentCount && paxSegmentRefIDs > 0)
                {
                    result.Message.Add($"Incorrect paxSegmentRefID in services, actual{paxSegmentRefIDs} but exepected{segmentCount}");
                }

                string serviceID = node.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;

                if (!serviceID.Is("SV" + serviceIDCounter))
                {
                    result.Message.Add($"Incorrect service ID in services, actual{serviceID} but exepected{"SV" + serviceIDCounter}");
                }

                serviceIDCounter = serviceIDCounter + serviceCount;
            }

            return result.IsValid;
        }

        public static bool ValidatePricedOfferBaggageAssociations(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            int expectedBaggageAssociationsCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:BaggageAssociations", xmlNamespaceManager).Count;

            int actualbaggageAssociations = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:BaggageAssociations[ct:BaggageAllowanceRefID=../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID and ct:PaxRefID=../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager).Count;

            int offerCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer", xmlNamespaceManager).Count;

            int baggageJourneyCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer[ct:BaggageAssociations/ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID = ct:JourneyOverview/ct:JourneyPriceClass/ct:PaxJourneyRefID]", xmlNamespaceManager).Count;

            if (offerCount != baggageJourneyCount)
            {
                result.Message.Add("Incorrect baggage journeys returned in offer");
            }

            if (!expectedBaggageAssociationsCount.Equals(actualbaggageAssociations))
            {
                result.Message.Add($"Invalid BaggageAssociations, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");
            }

            int expectedJourneyPriceClassCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:JourneyOverview/ct:JourneyPriceClass", xmlNamespaceManager).Count;

            int actualJourneyPriceClassCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:JourneyOverview/ct:JourneyPriceClass[ct:PriceClassRefID=../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID and ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!expectedJourneyPriceClassCount.Equals(actualJourneyPriceClassCount))
            {
                result.Message.Add($"Invalid priceclass, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");
            }

            decimal actualTotalAmount = 0m;
            XmlNodeList offerItems = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem", xmlNamespaceManager);

            foreach (XmlNode offerItemNode in offerItems)
            {
                int paxCount = offerItemNode.SelectNodes("ct:FareDetail/ct:PaxRefID", xmlNamespaceManager).Count;
                string baseAmount = offerItemNode.SelectSingleNode("ct:Price/ct:BaseAmount", xmlNamespaceManager).InnerText;
                string taxAmount = offerItemNode.SelectSingleNode("ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlNamespaceManager).InnerText;
                string totalamount = offerItemNode.SelectSingleNode("ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText;

                if (Math.Round(Convert.ToDecimal(totalamount), 2) != Math.Round(Convert.ToDecimal(baseAmount), 2) + Math.Round(Convert.ToDecimal(taxAmount), 2))
                {
                    result.Message.Add($"Incorrect Base amount, tax amount and total amount in offeritem , Base amount{baseAmount} taxamount {taxAmount} total amount {totalamount} ");
                }

                actualTotalAmount = (Convert.ToDecimal(totalamount) * paxCount) + actualTotalAmount;

            }

            return result.IsValid;
        }

        public static bool ValidateOtherOffersFareComponent(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            int expectedserviceCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:Service", xmlNamespaceManager).Count;

            int expectedCancelRestrictionCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions", xmlNamespaceManager).Count;

            int expectedChangeRestrictionCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions", xmlNamespaceManager).Count;

            int expectedfareComponentCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).Count;

            int actualfareComponentCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent[ct:PaxSegmentRefID=../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:PriceClassRefID=../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID]", xmlNamespaceManager).Count;

            if (!expectedfareComponentCount.Equals(actualfareComponentCount))
            {
                result.Message.Add($"Invalid farecomponent, actual{actualfareComponentCount} but exepected{expectedfareComponentCount}");
            }

            int actualjourneyServiceCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:Service[ct:PaxRefID=../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            int actualsegmentServiceCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:Service[ct:PaxRefID=../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OfferServiceAssociation/ct:ServiceDefinitionRef/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID=../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager).Count;

            if (!expectedserviceCount.Equals(actualjourneyServiceCount + actualsegmentServiceCount))
            {
                result.Message.Add($"Invalid Service, actual{actualjourneyServiceCount + actualsegmentServiceCount} but exepected{expectedserviceCount}");
            }

            int actualCancelRestrictionCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions[ct:AllowedModificationInd]", xmlNamespaceManager).Count;

            if (!actualCancelRestrictionCount.Equals(expectedCancelRestrictionCount))
            {
                result.Message.Add($"Invalid Cancel Restriction, actual{actualCancelRestrictionCount} but exepected{expectedCancelRestrictionCount}");
            }

            int actualChangeRestrictionCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions[ct:AllowedModificationInd]", xmlNamespaceManager).Count;

            if (!actualChangeRestrictionCount.Equals(expectedChangeRestrictionCount))
            {
                result.Message.Add($"Invalid Change Restriction, actual{actualChangeRestrictionCount} but exepected{expectedChangeRestrictionCount}");
            }
            return result.IsValid;
        }

        public static bool ValidatePricedOfferFareComponent(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument airShoppingRequestXmlDocument = new XmlDocument();
            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingRequestXmlDocument.LoadXml(requestXml);
            offerPriceXmlDocument.LoadXml(responseXml);

            int expectedserviceCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:Service", xmlNamespaceManager).Count;

            int expectedCancelRestrictionCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions", xmlNamespaceManager).Count;

            int expectedChangeRestrictionCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions", xmlNamespaceManager).Count;

            int expectedfareComponentCount = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).Count;

            int actualfareComponentCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent[ct:PaxSegmentRefID=../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:PriceClassRefID=../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID]", xmlNamespaceManager).Count;

            if (!expectedfareComponentCount.Equals(actualfareComponentCount))
            {
                result.Message.Add($"Invalid farecomponent, actual{actualfareComponentCount} but exepected{expectedfareComponentCount}");
            }

            int actualjourneyServiceCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            int actualsegmentServiceCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OfferServiceAssociation/ct:ServiceDefinitionRef/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager).Count;

            if (!expectedserviceCount.Equals(actualjourneyServiceCount + actualsegmentServiceCount))
            {
                result.Message.Add($"Invalid Service, actual{actualjourneyServiceCount + actualsegmentServiceCount} but exepected{expectedserviceCount}");
            }

            int actualCancelRestrictionCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions[ct:AllowedModificationInd]", xmlNamespaceManager).Count;

            if (!actualCancelRestrictionCount.Equals(expectedCancelRestrictionCount))
            {
                result.Message.Add($"Invalid Cancel Restriction, actual{actualCancelRestrictionCount} but exepected{expectedCancelRestrictionCount}");
            }

            int actualChangeRestrictionCount = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions[ct:AllowedModificationInd]", xmlNamespaceManager).Count;

            if (!actualChangeRestrictionCount.Equals(expectedChangeRestrictionCount))
            {
                result.Message.Add($"Invalid Change Restriction, actual{actualChangeRestrictionCount} but exepected{expectedChangeRestrictionCount}");
            }
            return result.IsValid;
        }

        public static bool ValidateExpirationTTL(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument offerPriceXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(offerPriceXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            offerPriceXmlDocument.LoadXml(responseXml);

            int offerIdCount = offerPriceXmlDocument.SelectNodes("//ct:OfferID", xmlNamespaceManager).Count;
            int expirationTtlCount = offerPriceXmlDocument.SelectNodes("//ct:OfferExpirationTimeLimitDateTime", xmlNamespaceManager).Count;
            int paymentTtlCount = offerPriceXmlDocument.SelectNodes("//ct:PaymentTimeLimitDateTime", xmlNamespaceManager).Count;

            if (!offerIdCount.Equals(expirationTtlCount))
            {
                result.Message.Add($"Offer count {offerIdCount} did not match Expiration TTL Count {expirationTtlCount}");
            }

            if (paymentTtlCount == 0)
            {
                result.Message.Add("Payment TTL returned as 0");
            }

            int experationTtlFilteredByTCount = offerPriceXmlDocument.SelectNodes("//ct:OfferExpirationTimeLimitDateTime//text()[contains(., 'T')]", xmlNamespaceManager).Count;

            if (!offerIdCount.Equals(experationTtlFilteredByTCount))
            {
                result.Message.Add($"Offer count {offerIdCount} did not match experation TTL filtered by T Count {paymentTtlCount}");
            }
            return result.IsValid;
        }

        public static bool ValidateFareDetail(string requestXml, string responseXml, string cabinName, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OfferPriceRequestXmlDocument = new XmlDocument();
            XmlDocument OfferPriceResponseXmlDocument = new XmlDocument();

            OfferPriceRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OfferPriceResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OfferPriceRequestXmlDocument.LoadXml(requestXml);
            OfferPriceResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add("priceClassList is not found in response");
            }

            string cabinNameResponse = OfferPriceResponseXmlDocument.SelectSingleNode("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:Name", xmlNamespaceManager).InnerText;

            if (!cabinNameResponse.Equals(cabinName, StringComparison.OrdinalIgnoreCase))
            {
                result.Message.Add("cabin Name returned in response is not as expected");
            }

            string priceClassIDResponse = OfferPriceResponseXmlDocument.SelectSingleNode("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID", xmlNamespaceManager).InnerText;

            int paxJourneysCount = OfferPriceRequestXmlDocument.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria", xmlNamespaceManager).Count;

            int segmentCount = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int paxCount = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNodeList fareComponent = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);

            int offerItemCount = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem", xmlNamespaceManager).Count;

            int mandatoryIndCount = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:MandatoryInd[text()='true']", xmlNamespaceManager).Count;

            if (offerItemCount != mandatoryIndCount)
            {
                result.Message.Add("mandatoryIndicator is not returned in all offerItems");
            }

            if (fareComponent.Count != (paxJourneysCount * offerItemCount))
            {
                result.Message.Add("farecomponent are not as expected");
            }

            int segmentflag = 0;

            foreach (XmlNode node in fareComponent)
            {
                segmentflag++;
                //validate price class in farecomponent
                string priceClassRefID = node.SelectSingleNode("ct:PriceClassRefID", xmlNamespaceManager).InnerText;

                XmlNodeList priceClass = OfferPriceResponseXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[(ct:PriceClassID='{priceClassRefID}')]", xmlNamespaceManager);

                if (!priceClass.Count.Equals(1))
                {
                    result.Message.Add($"priceClassRefID is incorrect in fare component");
                }

                string desc = node.SelectSingleNode("ct:PriceClassRefID", xmlNamespaceManager).InnerText;

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


                //validate farebasiscode in farecomponent
                string fareBasisCode = node.SelectSingleNode("ct:FareBasisCode", xmlNamespaceManager).InnerText;

                if (string.IsNullOrEmpty(fareBasisCode))
                {
                    result.Message.Add("FareBasisCode is incorrect in fare component");
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

                if (segmentCount == segmentflag)
                    segmentflag = 0;

            }

            return result.IsValid;
        }

        public static bool ValidateFareDetailOtherOffers(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OfferPriceRequestXmlDocument = new XmlDocument();
            XmlDocument OfferPriceResponseXmlDocument = new XmlDocument();

            OfferPriceRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OfferPriceResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OfferPriceRequestXmlDocument.LoadXml(requestXml);
            OfferPriceResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add("priceClassList is not found in response");
            }

            string priceClassIDResponse = OfferPriceResponseXmlDocument.SelectSingleNode("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID", xmlNamespaceManager).InnerText;

            int paxJourneysCount = OfferPriceRequestXmlDocument.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria", xmlNamespaceManager).Count;

            int segmentCount = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int paxCount = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNodeList otherOffers = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer", xmlNamespaceManager);

            foreach (XmlNode otherOffer in otherOffers)
            {
                XmlNodeList fareComponent = otherOffer.SelectNodes("ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);

                int offerItemCount = otherOffer.SelectNodes("ct:OfferItem", xmlNamespaceManager).Count;

                int mandatoryIndCount = otherOffer.SelectNodes("ct:OfferItem/ct:MandatoryInd[text()='true']", xmlNamespaceManager).Count;

                if (offerItemCount != mandatoryIndCount)
                {
                    result.Message.Add("missing mandatoryInd in offer items");
                }

                if (fareComponent.Count != (paxJourneysCount * offerItemCount))
                {
                    result.Message.Add("farecomponents are not as expected");
                }

                foreach (XmlNode node in fareComponent)
                {
                    //validate price class in farecomponent
                    string priceClassRefID = node.SelectSingleNode("ct:PriceClassRefID", xmlNamespaceManager).InnerText;

                    XmlNodeList priceClass = OfferPriceResponseXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[(ct:PriceClassID='{priceClassRefID}')]", xmlNamespaceManager);

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

                    //validate farebasiscode in farecomponent
                    string fareBasisCode = node.SelectSingleNode("ct:FareBasisCode", xmlNamespaceManager).InnerText;

                    if (string.IsNullOrEmpty(fareBasisCode))
                    {
                        result.Message.Add("FareBasisCode is incorrect in fare component");
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
                        }
                    }
                }

            }

            return result.IsValid;
        }

        public static bool ValidateFareCodeType(string responseXml, out ValidationResultHelper result, string FareTypeCode = "70J", string SubTypeCode = null)
        {
            result = new ValidationResultHelper();

            XmlDocument offerPriceXmlDocument = new XmlDocument();

            offerPriceXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = offerPriceXmlDocument.CreateXmlNamespaceManager();

            // Select FareComponent nodes from PricedOffer
            XmlNodeList pricedFareComponents = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:PricedOffer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);

            // Select FareComponent nodes from OtherOffers
            XmlNodeList otherOffersFareComponents = offerPriceXmlDocument.SelectNodes($"msg:IATA_OfferPriceRS/msg:Response/ct:OtherOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);

            // Merge both node lists into a single collection
            List<XmlElement> allFareComponents = new List<XmlElement>();

            if (pricedFareComponents != null)
            {
                foreach (XmlNode node in pricedFareComponents)
                {
                    if (node is XmlElement el)
                        allFareComponents.Add(el);
                }
            }

            if (otherOffersFareComponents != null)
            {
                foreach (XmlNode node in otherOffersFareComponents)
                {
                    if (node is XmlElement el)
                        allFareComponents.Add(el);
                }
            }

            foreach (XmlElement element in allFareComponents)
            {
                string fareTypeCodeValue = element["FareTypeCode"]?.InnerText;
                string negotiatedCodeValue = element["NegotiatedCode"]?.InnerText;

                if (negotiatedCodeValue.Is("70J"))
                {
                    if (!string.Equals(FareTypeCode, negotiatedCodeValue, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add($"Invalid NegotiatedCode: expected '{FareTypeCode}', actual '{negotiatedCodeValue}'.");
                    }
                }

                if (negotiatedCodeValue.Is("70G") || negotiatedCodeValue.Is("758"))
                {
                    if (fareTypeCodeValue == null)
                    {
                        result.Message.Add("FareTypeCode element is missing.");
                        continue;
                    }

                    if (!SubTypeCode.Is(fareTypeCodeValue) || !FareTypeCode.Is(negotiatedCodeValue))
                    {
                        result.Message.Add($"Invalid codes: expected FareTypeCode = '{SubTypeCode}', actual = '{fareTypeCodeValue}'; expected NegotiatedCode = '{FareTypeCode}', actual = '{negotiatedCodeValue}'.");
                    }
                }
            }
            return result.IsValid;
        }
       
        //Aircraft type code and Name
        public static bool ValidateAirCarftTypeCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OfferPriceResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OfferPriceResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OfferPriceResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList datedOperatingSegments = OfferPriceResponseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

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

        public static bool ValidateOperatingLeg(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName and ct:IATA_AircraftType/ct:IATA_AircraftTypeCode]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");
                }
            }
            return result.IsValid;
        }

        //form of Payments - non-IATA agents assertion
        public static bool ValidateFormOfPayments(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList paymentType = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod", xmlNamespaceManager);

            foreach (XmlNode paymentNode in paymentType)
            {
                string brandCode = paymentNode.SelectSingleNode("ct:PaymentBrandCode", xmlNamespaceManager)?.InnerText;

                string typeCode = paymentNode.SelectSingleNode("ct:PaymentTypeCode", xmlNamespaceManager)?.InnerText;

                if (!CreditCardsVendor.CardsVendor.Contains(brandCode) && !typeCode.Is("CA"))
                {
                    result.Message.Add($"Invalid Payment brandCode {brandCode}");
                }

                if (!typeCode.Is("CC") && !typeCode.Is("CA") && !typeCode.Is("AC"))
                {
                    result.Message.Add($"Invalid Payment type code {typeCode}");
                }

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

            string requestedCabinTypeCode = requestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int expectedpriceClassCount = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            XmlNodeList actualpriceclass = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[ct:CabinType/ct:CabinTypeCode=../../ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode and ct:CabinType/ct:CabinTypeName=../../ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeName]", xmlNamespaceManager);

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

                if (cabinCode.Equals("4") && !cabinName.Equals("Premium", StringComparison.InvariantCultureIgnoreCase))
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

        public static bool ValidateOrdinalAirshopping(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(responseXml);

            XmlNode codeNode = responseXmlDocument.SelectSingleNode("//ct:Code", xmlNamespaceManager);
            XmlNode DescpText = responseXmlDocument.SelectSingleNode("//ct:DescText", xmlNamespaceManager);

            // Extract error code from response
            string codeInnerText = codeNode.InnerText;
            string DescpText_InnerText = DescpText.InnerText;

            string expected_Error_Code = "368";
            string expected_Error_Description = "Seller is not authorised to access this service - No Ticketing Authority";

            // Check if error code matches expected
            if (codeInnerText != expected_Error_Code)
            {
                result.Message.Add($"Error: Unexpected error code. Expected: {expected_Error_Code}. Actual: {responseText}.");
            }

            // Check if error description matches expected
            if (!DescpText_InnerText.Equals(expected_Error_Description))
            {
                result.Message.Add($"Error: Error description mismatch. Expected: {expected_Error_Description}. Actual: {codeInnerText}.");
            }

            return result.IsValid;
        }

        public static bool ValidateTerminalName(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(responseXml);

            XmlNodeList datedMarketingSegments = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager);
            XmlNodeList datedOperatingLegs = responseXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

            foreach (XmlNode datedMarketingSegment in datedMarketingSegments)
            {
                string arrivalCode = datedMarketingSegment.SelectSingleNode("ct:Arrival/ct:IATA_LocationCode", xmlNamespaceManager)?.InnerText;
                string departureCode = datedMarketingSegment.SelectSingleNode("ct:Dep/ct:IATA_LocationCode", xmlNamespaceManager)?.InnerText;

                string[] airportCodesToCheck = new[] { "SEA", "TPA", "MBJ", "NAS", "PLS", "ANU", "UVF", "BVD", "GND", "BGI", "CPT", "MLE", "EDI" };

                // Check if both arrival and departure codes are present
                if (!string.IsNullOrEmpty(arrivalCode) && !string.IsNullOrEmpty(departureCode))
                {
                    foreach (string code in airportCodesToCheck)
                    {
                        // Check if the code matches arrival or departure
                        if (code == arrivalCode || code == departureCode)
                        {
                            // Skip terminal checking for this code
                            continue;
                        }

                        // Otherwise, check for terminal
                        XmlNodeList terminalNode = datedMarketingSegment.SelectNodes("ct:TerminalName", xmlNamespaceManager);
                        if (terminalNode == null)
                        {
                            result.Message.Add($"Terminal info is missing for airport code '{code}' in the DatedMarketingSegmentList.");
                        }
                    }
                }
            }

            foreach (XmlNode datedOperatingLeg in datedOperatingLegs)
            {
                string arrivalCode = datedOperatingLeg.SelectSingleNode("ct:Arrival/ct:IATA_LocationCode", xmlNamespaceManager)?.InnerText;
                string departureCode = datedOperatingLeg.SelectSingleNode("ct:Dep/ct:IATA_LocationCode", xmlNamespaceManager)?.InnerText;

                string[] airportCodesToCheck = new[] { "SEA", "TPA", "MBJ", "NAS", "PLS", "ANU", "UVF", "BVD", "GND", "BGI", "CPT", "MLE", "EDI" };

                // Check if both arrival and departure codes are present
                if (!string.IsNullOrEmpty(arrivalCode) && !string.IsNullOrEmpty(departureCode))
                {
                    foreach (string code in airportCodesToCheck)
                    {
                        // Check if the code matches arrival or departure
                        if (code == arrivalCode || code == departureCode)
                        {
                            // Skip terminal checking for this code
                            continue;
                        }

                        // Otherwise, check for terminal
                        XmlNodeList terminalNode = datedOperatingLeg.SelectNodes("ct:TerminalName", xmlNamespaceManager);
                        if (terminalNode == null)
                        {
                            result.Message.Add($"Terminal info is missing for airport code '{code}' in the DatedMarketingSegmentList.");
                        }
                    }
                }
            }


            return result.IsValid;
        }

        // Validating TaxSummary Country Code
        public static bool ValidateCountryCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument offerPriceXmlDocument = new XmlDocument();
            offerPriceXmlDocument.LoadXml(responseXml);
            XmlNamespaceManager xmlNamespaceManager = offerPriceXmlDocument.CreateXmlNamespaceManager();

            // Select all TaxSummary segments
            XmlNodeList searchOutYQ = offerPriceXmlDocument.SelectNodes("//ct:TaxSummary/ct:Tax[(ct:TaxCode = 'YQ' or ct:TaxCode = 'YR')]", xmlNamespaceManager);
            XmlNodeList searchTaxcode = offerPriceXmlDocument.SelectNodes("//ct:TaxSummary/ct:Tax", xmlNamespaceManager);
            XmlNodeList countryCode = offerPriceXmlDocument.SelectNodes("//ct:TaxSummary/ct:Tax/ct:Country/ct:CountryCode", xmlNamespaceManager);

            if (countryCode.Count < 1)
            {
                result.Message.Add("CountryCode is not returned in tax nodes");
            }

            if (searchOutYQ.Count + countryCode.Count != searchTaxcode.Count)
            {
                result.Message.Add("CountryCode node is not found in one of the Tax segments");
            }
            return result.IsValid;
        }

    }

}
