using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.OrderQuote
{
    public class OrderQuoteValidator
    {
        public static bool ValidateUpsellOffersPricingDetails(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            reshopResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = reshopResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList upsellOffers = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer", xmlNamespaceManager);

            foreach (XmlNode upsellOffer in upsellOffers)
            {
                XmlNodeList offerItems = upsellOffer.SelectNodes("ct:AddedOfferItem", xmlNamespaceManager);

                foreach (XmlNode offerItem in offerItems)
                {
                    decimal diffPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    decimal newPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    decimal oldPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    string differentialTypeCode = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DifferentialTypeCode", xmlNamespaceManager).InnerText;

                    decimal dueByAirlineAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueByAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    decimal dueToAirlineAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueToAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                    if (differentialTypeCode.Is("EvenExchange") && (dueByAirlineAmount != 0 || dueToAirlineAmount != 0))
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

                    if (((newPriceTotalAmount - oldPriceTotalAmount) == 0 && !differentialTypeCode.Is("EvenExchange")) || ((newPriceTotalAmount - oldPriceTotalAmount) > 0 && !differentialTypeCode.Is("AddCol")) || ((newPriceTotalAmount - oldPriceTotalAmount) < 0 && differentialTypeCode.Is("AddCol")))
                    {
                        result.Message.Add($"Incorrect Price  Differential Type {differentialTypeCode} {offerItem.OuterXml}");
                    }

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

            string requestedCabinTypeCode = requestXmlDocument.SelectSingleNode("msg: IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:ServiceOrder/ct:AddOfferItems/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;
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

        public static bool ValidateRequotePaymentFunctions(string offerPriceXml, string orderQuoteXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument offerPriceXmlDocument = new XmlDocument();
            XmlDocument orderQuoteXmlDocument = new XmlDocument();

            offerPriceXmlDocument.LoadXml(offerPriceXml);
            orderQuoteXmlDocument.LoadXml(orderQuoteXml);

            XmlNamespaceManager xmlNamespaceManager = offerPriceXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList offerPricePaymentTypeCode = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod/ct:PaymentTypeCode", xmlNamespaceManager);
            XmlNodeList offerPricePaymentBrandCode = offerPriceXmlDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod/ct:PaymentBrandCode", xmlNamespaceManager);

            XmlNodeList orderQuotePaymentTypeCode = orderQuoteXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod/ct:PaymentTypeCode", xmlNamespaceManager);
            XmlNodeList orderQuotePaymentBrandCode = orderQuoteXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod/ct:PaymentBrandCode", xmlNamespaceManager);

            if (offerPricePaymentTypeCode.Count != orderQuotePaymentTypeCode.Count)
            {
                result.Message.Add("PaymentTypeCode count is different between offer price and order quote response");

            }

            if (offerPricePaymentBrandCode.Count != orderQuotePaymentBrandCode.Count)
            {
                result.Message.Add("PaymentBrandCode count is different between offer price and order quote response");
            }

            foreach (XmlNode paymentTypeCode in offerPricePaymentTypeCode)
            {
                string offerPricePaymentTypeCodeValue = paymentTypeCode.InnerText;

                string orderQuotePaymentTypeCodeValue = orderQuoteXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod[ct:PaymentTypeCode='{offerPricePaymentTypeCodeValue}']/ct:PaymentTypeCode", xmlNamespaceManager)?.InnerText;
                if (orderQuotePaymentTypeCodeValue == null || !orderQuotePaymentTypeCodeValue.Equals(offerPricePaymentTypeCodeValue))
                {
                    result.Message.Add($"PaymentTypeCode {offerPricePaymentTypeCodeValue} in offer price response is not found or different in order quote response");
                }
            }

            foreach (XmlNode paymentBrandCode in offerPricePaymentBrandCode)
            {
                string offerPricePaymentBrandCodeValue = paymentBrandCode.InnerText;

                string orderQuotePaymentBrandCodeValue = orderQuoteXmlDocument.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:PaymentFunctions/ct:PaymentSupportedMethod[ct:PaymentBrandCode='{offerPricePaymentBrandCodeValue}']/ct:PaymentBrandCode", xmlNamespaceManager)?.InnerText;
                if (orderQuotePaymentBrandCodeValue == null || !orderQuotePaymentBrandCodeValue.Equals(offerPricePaymentBrandCodeValue))
                {
                    result.Message.Add($"PaymentBrandCode {offerPricePaymentBrandCodeValue} in offer price response is not found or different in order quote response");
                }
            }

            return result.IsValid;
        }

        public static bool ValidatePriceClassList(string requestXml, string requestXml1, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument airshopRequestXmlDocument = new XmlDocument();
            XmlDocument quoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(quoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            quoteResponseXmlDocument.LoadXml(responseXml);
            airshopRequestXmlDocument.LoadXml(requestXml1);

            int journeyCount = airshopRequestXmlDocument.SelectNodes("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria", xmlNamespaceManager).Count;

            int segmentCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            string requestedCabinTypeCodeReshop = reshopRequestXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRQ/msg:Request/ct:UpdateOrder/ct:ReshopOrder/ct:ReshopOrderChoice/ct:ServiceOrder/ct:AddOfferItems/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            string requestedCabinTypeCodeAirShopping = airshopRequestXmlDocument.SelectSingleNode("msg:IATA_AirShoppingRQ/msg:Request/ct:FlightRequest/ct:FlightRequestOriginDestinationsCriteria/ct:OriginDestCriteria[1]/ct:CabinType/ct:CabinTypeCode", xmlNamespaceManager).InnerText;

            XmlNode[] priceClass = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            if (requestedCabinTypeCodeReshop.Equals(requestedCabinTypeCodeAirShopping))
            {
                int i = 0;
                foreach (XmlElement element in priceClass)
                {
                    ++i;
                    if (element["Name"] is null)
                    {
                        result.Message.Add("Price class not available");
                        return false;
                    }

                    string name = element["Name"].InnerText;

                    if (requestedCabinTypeCodeReshop.Equals("5"))
                    {
                        if (!name.Is("Economy Classic") && !name.Is("Economy Classic Flex") && !name.Is("Economy Light") && !name.Is("Economy Delight") && !name.Is("Economy Delight Flex") && !name.Is("Premium") && !name.Is("Premium Flex") && !name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                        {
                            result.Message.Add("requested cabin type and cabin name in response are not same");

                        }
                    }
                    else if (requestedCabinTypeCodeReshop.Equals("4") && !name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                    {
                        if (!name.Is("Premium") && !name.Is("Premium Flex"))
                        {
                            result.Message.Add("requested cabin type and cabin name in response are not same");

                        }
                    }
                    else
                    {
                        if (!name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                        {
                            result.Message.Add("requested cabin type and cabin name in response are not same");

                        }
                    }

                    string priceClassID = element["PriceClassID"].InnerText;

                    if (!priceClassID.Equals("PC" + i))
                    {
                        result.Message.Add("Price class ID is not as expected");

                    }
                }
            }
            else
            {
                int i = 0;
                foreach (XmlElement element in priceClass)
                {
                    ++i;
                    string name = element["Name"].InnerText;

                    if (requestedCabinTypeCodeReshop.Equals("5") && requestedCabinTypeCodeAirShopping.Equals("4"))
                    {
                        if (!name.Is("Economy Classic") && !name.Is("Economy Classic Flex") && !name.Is("Economy Light") && !name.Is("Economy Delight") && !name.Is("Economy Delight Flex") && !name.Is("Premium") && !name.Is("Premium Flex") && !name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                        {
                            result.Message.Add("requested cabin type and cabin name in response are not same");

                        }
                    }
                    else if (requestedCabinTypeCodeReshop.Equals("4") && requestedCabinTypeCodeAirShopping.Equals("2") && !name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                    {
                        if (!name.Is("Premium") && !name.Is("Premium Flex") && !name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                        {
                            result.Message.Add("requested cabin type and cabin name in response are not same");

                        }
                    }
                    else
                    {
                        if (!name.Is("Upper Class") && !name.Is("Upper Class Flex"))
                        {
                            result.Message.Add("requested cabin type and cabin name in response are not same");

                        }
                    }

                    string priceClassID = element["PriceClassID"].InnerText;

                    if (!priceClassID.Equals("PC" + i))
                    {
                        result.Message.Add("Price class ID is not as expected");

                    }
                }

            }

            //validate requoted offer services
            int serviceIDCounter = 1;

            XmlNodeList pricedOfferServices = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem", xmlNamespaceManager);

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
                    result.Message.Add($"Incorrect paxSegmentRefID in services, actual{paxSegmentRefIDs} but expected{segmentCount}");
                }

                string serviceID = node.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;

                if (!serviceID.Is("SVNEW" + serviceIDCounter))
                {
                    result.Message.Add($"Incorrect service ID in services, actual{serviceID} but expected{"SVNEW" + serviceIDCounter}");
                }

                serviceIDCounter = serviceIDCounter + serviceCount;
            }

            //validate other offer services
            XmlNodeList otherOfferServices = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem", xmlNamespaceManager);

            foreach (XmlNode node in otherOfferServices)
            {
                int paxjourneyRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:PaxJourneyRef/ct:PaxJourneyRefID", xmlNamespaceManager).Count;

                int paxSegmentRefIDs = node.SelectNodes("ct:Service/ct:OfferServiceAssociation/ct:ServiceDefinitionRef[ct:ServiceDefinitionRefID]/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                int serviceCount = node.SelectNodes("ct:Service", xmlNamespaceManager).Count;

                if (paxjourneyRefIDs != journeyCount && paxjourneyRefIDs > 0)
                {
                    result.Message.Add($"Incorrect paxjourneyRefIDs in services, actual{paxjourneyRefIDs} but expected{journeyCount}");
                }

                if (paxSegmentRefIDs != segmentCount && paxSegmentRefIDs > 0)
                {
                    result.Message.Add($"Incorrect paxSegmentRefID in services, actual{paxSegmentRefIDs} but expected{segmentCount}");
                }

                string serviceID = node.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;

                if (!serviceID.Is("SVNEW" + serviceIDCounter))
                {
                    result.Message.Add($"Incorrect service ID in services, actual{serviceID} but exepected{"SVNEW" + serviceIDCounter}");
                }

                serviceIDCounter = serviceIDCounter + serviceCount;
            }

            return result.IsValid;
        }

        public static bool ValidateBaggageAssociations(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument quoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(quoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            quoteResponseXmlDocument.LoadXml(responseXml);

            int expectedBaggageAssociationsCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:BaggageAssociations", xmlNamespaceManager).Count;

            int actualbaggageAssociations = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:BaggageAssociations[ct:BaggageAllowanceRefID=../../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID and ct:PaxRefID=../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager).Count;

            if (!expectedBaggageAssociationsCount.Equals(actualbaggageAssociations))
            {
                result.Message.Add($"Invalid BaggageAssociations, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            int expectedJourneyPriceClassCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass", xmlNamespaceManager).Count;

            int actualJourneyPriceClassCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer/ct:JourneyOverview/ct:JourneyPriceClass[ct:PriceClassRefID=../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID and ct:PaxJourneyRefID=../../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

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
            XmlDocument quoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(quoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            quoteResponseXmlDocument.LoadXml(responseXml);

            int expectedserviceCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem", xmlNamespaceManager).Count;

            int expectedCancelRestrictionCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions", xmlNamespaceManager).Count;

            int expectedChangeRestrictionCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions", xmlNamespaceManager).Count;

            int expectedfareComponentCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).Count;

            int actualfareComponentCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent[ct:PaxSegmentRefID=../../../../../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:PriceClassRefID=../../../../../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID]", xmlNamespaceManager).Count;

            if (!expectedfareComponentCount.Equals(actualfareComponentCount))
            {
                result.Message.Add($"Invalid farecomponent, actual{actualfareComponentCount} but exepected{expectedfareComponentCount}");

            }

            int actualCancelRestrictionCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions[ct:AllowedModificationInd]", xmlNamespaceManager).Count;

            if (!actualCancelRestrictionCount.Equals(expectedCancelRestrictionCount))
            {
                result.Message.Add($"Invalid Cancel Restriction, actual{actualCancelRestrictionCount} but exepected{expectedCancelRestrictionCount}");

            }

            int actualChangeRestrictionCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions[ct:AllowedModificationInd]", xmlNamespaceManager).Count;

            if (!actualChangeRestrictionCount.Equals(expectedChangeRestrictionCount))
            {
                result.Message.Add($"Invalid Change Restriction, actual{actualChangeRestrictionCount} but exepected{expectedChangeRestrictionCount}");

            }
            return result.IsValid;
        }

        public static bool ValidateBaggageAssociationsRequotedOffer(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument quoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(quoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            quoteResponseXmlDocument.LoadXml(responseXml);

            int expectedBaggageAssociationsCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:RequotedOffer/ct:BaggageAssociations", xmlNamespaceManager).Count;

            int actualbaggageAssociations = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:RequotedOffer/ct:BaggageAssociations[ct:BaggageAllowanceRefID=../../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID and ct:OfferFlightAssociations/ct:PaxJourneyRef/ct:PaxJourneyRefID=../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID and ct:PaxRefID=../../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID]", xmlNamespaceManager).Count;

            if (!expectedBaggageAssociationsCount.Equals(actualbaggageAssociations))
            {
                result.Message.Add($"Invalid BaggageAssociations, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            int expectedJourneyPriceClassCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:RequotedOffer/ct:JourneyOverview/ct:JourneyPriceClass", xmlNamespaceManager).Count;

            int actualJourneyPriceClassCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:RequotedOffer/ct:JourneyOverview/ct:JourneyPriceClass[ct:PriceClassRefID=../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID and ct:PaxJourneyRefID=../../../../../ct:DataLists/ct:PaxJourneyList/ct:PaxJourney/ct:PaxJourneyID]", xmlNamespaceManager).Count;

            if (!expectedJourneyPriceClassCount.Equals(actualJourneyPriceClassCount))
            {
                result.Message.Add($"Invalid priceclass, actual{actualbaggageAssociations} but exepected{expectedBaggageAssociationsCount}");

            }

            return result.IsValid;
        }

        public static bool ValidateFareComponentRequotedOffer(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopRequestXmlDocument = new XmlDocument();
            XmlDocument quoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(quoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            reshopRequestXmlDocument.LoadXml(requestXml);
            quoteResponseXmlDocument.LoadXml(responseXml);

            int expectedserviceCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem", xmlNamespaceManager).Count;

            int expectedCancelRestrictionCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions", xmlNamespaceManager).Count;

            int expectedChangeRestrictionCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions", xmlNamespaceManager).Count;

            int expectedfareComponentCount = quoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent", xmlNamespaceManager).Count;

            int actualfareComponentCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent[ct:PaxSegmentRefID=../../../../../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:PriceClassRefID=../../../../../../../../../ct:DataLists/ct:PriceClassList/ct:PriceClass/ct:PriceClassID]", xmlNamespaceManager).Count;

            if (!expectedfareComponentCount.Equals(actualfareComponentCount))
            {
                result.Message.Add($"Invalid farecomponent, actual{actualfareComponentCount} but exepected{expectedfareComponentCount}");

            }

            int actualCancelRestrictionCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:CancelRestrictions[ct:AllowedModificationInd ]", xmlNamespaceManager).Count;

            if (!actualCancelRestrictionCount.Equals(expectedCancelRestrictionCount))
            {
                result.Message.Add($"Invalid Cancel Restriction, actual{actualCancelRestrictionCount} but exepected{expectedCancelRestrictionCount}");

            }

            int actualChangeRestrictionCount = quoteResponseXmlDocument.SelectNodes($"msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:ChangeRestrictions[ct:AllowedModificationInd ]", xmlNamespaceManager).Count;

            if (!actualChangeRestrictionCount.Equals(expectedChangeRestrictionCount))
            {
                result.Message.Add($"Invalid Change Restriction, actual{actualChangeRestrictionCount} but exepected{expectedChangeRestrictionCount}");

            }
            return result.IsValid;
        }

        //BaggaeAllowancelist
        public static bool ValidateBaggaeAllowancelist(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderQuoteRequestXmlDocument = new XmlDocument();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();

            orderQuoteRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderQuoteRequestXmlDocument.LoadXml(requestXml);
            orderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList baggageAllowanceCount = orderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance", xmlNamespaceManager);

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

        //ServiceDefinitionList
        public static bool ValidateServiceDefinitionList(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(orderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderQuoteResponseXmlDocument.LoadXml(responseXml);

            int serviceDefinitionCout = orderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).Count;

            IEnumerable<XmlNode> serviceDefinitions = orderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:ServiceDefinitionAssociation/ct:BaggageAllowanceRef/ct:BaggageAllowanceRefID=../../../ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance/ct:BaggageAllowanceID]", xmlNamespaceManager)
                                                            .OfType<XmlNode>();

            XmlNodeList serviceDefinitionAmenities = orderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:Desc]", xmlNamespaceManager);

            XmlNodeList baggageAllowance = orderQuoteResponseXmlDocument.GetElementsByTagName("BaggageAllowanceID");

            int serviceIDFlag = 0;

            foreach (XmlNode serviceDefinition in serviceDefinitions)
            {
                serviceIDFlag++;

                string serviceID = serviceDefinition.SelectSingleNode("ct:ServiceDefinitionID", xmlNamespaceManager).InnerText;
                if (!serviceID.Equals("SRV" + serviceIDFlag))
                {
                    result.Message.Add($"serviceDefinitionId is Invalid Expected SRV+ {serviceIDFlag} but actual is {serviceID} ");

                }
            }

            return result.IsValid;
        }

        //ReshopOffers FareComponent
        public static bool ValidateFareDetailOtherOffers(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            XmlDocument OrderQuoteResponseXmlDocument = new XmlDocument();

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);
            OrderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add($"priceClassList is not found in response");

            }

            int priceClassIDResponse = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            int paxJourneysCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest", xmlNamespaceManager).Count;

            int segmentCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int paxCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNodeList otherOffers = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer", xmlNamespaceManager);

            foreach (XmlNode offer in otherOffers)
            {
                XmlNodeList fareComponent = offer.SelectNodes("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);
                XmlNodeList addedOfferItems = offer.SelectNodes("ct:AddedOfferItem", xmlNamespaceManager);

                if (fareComponent.Count != (paxJourneysCount * addedOfferItems.Count))
                {
                    result.Message.Add("farecomponents are not as expected");

                }

                if (paxCount > 1)
                {

                    foreach (XmlNode addedOffer in addedOfferItems)
                    {
                        string priceClassRefId = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PriceClassRefID", xmlNamespaceManager).InnerText;

                        if (string.IsNullOrEmpty(priceClassRefId))
                        {
                            result.Message.Add($"priceClassRefID is incorrect in fare component");

                        }
                        string fareBasisCode = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareBasisCode", xmlNamespaceManager).InnerText;

                        if (string.IsNullOrEmpty(fareBasisCode))
                        {
                            result.Message.Add($"FareBasisCode is incorrect in fare component");

                        }
                        string paxSegRefId = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PaxSegmentRefID", xmlNamespaceManager).InnerText;

                        if (string.IsNullOrEmpty(paxSegRefId))
                        {
                            result.Message.Add($"PaxsegRefId is incorrect in fare component");

                        }
                    }
                }
                else
                {

                    string priceClassRefId1Pax = offer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PriceClassRefID", xmlNamespaceManager).InnerText;
                    if (string.IsNullOrEmpty(priceClassRefId1Pax))
                    {
                        result.Message.Add($"priceClassRefID is incorrect in fare component");

                    }

                    string fareBasisCode1Pax = offer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareBasisCode", xmlNamespaceManager).InnerText;

                    if (string.IsNullOrEmpty(fareBasisCode1Pax))
                    {
                        result.Message.Add($"FareBasisCode is incorrect in fare component");

                    }
                    string paxSegRefId1Pax = offer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PaxSegmentRefID", xmlNamespaceManager).InnerText;

                    if (string.IsNullOrEmpty(paxSegRefId1Pax))
                    {
                        result.Message.Add($"PaxsegRefId is incorrect in fare component");

                    }
                }

            }

            return result.IsValid;
        }
        //RequotedOffer 
        public static bool ValidateFareDetailRequotedOffer(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            XmlDocument OrderQuoteResponseXmlDocument = new XmlDocument();

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);
            OrderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add($"priceClassList is not found in response");

            }

            int priceClassIDResponse = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            int paxJourneysCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:OriginDestList/ct:OriginDest", xmlNamespaceManager).Count;

            int segmentCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int paxCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNode reqotedOffer = OrderQuoteResponseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer", xmlNamespaceManager);

            if (paxCount > 1)
            {
                XmlNodeList fareComponent = reqotedOffer.SelectNodes("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent", xmlNamespaceManager);
                XmlNodeList addedOfferItems = reqotedOffer.SelectNodes("ct:AddedOfferItem", xmlNamespaceManager);

                if (fareComponent.Count != (paxJourneysCount * addedOfferItems.Count))
                {
                    result.Message.Add("farecomponents are not as expected");

                }
                foreach (XmlNode addedOffer in addedOfferItems)
                {
                    string priceClassRefId = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PriceClassRefID", xmlNamespaceManager).InnerText;

                    if (string.IsNullOrEmpty(priceClassRefId))
                    {
                        result.Message.Add($"priceClassRefID is incorrect in fare component");

                    }
                    string fareBasisCode = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareBasisCode", xmlNamespaceManager).InnerText;

                    if (string.IsNullOrEmpty(fareBasisCode))
                    {
                        result.Message.Add($"FareBasisCode is incorrect in fare component");

                    }
                    string paxSegRefId = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PaxSegmentRefID", xmlNamespaceManager).InnerText;

                    if (string.IsNullOrEmpty(paxSegRefId))
                    {
                        result.Message.Add($"PaxsegRefId is incorrect in fare component");

                    }
                }
            }
            else
            {
                string priceClassRefId1Pax = reqotedOffer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PriceClassRefID", xmlNamespaceManager).InnerText;
                if (string.IsNullOrEmpty(priceClassRefId1Pax))
                {
                    result.Message.Add($"priceClassRefID is incorrect in fare component");

                }

                string fareBasisCode1Pax = reqotedOffer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareBasisCode", xmlNamespaceManager).InnerText;

                if (string.IsNullOrEmpty(fareBasisCode1Pax))
                {
                    result.Message.Add($"FareBasisCode is incorrect in fare component");

                }
                string paxSegRefId1Pax = reqotedOffer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:PaxSegmentRefID", xmlNamespaceManager).InnerText;

                if (string.IsNullOrEmpty(paxSegRefId1Pax))
                {
                    result.Message.Add($"PaxsegRefId is incorrect in fare component");

                }
            }

            return result.IsValid;
        }
        //ReshopOffers FareTypeCode
        public static bool ValidateFareTypeCodeOtherOffers(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            XmlDocument OrderQuoteResponseXmlDocument = new XmlDocument();

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);
            OrderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add($"priceClassList is not found in response");

            }

            int priceClassIDResponse = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            int segmentCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int paxCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNodeList otherOffers = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer", xmlNamespaceManager);

            foreach (XmlNode offer in otherOffers)
            {
                if (paxCount > 1)
                {
                    XmlNodeList addedOfferItems = offer.SelectNodes("ct:AddedOfferItem", xmlNamespaceManager);
                    foreach (XmlNode addedOffer in addedOfferItems)
                    {
                        string fareTypeCode = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareTypeCode", xmlNamespaceManager).InnerText;

                        if (fareTypeCode != "70J")
                        {
                            result.Message.Add($"fareType Code is incorrect in fare component");

                        }

                    }
                }
                else
                {
                    string fareTypeCode1Pax = offer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareTypeCode", xmlNamespaceManager).InnerText;
                    if (fareTypeCode1Pax != "70J")
                    {
                        result.Message.Add($"fareType Code is incorrect in fare component");

                    }
                }

            }

            return result.IsValid;
        }

        //RequoteOffer FareTypeCode
        public static bool ValidateFareTypeCodeRequottedOffer(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderQuoteRequestXmlDocument = new XmlDocument();
            XmlDocument OrderQuoteResponseXmlDocument = new XmlDocument();

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderQuoteRequestXmlDocument.LoadXml(requestXml);
            OrderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList priceClassList = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList", xmlNamespaceManager);

            if (priceClassList.Count == 0)
            {
                result.Message.Add($"priceClassList is not found in response");

            }

            int priceClassIDResponse = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PriceClassList/ct:PriceClass", xmlNamespaceManager).Count;

            int segmentCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int paxCount = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNode reqotedOffer = OrderQuoteResponseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer", xmlNamespaceManager);

            if (paxCount > 1)
            {
                XmlNodeList addedOfferItems = reqotedOffer.SelectNodes("ct:AddedOfferItem", xmlNamespaceManager);
                foreach (XmlNode addedOffer in addedOfferItems)
                {
                    string fareTypeCode = addedOffer.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareTypeCode", xmlNamespaceManager).InnerText;

                    if (fareTypeCode != "70J")
                    {
                        result.Message.Add($"fareType Code is incorrect in fare component");

                    }

                }
            }
            else
            {
                string fareTypeCode1Pax = reqotedOffer.SelectSingleNode("ct:AddedOfferItem/ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:FareDetail/ct:FareComponent/ct:FareTypeCode", xmlNamespaceManager).InnerText;
                if (fareTypeCode1Pax != "70J")
                {
                    result.Message.Add($"fareType Code is incorrect in fare component");

                }
            }

            return result.IsValid;
        }

        public static bool ValidateAirCarftTypeCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderQuoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(OrderQuoteResponseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            OrderQuoteResponseXmlDocument.LoadXml(responseXml);

            XmlNodeList datedOperatingSegments = OrderQuoteResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

            foreach (XmlNode datedOperatingSegment in datedOperatingSegments)
            {
                XmlNode AirCraftTypecodeNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftTypeCode", xmlNamespaceManager);

                if (AirCraftTypecodeNode is null)
                {
                    result.Message.Add($"AircrafttypeCode node is not found in one of the DatedOperatingSegments");

                }

                XmlNode AircraftTypeNameNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftTypeName", xmlNamespaceManager);

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

            int datedMarketingSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).Count;

            int datedLegSegmentCount = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName]", xmlNamespaceManager).Count;

                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");

                }
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

            XmlNodeList datedMarketingSegments = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager);
            XmlNodeList datedOperatingLegs = responseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", xmlNamespaceManager);

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

        //Validating TaxSummary Country Code
        public static bool ValidateCountryCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument quoteResponseXmlDocument = new XmlDocument();
            quoteResponseXmlDocument.LoadXml(responseXml);
            XmlNamespaceManager xmlNamespaceManager = quoteResponseXmlDocument.CreateXmlNamespaceManager();

            // Select all TaxSummary segments
            XmlNodeList searchOutYQ = quoteResponseXmlDocument.SelectNodes("//ct:TaxSummary/ct:Tax[(ct:TaxCode = 'YQ')]", xmlNamespaceManager);
            XmlNodeList searchTaxcode = quoteResponseXmlDocument.SelectNodes("//ct:TaxSummary/ct:Tax", xmlNamespaceManager);
            XmlNodeList countryCode = quoteResponseXmlDocument.SelectNodes("//ct:TaxSummary/ct:Tax/ct:Country/ct:CountryCode", xmlNamespaceManager);

            if (searchOutYQ.Count + countryCode.Count != searchTaxcode.Count)
            {
                result.Message.Add("CountryCode node is not found in one of the Tax segments");
            }
            return result.IsValid;
        }

        public static bool ValidateOrderQuoteHoldBookingResult(string responseXml, string ordercreateRS, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            orderCreateResponseXmlDocument.LoadXml(ordercreateRS);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            string expectedOrderID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager)?.InnerText;

            string actualOrderID = responseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager)?.InnerText;

            if (!expectedOrderID.Is(actualOrderID))
            {
                result.Message.Add($"Expected OrderID {expectedOrderID} but actual is {actualOrderID}");
            }

            string noChangeInd = responseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:NoChangeInd/ct:NoChangeInd", xmlNamespaceManager)?.InnerText;

            if (!noChangeInd.Is("true"))
            {
                result.Message.Add($"noChangeInd value is not true");
            }

            return result.IsValid;
        }

        public static bool ValidateRequotePricingDetails(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument reshopResponseXmlDocument = new XmlDocument();

            reshopResponseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = reshopResponseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList requotedOffer = reshopResponseXmlDocument.SelectNodes("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:RequotedOffer/ct:AddedOfferItem", xmlNamespaceManager);

            foreach (XmlNode offerItem in requotedOffer)
            {
                decimal diffPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal newPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal oldPriceTotalAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                string differentialTypeCode = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DifferentialTypeCode", xmlNamespaceManager).InnerText;

                decimal dueByAirlineAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueByAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                decimal dueToAirlineAmount = offerItem.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueToAirlineAmount", xmlNamespaceManager).InnerText.Parse<decimal>();

                if (differentialTypeCode.Is("EvenExchange") && (dueByAirlineAmount != 0 || dueToAirlineAmount != 0))
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

                if (((newPriceTotalAmount - oldPriceTotalAmount) == 0 && !differentialTypeCode.Is("EvenExchange")) || ((newPriceTotalAmount - oldPriceTotalAmount) > 0 && !differentialTypeCode.Is("AddCol")) || ((newPriceTotalAmount - oldPriceTotalAmount) < 0 && differentialTypeCode.Is("AddCol")))
                {
                    result.Message.Add($"Incorrect Price  Differential Type {differentialTypeCode} {offerItem.OuterXml}");
                }
            }

            return result.IsValid;
        }

    }

}
