using System.Xml;
using System.Xml.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.AirShopping
{
    public class AirshoppingValidator
    {
        public static bool ValidateErrorXmlResponse(XmlDocument responseXml, string expectedErrorCode,string expectedDesxText, out ValidationResultHelper result)
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

        public static bool ValidateOffersReturned(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager airShoppingXmlnsManager = new XmlNamespaceManager(responseXml.NameTable);
            airShoppingXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            airShoppingXmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            string offersNotFound = responseXml.SelectSingleNode("/msg:IATA_AirShoppingRS/msg:Error/def:DescText", airShoppingXmlnsManager)?.InnerText;

            if (offersNotFound != null)
            {
                result.Message.Add("No Offers Found in the AirShopping response");
            }

            return result.IsValid;
        }

        public static bool ValidateOperatingLeg(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int datedMarketingSegmentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID ]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");
                }
            }

            return result.IsValid;
        }

        public static bool ValidatePaxList(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager airShoppingXmlnsManager = requestXml.CreateXmlNamespaceManager();

            XmlDocument paxListXmlDocument = new XmlDocument();

            XmlNode expectedPaxList = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:PaxList", airShoppingXmlnsManager);
            XmlNode actualPaxList = responseXml.SelectSingleNode("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxList", airShoppingXmlnsManager);

            XmlElement requestPaxList = paxListXmlDocument.CreateElement("Request");
            XmlElement responsePaxList = paxListXmlDocument.CreateElement("Response");

            requestPaxList.AppendChild(paxListXmlDocument.ImportNode(expectedPaxList.CloneNode(true), true));
            responsePaxList.AppendChild(paxListXmlDocument.ImportNode(actualPaxList.CloneNode(true), true));

            XmlElement datalistXmlNode = paxListXmlDocument.CreateElement("Datalist");

            datalistXmlNode.AppendChild(requestPaxList);
            datalistXmlNode.AppendChild(responsePaxList);

            paxListXmlDocument.AppendChild(datalistXmlNode);

            //check for infant pax
            int expectedInfantPaxCount = requestXml.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:PaxList/ct:Pax/ct:PaxID[contains(text(), 'INFANT')]", airShoppingXmlnsManager).Count;
            int actualInfantPaxCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:PaxID = ../../../Response/ct:PaxList/ct:Pax/ct:PaxRefID]", airShoppingXmlnsManager).Count;
            
            if (expectedInfantPaxCount != actualInfantPaxCount)
            {
                result.Message.Add($"Count of Infants within Adult Pax is not as expected. Expected: {expectedInfantPaxCount} and actual: {actualInfantPaxCount})");
            }

            int expectedPaxListCount = expectedPaxList.SelectNodes("ct:Pax", airShoppingXmlnsManager).Count;
            int actualPaxListCount = actualPaxList.SelectNodes("ct:Pax", airShoppingXmlnsManager).Count;
            int validatedPaxListCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:PaxID = ../../../Response/ct:PaxList/ct:Pax/ct:PaxID]", airShoppingXmlnsManager).Count;

            if (actualPaxListCount != validatedPaxListCount)
            {
                result.Message.Add($"incorrect paxlist returned in response expected: {expectedPaxList.OuterXml} and actual: {actualPaxList.OuterXml})");
            }

            return result.IsValid;
        }

        public static bool ValidateCabinType(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager airShoppingXmlnsManager = requestXml.CreateXmlNamespaceManager();

            string cabinTypeValueFromRequest = requestXml.SelectSingleNode("//ct:CabinTypeCode", airShoppingXmlnsManager)?.InnerText;

            XmlNodeList cabinTypeResponseFilteredCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode[.='{cabinTypeValueFromRequest}']", airShoppingXmlnsManager);

            if (cabinTypeResponseFilteredCount?.Count == 0)
            {
                result.Message.Add($"Cabin Type value from Air Shopping Request: {cabinTypeValueFromRequest} was not returned for all CabinTypeCode nodes in the Air Shopping response being {cabinTypeResponseFilteredCount?.Count})");
            }

            return result.IsValid;
        }

        public static bool ValidateOriginDestList(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlDocument orgDestListXmlDocument = new XmlDocument();

            XmlNode expectedOrgDestList = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria", xmlNamespaceManager);
            XmlNode actualOrgDestList = responseXml.SelectSingleNode("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:OriginDestList", xmlNamespaceManager);

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

            int? paxJourneys = responseXml?.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest[ct:PaxJourneyRefID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID ]", xmlNamespaceManager)?.Count;

            if (!paxJourneys.Equals(expectedOrgDestListCount) && actualOrgDestListCount < expectedOrgDestListCount)
            {
                result.Message.Add("Journey ID in Origin DestList is not found in PaxJOurneyList");
            }

            return result.IsValid;
        }

        public static bool ValidatePaxJourneylist(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int? expectedPaxJourneyCount = responseXml?.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney", xmlNamespaceManager)?.Count;

            IGrouping<string, XmlNode>[] paxJourneys = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxJourneyList/ct:PaxJourney[ct:PaxJourneyID=../../../ct:DataLists/ct:OriginDestList/ct:OriginDest/ct:PaxJourneyRefID and ct:PaxSegmentRefID=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxJourneyID", xmlNamespaceManager)?.InnerText)
                                                            .ToArray();

            if (!paxJourneys.Length.Equals(expectedPaxJourneyCount))
            {
                result.Message.Add($"Invalid PaxJourneys, was expecting {expectedPaxJourneyCount} but found {paxJourneys.Length} instead.");
            }

            return result.IsValid;
        }

        public static bool ValidateDatedMarketingAndOperatingList(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int datedMarketingSegmentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] datedMarketingSegments = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[ct:DatedMarketingSegmentId=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:DatedMarketingSegmentRefId and ct:DatedOperatingSegmentRefId=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingSegmentId and ct:Arrival/ct:IATA_LocationCode != ct:Dep/ct:IATA_LocationCode  and ct:Arrival/ct:AircraftScheduledDateTime != ct:Dep/ct:AircraftScheduledDateTime]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:DatedMarketingSegmentId", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            int secureFlightIndCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment[ct:SecureFlightInd='true']", xmlNamespaceManager).Count;

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

        public static bool ValidatePaxSegmentList(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            IDictionary<int, string> cabinTypeCodeAndNames = new Dictionary<int, string>();
            cabinTypeCodeAndNames.Add(2, "Business");
            cabinTypeCodeAndNames.Add(4, "Premium Economy");
            cabinTypeCodeAndNames.Add(5, "Economy");

            string expectedCabinTypeCode = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int paxSegmentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            IGrouping<string, XmlNode>[] paxSegments = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[ct:PaxSegmentID=../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxSegmentRefID and ct:DatedMarketingSegmentRefId=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:DatedMarketingSegmentId]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxSegmentID", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!paxSegments.Length.Equals(paxSegmentCount))
            {
                result.Message.Add($"Invalid paxSegments, was expecting {paxSegmentCount} but found {paxSegments.Length} instead.");
            }

            int paxSegmentIdCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID", xmlNamespaceManager).Count;

            string cabinTypeValueFromRequest = requestXml.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager)?.InnerText;

            int cabinTypeCodes = responseXml.SelectNodes($"//ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode[.='{cabinTypeValueFromRequest}']", xmlNamespaceManager).Count;

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
                string paxSegmentId = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{i}]/ct:PaxSegmentID", xmlNamespaceManager).InnerText;

                if (!paxSegmentId.Equals("SEG" + i))
                {
                    result.Message.Add($"paxsegmentId is not as expected SEG{i} actual {paxSegmentId}");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateBaggageAllowancelist(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList baggageAllowanceCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance", xmlNamespaceManager);

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

                    if (weightAllowance.Count != 2)
                    {
                        result.Message.Add("weightAllowances are not returned");
                    }

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




            }

            return result.IsValid;
        }

        public static bool ValidatePriceClassList(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string requestedCabinTypeCode = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int priceClassCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            for (int i = 1; i <= priceClassCount; i++)
            {
                string name = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                if (requestedCabinTypeCode.Equals("5"))
                {
                    if (!name.Equals("Economy Classic") && !name.Equals("Economy Classic Flex") && !name.Equals("Economy Light") && !name.Equals("Economy Delight") && !name.Equals("Economy Delight Flex"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");

                    }
                }
                else if (requestedCabinTypeCode.Equals("4"))
                {
                    if (!name.Equals("Premium") && !name.Equals("Premium Flex"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");

                    }
                }
                else
                {
                    if (!name.Equals("Upper Class") && !name.Equals("Upper Class Flex"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");

                    }
                }

                string priceClassID = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                if (!priceClassID.Equals("PC" + i))
                {
                    result.Message.Add("Price class ID is not as expected");

                }
            }
            return result.IsValid;
        }

        public static bool ValidateBaggageAssociations(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int expectedBaggageAssociationsCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:BaggageAssociations", xmlNamespaceManager).Count;

            int actualbaggageAssociations = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:BaggageAssociations[ct:BaggageAllowanceRefID=../../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID and ct:PaxRefID=../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager).Count;

            int offerCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer", xmlNamespaceManager).Count;

            int baggageJourneyCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer[ct:BaggageAssociations/ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID = ct:JourneyOverview/ct:JourneyPriceClass/ct:PaxJourneyRefID]", xmlNamespaceManager).Count;

            if (offerCount != baggageJourneyCount)
            {
                result.Message.Add("Incorrect baggage journeys returned in offer");

            }

            if (!expectedBaggageAssociationsCount.Equals(actualbaggageAssociations))
            {
                result.Message.Add($"Invalid BaggageAssociations, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            int expectedJourneyPriceClassCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass", xmlNamespaceManager).Count;

            int actualJourneyPriceClassCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass[ct:PriceClassRefID=../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID and ct:PaxJourneyRefID=../../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!expectedJourneyPriceClassCount.Equals(actualJourneyPriceClassCount))
            {
                result.Message.Add($"Invalid priceclass, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            XmlNodeList offers = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer", xmlNamespaceManager);

            foreach (XmlNode node in offers)
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

            return result.IsValid;
        }

        public static bool ValidateFareComponent(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int journeyCount = requestXml.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria", xmlNamespaceManager).Count;

            XmlNodeList expectedserviceCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem", xmlNamespaceManager);

            int expectedCancelRestrictionCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions", xmlNamespaceManager).Count;

            int expectedChangeRestrictionCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions", xmlNamespaceManager).Count;

            int expectedfareComponentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).Count;

            int actualfareComponentCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent[ct:PaxSegmentRefID=../../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:PriceClassRefID=../../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID]", xmlNamespaceManager).Count;

            if (!expectedfareComponentCount.Equals(actualfareComponentCount))
            {
                result.Message.Add($"Invalid farecomponent, actual{actualfareComponentCount} but exepected{expectedfareComponentCount}");
            }

            int actualServiceCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:Service[ct:PaxRefID=../../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!expectedserviceCount.Count.Equals(actualServiceCount))
            {
                result.Message.Add($"Invalid Service, actual{actualServiceCount} but exepected{expectedserviceCount.Count}");
            }

            int actualCancelRestrictionCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions[ct:AllowedModificationInd and ct:DescText]", xmlNamespaceManager).Count;

            if (!actualCancelRestrictionCount.Equals(expectedCancelRestrictionCount))
            {
                result.Message.Add($"Invalid Cancel Restriction, actual{actualCancelRestrictionCount} but exepected{expectedCancelRestrictionCount}");
            }

            int actualChangeRestrictionCount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions[ct:AllowedModificationInd and ct:DescText]", xmlNamespaceManager).Count;

            if (!actualChangeRestrictionCount.Equals(expectedChangeRestrictionCount))
            {
                result.Message.Add($"Invalid Change Restriction, actual{actualChangeRestrictionCount} but exepected{expectedChangeRestrictionCount}");
            }

            int serviceIDCounter = 0;

            foreach(XmlNode node in expectedserviceCount)
            {
                serviceIDCounter++;

                int paxjourneyRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID", xmlNamespaceManager).Count;
                
                if(paxjourneyRefIDs!=journeyCount)
                {
                    result.Message.Add($"Incorrect paxjourneyRefIDs in services, actual{paxjourneyRefIDs} but exepected{journeyCount}");
                }

                string serviceID = node.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;
                
                if(!serviceID.Is("SV"+ serviceIDCounter))
                {
                    result.Message.Add($"Incorrect service ID in services, actual{serviceID} but exepected{"SV" + serviceIDCounter}");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateExpirationTTL(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = responseXml.CreateXmlNamespaceManager();

            int offerIdCount = responseXml.SelectNodes("//ct:OfferID", xmlNamespaceManager).Count;
            int offerItemIdCount = responseXml.SelectNodes("//ct:OfferItemID", xmlNamespaceManager).Count;
            int expirationTtlCount = responseXml.SelectNodes("//ct:OfferExpirationTimeLimitDateTime", xmlNamespaceManager).Count;
            int paymentTtlCount = responseXml.SelectNodes("//ct:PaymentTimeLimitDateTime", xmlNamespaceManager).Count;

            if (!offerIdCount.Equals(expirationTtlCount))
            {
                result.Message.Add($"Offer count {offerIdCount} did not match Expiration TTL Count {expirationTtlCount}");

            }

            if (paymentTtlCount == 0)
            {
                result.Message.Add("Payment TTL returned as 0");

            }

            int experationTtlFilteredByTCount = responseXml.SelectNodes("//ct:OfferExpirationTimeLimitDateTime//text()[contains(., 'T')]", xmlNamespaceManager).Count;

            int paymentTtlFilteredByTCount = responseXml.SelectNodes("//ct:PaymentTimeLimitDateTime//text()[contains(., 'T')]", xmlNamespaceManager).Count;

            if (!paymentTtlFilteredByTCount.Equals(offerItemIdCount))
            {
                result.Message.Add($"OfferItemID count {offerItemIdCount} did not match Payment TTL filtered by T Count {paymentTtlFilteredByTCount}");

            }

            if (!offerIdCount.Equals(experationTtlFilteredByTCount))
            {
                result.Message.Add($"Offer count {offerIdCount} did not match Expiration TTL filtered by T Count {experationTtlFilteredByTCount}");

            }
            return result.IsValid;
        }

        public static bool ValidateFlexFare(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string requestedCabinTypeCode = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int priceClassCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            for (int i = 1; i <= priceClassCount; i++)
            {
                string name = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                if (requestedCabinTypeCode.Equals("5"))
                {
                    if (!name.Equals("Economy Classic Flex") && !name.Equals("Economy Delight Flex"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else if (requestedCabinTypeCode.Equals("4"))
                {
                    if (!name.Equals("Premium Flex"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else
                {
                    if (!name.Equals("Upper Class Flex"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }

                string priceClassID = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                if (!priceClassID.Equals("PC" + i))
                {
                    result.Message.Add("Price class ID is not as expected");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateNonFlexFare(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string requestedCabinTypeCode = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int priceClassCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            for (int i = 1; i <= priceClassCount; i++)
            {
                string name = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                if (requestedCabinTypeCode.Equals("5"))
                {
                    if (!name.Equals("Economy Classic") && !name.Equals("Economy Light") && !name.Equals("Economy Delight"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else if (requestedCabinTypeCode.Equals("4"))
                {
                    if (!name.Equals("Premium"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }
                else
                {
                    if (!name.Equals("Upper Class"))
                    {
                        result.Message.Add("requested cabin type and cabin name in response are not same");
                    }
                }

                string priceClassID = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                if (!priceClassID.Equals("PC" + i))
                {
                    result.Message.Add("Price class ID is not as expected");
                }
            }
            return result.IsValid;
        }

        public static bool ValidateAllCabinType(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int priceClassCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            for (int i = 1; i <= priceClassCount; i++)
            {
                string name = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:Name", xmlNamespaceManager).InnerText;

                if (!name.Equals("Economy Classic") && !name.Equals("Economy Classic Flex") && !name.Equals("Economy Light") && !name.Equals("Economy Delight") && !name.Equals("Economy Delight Flex") && !name.Equals("Premium") && !name.Equals("Premium Flex") && !name.Equals("Upper Class") && !name.Equals("Upper Class Flex"))
                {
                    result.Message.Add("requested cabin type and cabin name in response are not same");

                }

                string priceClassID = responseXml.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[{i}]/ct:PriceClassID", xmlNamespaceManager).InnerText;

                if (!priceClassID.Equals("PC" + i))
                {
                    result.Message.Add("Price class ID is not as expected");

                }
            }
            return result.IsValid;
        }

        public static bool ValidatePrice(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNode[] price = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:Price", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            Double[] offerItemTotalAmount = { };

            foreach (XmlElement element in price)
            {
                double baseAmount = Convert.ToDouble(element["BaseAmount"].InnerText);

                double totalTaxAmount = Convert.ToDouble(element["TaxSummary"]["TotalTaxAmount"].InnerText);

                double totalAmount = Convert.ToDouble(element["TotalAmount"].InnerText);

                double sumOfBaseAndtotalAmount = Math.Round(baseAmount + totalTaxAmount, 2);

                if (!(sumOfBaseAndtotalAmount == totalAmount))
                {
                    result.Message.Add("sum of baseAmount and totaltaxAmount is not equal to total amount in offer item");

                }
                offerItemTotalAmount.Append(totalAmount);
            }

            XmlNode[] offerTotalAmount = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:TotalPrice", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            foreach (XmlElement element in offerTotalAmount)
            {
                string TotalAmount = element["TotalAmount"].InnerText;

                if (string.IsNullOrEmpty(TotalAmount))
                {
                    result.Message.Add(" totalAmount in offer cannot be null");

                }

            }

            return result.IsValid;
        }

        public static bool ValidateTaxonomyCode(XmlDocument requestXml, XmlDocument responseXml, bool tax_13EC, bool tax_189C, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int offerItemTaxonomyList = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem", xmlNamespaceManager).Count;
            int taxonomy189C_validate = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:AirlineTaxonomy/ct:TaxonomyCode[.='189C']", xmlNamespaceManager).Count;
            int taxonomy13EC_validate = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:AirlineTaxonomy/ct:TaxonomyCode[.='13EC']", xmlNamespaceManager).Count;

            if (tax_13EC == true && tax_189C == true)
            {
                if (offerItemTaxonomyList != (taxonomy13EC_validate + taxonomy189C_validate) / 2)
                {
                    result.Message.Add("Taxonomy code do not matches when both 13EC and 189C true");

                }
            }
            if (tax_13EC == true && tax_189C == false)
            {
                if (offerItemTaxonomyList != (taxonomy13EC_validate + taxonomy189C_validate))
                {
                    result.Message.Add("Taxonomy code do not matches when 13EC true and 189C false");

                }
            }
            if (tax_13EC == false && tax_189C == true)
            {
                if (offerItemTaxonomyList != (taxonomy13EC_validate + taxonomy189C_validate))
                {
                    result.Message.Add("Taxonomy code do not matches when 13EC false and 189C true");

                }
            }
            if (tax_13EC == false && tax_189C == false)
            {
                if (taxonomy13EC_validate > 0 || taxonomy189C_validate > 0)
                {
                    result.Message.Add("Taxonomy code do not matches when 13EC false and 189C false");

                }
            }

            return result.IsValid;
        }

        public static bool ValidateFareCodeType(XmlDocument responseXml, out ValidationResultHelper result, string FareTypeCode = null, string SubTypeCode = null)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = responseXml.CreateXmlNamespaceManager();

            XmlNode[] Offers = responseXml.SelectNodes($"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:OfferItem/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            foreach (XmlElement element in Offers)
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
                if (negotiatedCodeValue.Is("70G") ||
                         negotiatedCodeValue.Is("758"))
                {
                    if (fareTypeCodeValue == null)
                    {
                        result.Message.Add("FareTypeCode element is missing.");
                        continue;
                    }

                    if (!string.Equals(SubTypeCode, fareTypeCodeValue, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(FareTypeCode, negotiatedCodeValue, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Message.Add($"Invalid codes: expected FareTypeCode = '{SubTypeCode}', actual = '{fareTypeCodeValue}'; expected NegotiatedCode = '{FareTypeCode}', actual = '{negotiatedCodeValue}'.");
                    }
                }
                
            }

            return result.IsValid;
        }

        public static bool ValidatePriceClassListCabinType(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");


            string requestedCabinTypeCode = requestXml.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            int expectedpriceClassCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            XmlNodeList actualpriceclass = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass[ct:CabinType/ct:CabinTypeCode=../../ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeCode and ct:CabinType/ct:CabinTypeName=../../ct:PaxSegmentList/ct:PaxSegment/ct:CabinTypeAssociationChoice/ct:SegmentCabinType/ct:CabinTypeName]", xmlNamespaceManager);

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

        public static bool ValidateMandatoryInd(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int offerItemCount = responseXml.SelectNodes("//ct:OfferItem", xmlNamespaceManager).Count;
            int mandatoryIndCount = responseXml.SelectNodes("//ct:MandatoryInd", xmlNamespaceManager).Count;
            XmlNodeList mandatoryIndValue = responseXml.SelectNodes("//ct:MandatoryInd", xmlNamespaceManager);

            if (!offerItemCount.Equals(mandatoryIndCount))
            {
                result.Message.Add($"Offer count {offerItemCount} did not match Expiration TTL Count {mandatoryIndCount}");

            }

            foreach (XmlNode manInd in mandatoryIndValue)
            {
                if (!manInd.InnerText.Equals("true"))
                {
                    result.Message.Add($"Mandatory indicator value is not True, it is {manInd.InnerText}");

                }
            }

            return result.IsValid;
        }

        public static bool ValidateOperatingLegAirshopping(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            int datedMarketingSegmentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXml.SelectNodes("msg:IATA_AirShoppingRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName and ct:IATA_AircraftType/ct:IATA_AircraftTypeCode]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");

                }
            }

            return result.IsValid;
        }

        public static bool ValidateCurrency(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList amount = responseXml.GetElementsByTagName("Amount");

            foreach (XmlNode node in amount)

            {
                string currency = node.Attributes.GetNamedItem("CurCode").InnerText;
                string price = node.FirstChild.InnerText.ToString();
                int decimalPointIndex = price.IndexOf('.');
                int decimalPlaces = price.Length - (decimalPointIndex + 1);

                if (currency.Equals("GBP") && decimalPlaces != 2)

                {
                    result.Message.Add($"Incorrect decimal places for the currnecy code {currency}");

                }

                if (currency.Equals("USD") && decimalPlaces != 2)

                {
                    result.Message.Add($"Incorrect decimal places for the currnecy code {currency}");


                }

                if (currency.Equals("INR") && decimalPointIndex != -1)

                {
                    result.Message.Add($"Incorrect decimal places for the currnecy code {currency}");

                }

                if (currency.Equals("BHD") && decimalPlaces != 3)
                {
                    result.Message.Add($"Incorrect decimal places for the currnecy code {currency}");

                }

            }
            return result.IsValid;
        }

        public static bool ValidateOrdinalAirshopping(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNode codeNode = responseXml.SelectSingleNode("//ct:Code", xmlNamespaceManager);
            XmlNode DescpText = responseXml.SelectSingleNode("//ct:DescText", xmlNamespaceManager);

            // Extract error code from response
            string codeInnerText = codeNode.InnerText;
            string DescpText_InnerText = DescpText.InnerText;

            string expected_Error_Code = "912";
            string expected_Error_Description = "Invalid ordinal number - should start from 1";

            // Check if error code matches expected
            if (codeInnerText != expected_Error_Code)
            {
                result.Message.Add($"Error: Unexpected error code. Expected: {expected_Error_Code}. Actual: {codeInnerText}.");
                // Indicate failure if error code doesn't match expected
            }

            // Check if error description matches expected
            if (!DescpText_InnerText.Equals(expected_Error_Description))
            {
                result.Message.Add($"Error: Error description mismatch. Expected: {expected_Error_Description}. Actual: {codeInnerText}.");
                // Indicate failure if error description doesn't match expected
            }

            return result.IsValid;
        }

        public static bool ValidateDepartureTimeSearch(XmlDocument requestXml, XmlDocument responseXml, int outBoundJuorneyafterDepTimeHours, int outBoundJourneybeforeDeptTimeHours, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = responseXml.CreateXmlNamespaceManager();

            string deptDate = requestXml.SelectSingleNode("//ct:OriginDestCriteria[1]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager).InnerText;

            string returnDate = requestXml.SelectSingleNode("//ct:OriginDestCriteria[2]//ct:OriginDepCriteria//ct:Date", xmlNamespaceManager)?.InnerText;

            XmlNodeList dmsCount = responseXml.SelectNodes("//ct:DatedMarketingSegment", xmlNamespaceManager);

            XmlNode code = responseXml.SelectSingleNode("//ct:Code", xmlNamespaceManager);

            string origin = responseXml.SelectSingleNode("//ct:OriginDest[1]//ct:OriginCode", xmlNamespaceManager).InnerText;

            string dest = responseXml.SelectSingleNode("//ct:OriginDest[1]//ct:DestCode", xmlNamespaceManager).InnerText;

            if (dmsCount.Count > 0)
            {
                foreach (XmlNode dms in dmsCount)
                {
                    XmlNode dateAndTime = dms.SelectSingleNode("//ct:Dep//ct:AircraftScheduledDateTime", xmlNamespaceManager);
                    string flightTime = (dateAndTime.InnerText).Substring(11, 8);
                    string flightDate = (dateAndTime.InnerText).Substring(0, 10);

                    //Comparing the date of the flight with current date
                    string dep = dms.SelectSingleNode("//ct:Dep//ct:IATA_LocationCode", xmlNamespaceManager).InnerText;

                    if ((flightDate.Is(deptDate) && dep.Is(origin)) || (flightDate.Is(returnDate) && dep.Is(dest)))
                    {
                        int responseHours = int.Parse(flightTime.Substring(0, 2));
                        if (responseHours >= outBoundJuorneyafterDepTimeHours && responseHours <= outBoundJourneybeforeDeptTimeHours)
                        {
                            result.Message.Add($"Expected offers between {outBoundJuorneyafterDepTimeHours} to {outBoundJourneybeforeDeptTimeHours} but actual offer was {responseHours}");
                        }
                    }
                }
            }
            return result.IsValid;
        }

        public static bool ValidateTaxSummary(XmlDocument requestXml, XmlDocument responseXml, out ValidationResultHelper result)

        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXml.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList taxCode = responseXml.GetElementsByTagName("TaxCode");

            foreach (XmlElement taxCodes in taxCode)
            {
                string taxCodeValue = taxCodes.InnerText;

                if (taxCodeValue.IsNullOrEmpty())
                {
                    result.Message.Add(" tax code value cannot be null");
                }
            }

            XmlNodeList taxSegments = responseXml.SelectNodes("//ct:Tax", xmlNamespaceManager);

            foreach (XmlNode tax in taxSegments)
            {
                XmlNode amount = tax.SelectSingleNode("ct:Amount", xmlNamespaceManager);
                string amountValue = amount.InnerText;
                if (amountValue.IsNullOrEmpty())
                {
                    result.Message.Add("Amount node is not found in one of the TaxSummarySegments");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateNullValues(string xmlString, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XDocument xdoc = XDocument.Parse(xmlString);

            IEnumerable<XElement> emptyElements = xdoc.Descendants()
                                    .Where(e => string.IsNullOrEmpty(e.Value));

            if (emptyElements.Any())
            {
                foreach (XElement elem in emptyElements)
                {
                    result.Message.Add($"{elem.Name.LocalName}: '{elem.Value}'");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateDirectFlightSearch(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager xmlNamespaceManager = responseXml.CreateXmlNamespaceManager();

            XmlNodeList paxJourneyCount = responseXml.SelectNodes("//ct:PaxJourney", xmlNamespaceManager);

            foreach (XmlNode node in paxJourneyCount)
            {
                int segmentCount = node.SelectNodes("ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                if (segmentCount != 1)
                {
                    result.Message.Add("In direct flight search each journey Should have only one segment");
                }
            }

            return result.IsValid;
        }

    }
}