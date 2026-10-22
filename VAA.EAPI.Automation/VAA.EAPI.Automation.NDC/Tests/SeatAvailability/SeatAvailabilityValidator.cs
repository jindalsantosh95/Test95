using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.SeatAvailability
{
    public class SeatAvailabilityValidator
    {
        public static bool ValidateAssignedSeatWarning(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList warning = responseXmlDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:Warning[ct:DescText='One or more passenger(s) within the order have already reserved seats - Change seat is not allowed' and ct:Code='207' and ct:LangCode='EN' and ct:TagText='Request Validation Error' and ct:TypeCode='Warning']", xmlNamespaceManager);

            if (warning.Count == 0)
            {
                result.Message.Add($"incorrect seat warning returned in seat response {responseXmlDocument.SelectSingleNode("msg:IATA_SeatAvailabilityRS/msg:Response/ct:Warning", xmlNamespaceManager)?.InnerText}");
            }

            return result.IsValid;
        }

        //DatedMarketingAndOperatingList
        public static bool ValidateDatedMarketingAndOperatingList(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument seatAvailabilityDocument = new XmlDocument();

            seatAvailabilityDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityDocument.CreateXmlNamespaceManager();

            XmlNode[] datedMarketingSegmentCount = seatAvailabilityDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            IGrouping<string, XmlNode>[] datedMarketingSegments = seatAvailabilityDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[ct:DatedMarketingSegmentId=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:DatedMarketingSegmentRefId and ct:DatedOperatingSegmentRefId=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingSegmentId and ct:Arrival/ct:IATA_LocationCode != ct:Dep/ct:IATA_LocationCode  and ct:Arrival/ct:AircraftScheduledDateTime != ct:Dep/ct:AircraftScheduledDateTime]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:DatedMarketingSegmentId", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!datedMarketingSegments.Length.Equals(datedMarketingSegmentCount.Length))
            {
                result.Message.Add($"Invalid datedMarketingSegments, was expecting {datedMarketingSegmentCount.Length} but found {datedMarketingSegments.Length} instead.");

            }

            int segmentcountVS = 0;
            int nonVsSegmentCount = 0;

            XmlNode[] datedOperatingSegments = seatAvailabilityDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            foreach (XmlNode node in datedOperatingSegments)
            {
                string carrierDesignCode = node["CarrierDesigCode"].InnerText;

                if (carrierDesignCode.IsAny("DL","VS"))
                {
                    segmentcountVS++;
                }
                else
                {
                    nonVsSegmentCount++;
                }
            }

            int seatMapCount = seatAvailabilityDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:SeatMap", xmlNamespaceManager).Count;

            if (!segmentcountVS.Equals(seatMapCount))
            {
                result.Message.Add($"Mismatch in seat map and Vs segments, was expecting {segmentcountVS} but found {seatMapCount} instead.");
            }

            int nonVSFlightWarningCount = seatAvailabilityDocument.SelectNodes($"//ct:Warning[ct:Code='200' and ct:DescText]", xmlNamespaceManager).Count;

            if (nonVSFlightWarningCount != nonVsSegmentCount)
            {
                result.Message.Add($"Non VS flight warning, was expecting {nonVsSegmentCount} but found {nonVSFlightWarningCount} instead.");
            }

            return result.IsValid;
        }

        //PaxList List
        public static bool ValidatePaxList(string requestXml, string responseXml, out ValidationResultHelper result, string sale = "")
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocumentAirShoppingRequest = new XmlDocument();

            xmlDocumentAirShoppingRequest.LoadXml(requestXml);

            XmlDocument xmlDocumentSeatAvailabilityResponse = new XmlDocument();

            xmlDocumentSeatAvailabilityResponse.LoadXml(responseXml);

            XmlNamespaceManager xmlnsManager = xmlDocumentAirShoppingRequest.CreateXmlNamespaceManager();

            XmlNode paxListBlockRequest = xmlDocumentAirShoppingRequest.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/ct:PaxList", xmlnsManager);

            XmlNode paxListBlockResponse = xmlDocumentSeatAvailabilityResponse.SelectSingleNode("/msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:PaxList", xmlnsManager);

            XmlNodeList infantPax = xmlDocumentAirShoppingRequest.SelectNodes("/msg:IATA_AirShoppingRQ/msg:Request/ct:PaxList/ct:Pax[(ct:PTC = 'INF')]", xmlnsManager);

            if (infantPax.Count > 0)
            {
                XmlNodeList paxListInfantResponse = xmlDocumentSeatAvailabilityResponse.SelectNodes("/msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax/ct:PaxRefID", xmlnsManager);

                if (infantPax.Count != paxListInfantResponse.Count)
                {
                    result.Message.Add("Infant pax refID's are not mapped to the adults in the response");
                }

                foreach (XmlNode node in paxListInfantResponse)
                {
                    node.ParentNode.RemoveChild(node);
                }

                XmlNode infantRemovedResponse = xmlDocumentSeatAvailabilityResponse.SelectSingleNode("/msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:PaxList", xmlnsManager);

                if (!paxListBlockRequest.OuterXml.Is(infantRemovedResponse.OuterXml))
                {
                    result.Message.Add($"Expected paxlist{paxListBlockRequest.OuterXml} but actual is {infantRemovedResponse.OuterXml}");
                }
            }
            else
            {
                if (!paxListBlockRequest.OuterXml.Is(paxListBlockResponse.OuterXml))
                {
                    result.Message.Add($"Expected paxlist{paxListBlockRequest.OuterXml} but actual is {paxListBlockResponse.OuterXml}");
                }
            }

            int childCount = xmlDocumentSeatAvailabilityResponse.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[(ct:PTC = 'CHD')]", xmlnsManager).Count;

            if (childCount > 0)
            {
                int exitCount = xmlDocumentSeatAvailabilityResponse.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[(ct:ServiceCode = 'EXT')]", xmlnsManager).Count;

                if (exitCount > 0)
                {
                    result.Message.Add($"exit sesats should not be returned in response when child pax is presnet");

                }
            }

            return result.IsValid;
        }

        //ALaCarteOffer
        public static bool ValidateAlaCarteOffer(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            xmlDocument.LoadXml(responseXml);

            int allOfferItems = xmlDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:ALaCarteOffer/ct:OfferItem", xmlnsManager).Count;

            if (allOfferItems == 0)
            {
                result.Message.Add("No alcartOfferItems found");

            }

            int filteredOfferItems = xmlDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:ALaCarteOffer/ct:OfferItem[ct:Eligibility/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID=../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Eligibility/ct:PaxRefID=../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlnsManager).Count;

            if (allOfferItems != filteredOfferItems)
            {
                result.Message.Add($"All Offer Items count was {allOfferItems} and Filtered Offer Items was {filteredOfferItems}. Expected them to be the same");

            }

            return result.IsValid;

        }

        //ServiceDefinitionList
        public static bool ValidateServiceDefinitionList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");
            xmlnsManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            xmlDocument.LoadXml(responseXml);

            int serviceDefinitionCount = xmlDocument.SelectNodes("//ct:ServiceDefinition/ct:Name", xmlnsManager).Count;

            int serviceDefinitionListNameNotBlankCheck = xmlDocument.SelectNodes("//ct:ServiceDefinition/ct:Name[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;

            if (serviceDefinitionCount != serviceDefinitionListNameNotBlankCheck)
            {
                result.Message.Add($"Service Definition Count {serviceDefinitionCount} was not equal to count returned from empty Name field check being {serviceDefinitionListNameNotBlankCheck}");

            }

            int ownerCodeNotBlankCheck = xmlDocument.SelectNodes("//ct:ServiceDefinition/ct:OwnerCode[text()='VS']", xmlnsManager).Count;

            if (serviceDefinitionCount != ownerCodeNotBlankCheck)
            {
                result.Message.Add($"Service Definition Count {serviceDefinitionCount} was not equal to count returned from VS check in OwnerCode field being {ownerCodeNotBlankCheck}");

            }

            int rFicInnerTextCheck = xmlDocument.SelectNodes("//ct:ServiceDefinition/ct:RFIC[text()='I']", xmlnsManager).Count;

            if (serviceDefinitionCount != rFicInnerTextCheck)
            {
                result.Message.Add($"Service Definition Count {serviceDefinitionCount} was not equal to count returned from RFIC check for I being {rFicInnerTextCheck}");

            }

            XmlDocument xmlDocumentAirShoppingRequest = new XmlDocument();

            xmlDocumentAirShoppingRequest.LoadXml(responseXml);
            string cabinTypeValueFromRequest = xmlDocumentAirShoppingRequest.SelectSingleNode("/msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria/ct:CabinType/ct:CabinTypeCode", xmlnsManager)?.InnerText;

            if (cabinTypeValueFromRequest == "5")
            {
                int serviceCodeContainsSpecifiedValues = xmlDocument.SelectNodes("//ct:ServiceDefinition/ct:ServiceCode[contains('EXT UPD STD', text())]", xmlnsManager).Count;

                if (serviceDefinitionCount != serviceCodeContainsSpecifiedValues)
                {
                    result.Message.Add($"Service Definition Count {serviceDefinitionCount} was not equal to count returned from Service Code check for specified values being {rFicInnerTextCheck}");

                }
            }

            int serviceDefinitionIdCount = xmlDocument.SelectNodes("//ct:ServiceDefinition/ct:ServiceDefinitionID", xmlnsManager).Count;

            if (serviceDefinitionCount != serviceDefinitionIdCount)
            {
                result.Message.Add($"Service Definition Count {serviceDefinitionCount} was not equal to count returned Service Definition ID Count being {serviceDefinitionIdCount}");

            }

            return result.IsValid;
        }

        //DisclosureAndSeatProfile
        public static bool ValidateDisclosureAndSeatProfile(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            xmlDocument.LoadXml(responseXml);

            int disclosureUrlCount = xmlDocument.SelectNodes("//ct:Disclosure/ct:Desc/ct:URL", xmlnsManager).Count;
            int disclosureUrlNotNull = xmlDocument.SelectNodes("//ct:Disclosure/ct:Desc/ct:URL[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;

            if (disclosureUrlCount != disclosureUrlNotNull)
            {
                result.Message.Add($"URL Count {disclosureUrlCount} was not equal to count returned from URL null check being {disclosureUrlNotNull}");

            }

            int descTextCount = xmlDocument.SelectNodes("//ct:Disclosure/ct:Desc/ct:DescText", xmlnsManager).Count;
            int descTextUniqueTextCheck = xmlDocument.SelectNodes("//ct:Disclosure/ct:Desc/ct:DescText/text()[not(.= preceding::node()/text())]", xmlnsManager).Count;

            if (descTextCount != descTextUniqueTextCheck)
            {
                result.Message.Add($"DescText count {descTextCount} was not equal to count returned DescText unique text value count being {descTextUniqueTextCheck}");

            }

            int disclosureCount = xmlDocument.SelectNodes("//ct:Disclosure", xmlnsManager).Count;
            int disclosureIdCount = xmlDocument.SelectNodes("//ct:Disclosure/ct:DisclosureID", xmlnsManager).Count;

            if (disclosureCount != disclosureIdCount)
            {
                result.Message.Add($"Desclosure count {disclosureCount} was not equal to count returned Disclosure ID count being {disclosureIdCount}");

            }

            int seatProfileUrlCount = xmlDocument.SelectNodes("//ct:SeatProfile/ct:MarketingInfo/ct:URL", xmlnsManager).Count;
            int seatProfileUurlNotNull = xmlDocument.SelectNodes("//ct:SeatProfile/ct:MarketingInfo/ct:URL[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;

            if (seatProfileUrlCount != seatProfileUurlNotNull)
            {
                result.Message.Add($"URL Count {seatProfileUrlCount} was not equal to count returned from URL null check being {seatProfileUurlNotNull} within the Seat Profile block");

            }

            int seatProfileDescTextCount = xmlDocument.SelectNodes("//ct:SeatProfile/ct:MarketingInfo/ct:DescText", xmlnsManager).Count;
            int seatProfileDescTextUniqueTextCheck = xmlDocument.SelectNodes("//ct:SeatProfile/ct:MarketingInfo/ct:DescText/text()[not(.=  preceding::node()/text())]", xmlnsManager).Count;

            if (seatProfileDescTextCount != seatProfileDescTextUniqueTextCheck)
            {
                result.Message.Add($"DescText count {seatProfileDescTextCount} was not equal to count returned DescText unique text value count being {seatProfileDescTextUniqueTextCheck} within the Seat Profile block");

            }

            int seatProfileCount = xmlDocument.SelectNodes("//ct:SeatProfile", xmlnsManager).Count;
            int seatProfileIdCount = xmlDocument.SelectNodes("//ct:SeatProfile/ct:SeatProfileID", xmlnsManager).Count;

            if (seatProfileCount != seatProfileIdCount)
            {
                result.Message.Add($"SeatProfile count {seatProfileCount} was not equal to count returned SeatProfile ID count being {seatProfileIdCount}");

            }

            return result.IsValid;
        }

        //CabinComponent
        public static bool ValidateSeatMap(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlnsManager = xmlDocument.CreateXmlNamespaceManager();

            int cabinComponentCount = xmlDocument.SelectNodes("//ct:SeatMap/ct:CabinCompartment/ct:CabinComponent", xmlnsManager).Count;

            int cabinComponentNullCheckCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:CabinComponent[not(string-length(normalize-space(ct:CabinComponentTypeCode)) = 0) and not(string-length(normalize-space(ct:FirstRowNumber)) = 0)]", xmlnsManager).Count;

            if (cabinComponentCount != cabinComponentNullCheckCount)
            {
                result.Message.Add($"Seat Map cabin component count {cabinComponentCount} was not equal to count returned Seat Map cabin component null check count being {cabinComponentNullCheckCount}");

            }

            int deckCodeCount = xmlDocument.SelectNodes($"//ct:DeckCode", xmlnsManager).Count;
            int deckCodeValueCheckCount = xmlDocument.SelectNodes($"//ct:DeckCode[text()='Main']", xmlnsManager).Count;

            if (deckCodeCount != deckCodeValueCheckCount)
            {
                result.Message.Add($"Seat Map Deck Code count {cabinComponentCount} was not equal to count returned Seat Map Deck Code value check count being {deckCodeValueCheckCount}");

            }

            //Seat Column
            int cabinCompartmentSeatColumnCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatColumn", xmlnsManager).Count;
            int columnIdNullCheckCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatColumn/ct:ColumnID[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;
            int columnIdUniqueCheckCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatColumn/ct:ColumnID[not(. = preceding-sibling::node)]", xmlnsManager).Count;

            if (cabinCompartmentSeatColumnCount != columnIdNullCheckCount)
            {
                result.Message.Add($"Seat Map Compartment count {cabinCompartmentSeatColumnCount} was not equal to count returned within the Seat Map compartment null check count being {columnIdNullCheckCount}");

            }

            if (cabinCompartmentSeatColumnCount != columnIdUniqueCheckCount)
            {
                result.Message.Add($"Seat Map Compartment count {cabinCompartmentSeatColumnCount} was not equal to count returned within the Seat Map compartment unique value check count being {columnIdUniqueCheckCount}");

            }

            //SeatRow
            int seatCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatRow/ct:Seat", xmlnsManager).Count;

            int rowNumberNullCheckCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatRow/ct:Seat/ct:RowNumber[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;
            int seatRowColumnIdNullCheckCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatRow/ct:Seat/ct:ColumnID[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;
            int occupationStatusCodeNullCheckCount = xmlDocument.SelectNodes($"//ct:SeatMap/ct:CabinCompartment/ct:SeatRow/ct:Seat/ct:OccupationStatusCode[not(string-length(normalize-space()) = 0)]", xmlnsManager).Count;

            if (rowNumberNullCheckCount != rowNumberNullCheckCount)
            {
                result.Message.Add($"Seat Map Seat Count {rowNumberNullCheckCount} was not equal to count returned within the Seat Map seat row number null check count being {rowNumberNullCheckCount}");

            }

            if (rowNumberNullCheckCount != seatRowColumnIdNullCheckCount)
            {
                result.Message.Add($"Seat Map Seat Count {rowNumberNullCheckCount} was not equal to count returned within the Seat Map seat row column id null check count being {seatRowColumnIdNullCheckCount}");

            }

            if (rowNumberNullCheckCount != occupationStatusCodeNullCheckCount)
            {
                result.Message.Add($"Seat Map Seat Count {rowNumberNullCheckCount} was not equal to count returned within the Seat Map seat Occupation status code null null check count being {occupationStatusCodeNullCheckCount}");

            }
            return result.IsValid;
        }

        //AircraftDetails
        public static bool ValidateAirCarftTypeCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument SeatAvailablityResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(SeatAvailablityResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            SeatAvailablityResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList datedOperatingLegSegments = SeatAvailablityResponseXmlDocument.SelectNodes("msg:SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

            foreach (XmlNode datedOperatingLegSegment in datedOperatingLegSegments)
            {
                XmlNode AirCraftTypecodeNode = datedOperatingLegSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeCode", xmlNamespaceManager);

                if (AirCraftTypecodeNode is null)
                {
                    result.Message.Add("AircrafttypeCode node is not found in one of the DatedOperatingSegments");

                }

                XmlNode AircraftTypeNameNode = datedOperatingLegSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeName", xmlNamespaceManager);

                if (AircraftTypeNameNode is null)
                {
                    result.Message.Add("AircraftTypeName node is not found in one of the DatedOperatingSegments");

                }
            }

            return result.IsValid;


        }

        //DatedOperatingLegList 
        public static bool ValidateOperatingLeg(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(responseXml);

            int datedMarketingSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXmlDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");

                }
            }
            return result.IsValid;
        }

        public static bool ValidateNonVSAndDlErrorScenario(string responseXml, string offerPriceResponse , out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument seatAvailabilityDocument = new XmlDocument();

            XmlDocument offerPriceDocument = new XmlDocument();

            seatAvailabilityDocument.LoadXml(responseXml);

            offerPriceDocument.LoadXml(offerPriceResponse);

            XmlNamespaceManager xmlNamespaceManager = seatAvailabilityDocument.CreateXmlNamespaceManager();

            int nonVsSegmentCount = offerPriceDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment[ct:CarrierDesigCode!='DL' or ct:CarrierDesigCode!='VS']", xmlNamespaceManager).Count;

            int errorCount = seatAvailabilityDocument.SelectNodes("msg:IATA_SeatAvailabilityRS/msg:Error[ct:Code='200' and ct:DescText]", xmlNamespaceManager).Count;

            if (errorCount != nonVsSegmentCount)
            {
                result.Message.Add($"Non VS flight error, was expecting {nonVsSegmentCount} but found {errorCount} instead.");
            }

            return result.IsValid;
        }

    }
}
