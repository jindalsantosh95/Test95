using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using static VAA.EAPI.Automation.NDC.Requests.RequestHelper;

namespace VAA.EAPI.Automation.NDC.Helpers
{
    public static class FindAndReturnDataHelper
    {

        public static Random random = new();

        public static Dictionary<string, string> FindOffer(string xml, CabinType cabinType, FlightScenarioType flightScenario, FlightType operatorName = FlightType.VS, string requiredFlightNumber = "") => FindOffers(xml, cabinType, flightScenario, false, operatorName, requiredFlightNumber).FirstOrDefault() ?? new Dictionary<string, string>();

        public static IEnumerable<Dictionary<string, string>> FindOffers(string xml, CabinType cabinType, FlightScenarioType flightScenario, bool firstOnly = false, FlightType operatorName = FlightType.VS, string requiredFlightNumber = "")
        {
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(xml);

            XmlNamespaceManager xmlnsManager = xmlDocument.CreateXmlNamespaceManager();

            string datedMarketingSegmentQueryVirgin = $"ct:DatedMarketingSegmentRefId = ../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[string-length(ct:MarketingCarrierFlightNumberText) != 4]/ct:DatedMarketingSegmentId";
            string datedMarketingSegmentQueryNonVirgin = $"ct:DatedMarketingSegmentRefId = ../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[string-length(ct:MarketingCarrierFlightNumberText) = 4]/ct:DatedMarketingSegmentId";
            string datedoperatingSegmentQuery = $"ct:DatedOperatingSegmentRefId = ../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment[ct:CarrierDesigCode='{operatorName}']/ct:DatedOperatingSegmentId";
            string otherdatedoperatingSegmentQuery = $"ct:DatedOperatingSegmentRefId = ../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment[ct:CarrierDesigCode!='{operatorName}']/ct:DatedOperatingSegmentId";


            string datedMarketingSegmentOperaterQuery = $"ct:DatedMarketingSegmentRefId = ../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[{datedoperatingSegmentQuery}]/ct:DatedMarketingSegmentId";
            string datedMarketingSegmentOtherOperaterQuery = $"ct:DatedMarketingSegmentRefId = ../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[{otherdatedoperatingSegmentQuery}]/ct:DatedMarketingSegmentId";

            string paxSegmentQueryVirgin = $"ct:FareComponent/ct:PaxSegmentRefID=../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{datedMarketingSegmentQueryVirgin}]/ct:PaxSegmentID";
            string paxSegmentQueryNonVirgin = $"ct:FareComponent/ct:PaxSegmentRefID=../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{datedMarketingSegmentQueryNonVirgin}]/ct:PaxSegmentID";
            string operatorPaxSegmentQuery = $"ct:FareComponent/ct:PaxSegmentRefID=../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{datedMarketingSegmentOperaterQuery}]/ct:PaxSegmentID";
            string otheroperatorPaxSegmentQuery = $"ct:FareComponent/ct:PaxSegmentRefID=../../../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[{datedMarketingSegmentOtherOperaterQuery}]/ct:PaxSegmentID";

            string paxSegmentQueryVirginOnly = $"{paxSegmentQueryVirgin} and not(boolean({paxSegmentQueryNonVirgin}))";
            string paxSegmentQueryNonVirginOnly = $"{paxSegmentQueryNonVirgin} and not(boolean({paxSegmentQueryVirgin}))";
            string paxSegmentQueryVirginAndNonVirginOnly = $"{paxSegmentQueryVirgin} and {paxSegmentQueryNonVirgin}";
            string paxSegmentQueryVirginAndOperatoronly = $"{paxSegmentQueryVirgin} and {operatorPaxSegmentQuery}";
            string paxSegmentQueryOperatorOnly = $"{operatorPaxSegmentQuery} and  not(boolean({otheroperatorPaxSegmentQuery})) and not(boolean({paxSegmentQueryVirginOnly}))";
            string paxSegmentQueryOtheroperators = $"{operatorPaxSegmentQuery} and  not(boolean({paxSegmentQueryVirgin}))";

            string paxSegmentQuery = string.Empty;

            switch (flightScenario)
            {
                case FlightScenarioType.NonVirgin:

                    paxSegmentQuery = paxSegmentQueryNonVirginOnly;

                    break;
                case FlightScenarioType.Virgin:

                    paxSegmentQuery = paxSegmentQueryVirginOnly;

                    break;
                case FlightScenarioType.VirginAndNonVirgin:

                    paxSegmentQuery = paxSegmentQueryVirginAndNonVirginOnly;
                    break;

                case FlightScenarioType.VirginAndNonVirginRequestedOperator:
                    paxSegmentQuery = paxSegmentQueryVirginAndOperatoronly;
                    break;

                case FlightScenarioType.NonVirginRequestedOperator:
                    paxSegmentQuery = paxSegmentQueryOperatorOnly;
                    break;

                case FlightScenarioType.NonVirginRequestedWithOtherOperators:
                    paxSegmentQuery = paxSegmentQueryOtheroperators;
                    break;
            }

            string offersQuery = $"msg:IATA_AirShoppingRS/msg:Response/ct:OffersGroup/ct:CarrierOffers/ct:Offer[ct:JourneyOverview/ct:JourneyPriceClass[1]/ct:PriceClassRefID = ../../../ct:DataLists/ct:PriceClassList/ct:PriceClass[ct:Name = '{cabinType.ToString().SeparateUpperCamelCase()}']/ct:PriceClassID]/ct:OfferItem[1]/ct:FareDetail[{paxSegmentQuery}]/../..";

            // Select all matching offers
            var offerNodes = xmlDocument.SelectNodes(offersQuery, xmlnsManager);

            if (offerNodes == null || offerNodes.Count == 0)
                return Array.Empty<Dictionary<string, string>>();

            var matchedNodes = offerNodes
                .OfType<XmlNode>()
                .Where(offer => requiredFlightNumber.IsNullOrWhiteSpace() ||
                                OfferContainsFlightNumber(offer, xmlDocument, xmlnsManager, requiredFlightNumber, operatorName))
                .ToList();

            if (matchedNodes.Count == 0)
                return Array.Empty<Dictionary<string, string>>();

            List<XmlNode> nodesToReturn;
            if (firstOnly)
            {
                nodesToReturn = matchedNodes.Take(1).ToList();
            }
            else
            {
                int chosenIndex = random.Next(0, matchedNodes.Count); // safe: includes last element and works when Count==1
                nodesToReturn = new List<XmlNode> { matchedNodes[chosenIndex] };
            }

            string shoppingResponseRefID = xmlDocument.SelectSingleNode($"msg:IATA_AirShoppingRS/msg:Response/ct:ShoppingResponse/ct:ShoppingResponseID", xmlnsManager)?.InnerText ?? string.Empty;

            return nodesToReturn.Select(n => new Dictionary<string, string>()
            {
                { "OfferRefID", n.SelectSingleNode($"ct:OfferID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC1", n.SelectSingleNode($"ct:OfferItem[1]/ct:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC2", n.SelectSingleNode($"ct:OfferItem[2]/ct:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC3", n.SelectSingleNode($"ct:OfferItem[3]/ct:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC4", n.SelectSingleNode($"ct:OfferItem[4]/ct:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "TotalAmount", ReturnOfferTotalAmount(n) ?? string.Empty },
                { "CurrencyCode", n.SelectSingleNode($"ct:OfferItem[1]/ct:Price/ct:TotalAmount/@CurCode", xmlnsManager)?.InnerText ?? string.Empty },
                { "ShoppingResponseRefID", shoppingResponseRefID }
            });
        }

        private static bool OfferContainsFlightNumber(XmlNode offer, XmlDocument document, XmlNamespaceManager namespaceManager, string requiredFlightNumber, FlightType operatorName)
        {
            string numericFlightNumber = new string(requiredFlightNumber.Where(char.IsDigit).ToArray());

            if (numericFlightNumber.IsNullOrWhiteSpace())
                return false;

            IEnumerable<XmlNode> paxSegmentReferences = offer.SelectNodes(".//ct:PaxSegmentRefID", namespaceManager)?.OfType<XmlNode>() ?? Enumerable.Empty<XmlNode>();

            return paxSegmentReferences
                .Select(reference => reference.InnerText)
                .SelectMany(paxSegmentId => document.SelectNodes(
                    $"//ct:PaxSegment[ct:PaxSegmentID='{paxSegmentId}']/ct:DatedMarketingSegmentRefId",
                    namespaceManager)?.OfType<XmlNode>() ?? Enumerable.Empty<XmlNode>())
                .Select(reference => document.SelectSingleNode(
                    $"//ct:DatedMarketingSegment[ct:DatedMarketingSegmentId='{reference.InnerText}']",
                    namespaceManager))
                .Where(segment => segment != null)
                .Any(segment => IsMatchingDatedMarketingSegment(segment, document, namespaceManager, numericFlightNumber, operatorName));
        }

        private static bool IsMatchingDatedMarketingSegment(XmlNode segment, XmlDocument document, XmlNamespaceManager namespaceManager, string numericFlightNumber, FlightType operatorName)
        {
            string flightNumber = segment.SelectSingleNode("ct:MarketingCarrierFlightNumberText", namespaceManager)?.InnerText ?? string.Empty;
            string normalizedFlightNumber = new string(flightNumber.Where(char.IsDigit).ToArray());

            if (!normalizedFlightNumber.Is(numericFlightNumber))
                return false;

            string marketingCarrier = segment.SelectSingleNode("ct:CarrierDesigCode", namespaceManager)?.InnerText;

            if (marketingCarrier.Is(operatorName.ToString()))
                return true;

            string operatingSegmentReference = segment.SelectSingleNode("ct:DatedOperatingSegmentRefId", namespaceManager)?.InnerText;
            string operatingCarrier = document.SelectSingleNode(
                $"//ct:DatedOperatingSegment[ct:DatedOperatingSegmentId='{operatingSegmentReference}']/ct:CarrierDesigCode",
                namespaceManager)?.InnerText;

            return operatingCarrier.Is(operatorName.ToString());
        }

        public static IEnumerable<dynamic> FindSeats(string xml, SeatPriceType priceType)
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);

            xmlnsManager.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            xmlnsManager.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
            xmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            xmlDocument.LoadXml(xml);

            string offerId = xmlDocument.SelectSingleNode("rs:IATA_SeatAvailabilityRS/rs:Response/ct:ALaCarteOffer/ct:OfferID", xmlnsManager).InnerText;

            var unitPrices = xmlDocument.SelectNodes("rs:IATA_SeatAvailabilityRS/rs:Response/ct:ALaCarteOffer/ct:OfferItem", xmlnsManager)
                                        .OfType<XmlNode>()
                                        .ToDictionary(k => k.SelectSingleNode("ct:OfferItemID", xmlnsManager).InnerText, v => v.SelectSingleNode("ct:UnitPrice/ct:TotalAmount", xmlnsManager).InnerText.Parse<double>());

            var passangers = xmlDocument.SelectNodes("rs:IATA_SeatAvailabilityRS/rs:Response/ct:DataLists/ct:PaxList/ct:Pax[ct:PTC/text() != 'INF']", xmlnsManager)
                                        .OfType<XmlNode>()
                                        .ToArray();

            return xmlDocument.SelectNodes("rs:IATA_SeatAvailabilityRS/rs:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID/text()", xmlnsManager)
                                   .OfType<XmlNode>()
                                   .SelectMany(x =>
                                   {
                                       string freeOnlyFilter = $"[ct:UnitPrice/ct:TotalAmount {(priceType == SeatPriceType.Free ? "=" : "!=")} '0.00']";
                                       string avaliableSeats = $"rs:IATA_SeatAvailabilityRS/rs:Response/ct:SeatMap[ct:PaxSegmentRefID='{x.InnerText}']/ct:CabinCompartment/ct:SeatRow/ct:Seat[ct:OfferItemRefID = ../../../../ct:ALaCarteOffer/ct:OfferItem{freeOnlyFilter}/ct:OfferItemID]";

                                       var seats = x.OwnerDocument.SelectNodes(avaliableSeats, xmlnsManager)
                                                                   .OfType<XmlNode>()
                                                                   .ToArray();

                                       int numberOfSeatsNeeded = passangers.Length;
                                       double amount = 0d;

                                       if (seats.Any())
                                           amount = unitPrices[seats[0].SelectSingleNode("ct:OfferItemRefID", xmlnsManager).InnerText];

                                       return seats.Take(numberOfSeatsNeeded)
                                                   .Zip(passangers)
                                                   .Select(t => new
                                                   {
                                                       OfferID = offerId,
                                                       Segment = x.InnerText,
                                                       OfferItemRefID = t.First.SelectSingleNode("ct:OfferItemRefID", xmlnsManager).InnerText,
                                                       UnitAmount = unitPrices[t.First.SelectSingleNode("ct:OfferItemRefID", xmlnsManager).InnerText],
                                                       ColumnID = t.First.SelectSingleNode("ct:ColumnID", xmlnsManager).InnerText,
                                                       RowNumber = t.First.SelectSingleNode("ct:RowNumber", xmlnsManager).InnerText,
                                                       PaxRefID = t.Second.SelectSingleNode("ct:PaxID", xmlnsManager).InnerText,
                                                   });
                                   })
                                   .ToArray();
        }

        public static Dictionary<string, string> FindOfferFromReshop(string xml, string exchangeType = "")
        {
            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);

            xmlnsManager.AddNamespace("rs", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            xmlDocument.LoadXml(xml);

            var reshopOffer = xmlDocument.SelectNodes("//rs:ReshopResults//rs:ReshopOffers//rs:Offer", xmlnsManager).OfType<XmlNode>().ToArray();

            XmlNode offerNode = null;

            foreach (XmlElement element in reshopOffer)
            {
                decimal totalAmountNode = ReturnReshopOfferrTotalAmount(element).Parse<decimal>();

                string exchangeNode = element.SelectSingleNode("rs:AddedOfferItem/rs:ReshopPrice/rs:PriceDifferential/rs:DifferentialTypeCode", xmlnsManager).InnerText;

                if (exchangeNode != null)
                {
                    if (exchangeType.Is(exchangeNode) && ((totalAmountNode == 0 && exchangeNode.Is("EvenExchange")) || (totalAmountNode > 0 && exchangeNode.Is("AddCol")) || (totalAmountNode < 0 && exchangeNode.Is("Refund"))))
                    {
                        offerNode = element;
                        break;
                    }
                }
            }

            return offerNode == null ? new Dictionary<string, string>() : new Dictionary<string, string>()
            {
                { "OfferRefID", offerNode.SelectSingleNode($"rs:OfferID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC1", offerNode.SelectSingleNode($"rs:AddedOfferItem[1]/rs:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC2", offerNode.SelectSingleNode($"rs:AddedOfferItem[2]/rs:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC3", offerNode.SelectSingleNode($"rs:AddedOfferItem[3]/rs:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "OfferItemRefID_PTC4", offerNode.SelectSingleNode($"rs:AddedOfferItem[4]/rs:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty },
                { "TotalAmount", ReturnReshopOfferrTotalAmount(offerNode) ?? string.Empty },
                { "CurrencyCode", offerNode.SelectSingleNode($"rs:TotalPrice/rs:TotalAmount/@CurCode", xmlnsManager)?.InnerText ?? string.Empty }
            };
        }

        public static Dictionary<string, int> ReturnPaxCount(XmlDocument responseDoc)
        {
            var result = new Dictionary<string, int>();

            var xmlnsManager = responseDoc.CreateXmlNamespaceManager();

            int adultCount = responseDoc.SelectNodes("//ct:PaxList//ct:Pax[ct:PTC='ADT']", xmlnsManager).Count;

            result.Add("Adult", adultCount);

            int youngAdultCount = responseDoc.SelectNodes("//ct:PaxList//ct:Pax[ct:PTC='GBE']", xmlnsManager).Count;

            result.Add("YoungAdult", youngAdultCount);

            int childCount = responseDoc.SelectNodes("//ct:PaxList//ct:Pax[ct:PTC='CHD']", xmlnsManager).Count;

            result.Add("Child", childCount);

            int infantCount = responseDoc.SelectNodes("//ct:PaxList//ct:Pax[ct:PTC='INF']", xmlnsManager).Count;

            result.Add("Infant", infantCount);

            return result;
        }

        public static string ReturnReQuotedOfferrTotalAmount(XmlNode responseXml)
        {
            var responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(responseXml.OuterXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            decimal totalOfferPrice = 0m;

            var offerItems = responseXmlDocument.SelectNodes("//ct:RequotedOffer//ct:AddedOfferItem", xmlNamespaceManager);

            foreach (XmlNode offerItemNode in offerItems)
            {
                int paxCount = offerItemNode.SelectNodes("ct:Service[1]/ct:PaxRefID", xmlNamespaceManager).Count;

                string totalamount = offerItemNode.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText;

                totalOfferPrice = (Convert.ToDecimal(totalamount) * paxCount) + totalOfferPrice;
            }

            return totalOfferPrice.ToString();
        }

        public static string ReturnBookingId(string responseXml)
        {
            var orderCreateResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            orderCreateResponseXmlDocument.LoadXml(responseXml);

            string bookingID = orderCreateResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[1]/ct:Service[1]/ct:BookingRef/ct:BookingID", xmlNamespaceManager)?.InnerText;

            return bookingID;
        }

        public static string GetYearSuffix()
        {
            string yearSuffix = DateTime.Now.ToString("yy");

            return yearSuffix;
        }

        public static string ReturnOfferTotalAmount(XmlNode responseXml)
        {
            var responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(responseXml.OuterXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            decimal totalOfferPrice = 0m;

            var offerItems = responseXmlDocument.SelectNodes("//ct:OfferItem", xmlNamespaceManager);

            foreach (XmlNode offerItemNode in offerItems)
            {
                int paxCount = offerItemNode.SelectNodes("ct:FareDetail/ct:PaxRefID", xmlNamespaceManager).Count;
                string baseAmount = offerItemNode.SelectSingleNode("ct:Price/ct:BaseAmount", xmlNamespaceManager).InnerText;
                string taxAmount = offerItemNode.SelectSingleNode("ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlNamespaceManager).InnerText;
                string totalamount = offerItemNode.SelectSingleNode("ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText;

                totalOfferPrice = (Convert.ToDecimal(totalamount) * paxCount) + totalOfferPrice;
            }

            return totalOfferPrice.ToString();
        }

        public static string ReturnReshopOfferrTotalAmount(XmlNode responseXml)
        {
            var responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(responseXml.OuterXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            decimal totalOfferPrice = 0m;

            var offerItems = responseXmlDocument.SelectNodes("//ct:AddedOfferItem", xmlNamespaceManager);

            foreach (XmlNode offerItemNode in offerItems)
            {
                int paxCount = offerItemNode.SelectNodes("ct:Service/ct:PaxRefID", xmlNamespaceManager).Count;
                string totalamount = offerItemNode.SelectSingleNode("ct:ReshopPrice/ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlNamespaceManager).InnerText;

                totalOfferPrice = (Convert.ToDecimal(totalamount) * paxCount) + totalOfferPrice;
            }

            return totalOfferPrice.ToString();
        }

        public static string ReturnNewOrderItemID(string request, string response)
        {
            XmlDocument orderRetrieveResponseXmlDocument = new XmlDocument();
            orderRetrieveResponseXmlDocument.LoadXml(response);

            XmlNamespaceManager xmlNamespaceManager = orderRetrieveResponseXmlDocument.CreateXmlNamespaceManager();

            var orderItemID = orderRetrieveResponseXmlDocument.SelectNodes("//ct:Order//ct:OrderItem[ct:FareDetail]//ct:OrderItemID", xmlNamespaceManager);

            var orderReshopRequestXmlDocument = new XmlDocument();
            orderReshopRequestXmlDocument.LoadXml(request);

            int newOrderItemCounter = 1;

            foreach (XmlNode node in orderItemID)
            {
                orderReshopRequestXmlDocument.SelectSingleNode($"//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:ServiceOrder//ct:DeleteOrderItem[{newOrderItemCounter++}]//ct:OrderItemRefID", xmlNamespaceManager).InnerText = node.InnerText;
            }

            return orderReshopRequestXmlDocument.OuterXml;
        }


        public static IEnumerable<Dictionary<string, string>> FindTTLOffersssss(string xml, string targetDate, TimeSpan fromTime, TimeSpan toTime, string requestedCarrier, bool firstOnly = false)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            XmlNamespaceManager xmlnsManager = xmlDocument.CreateXmlNamespaceManager();

            if (!DateTime.TryParse(targetDate, out var targetDateParsed))
                return Enumerable.Empty<Dictionary<string, string>>();

            string offersQuery = "//ct:CarrierOffers/ct:Offer";

            var offerNodes = xmlDocument.SelectNodes(offersQuery, xmlnsManager);

            if (offerNodes == null || offerNodes.Count == 0)
                return Enumerable.Empty<Dictionary<string, string>>();

            var matchedNodes = new List<XmlNode>();

            foreach (XmlNode offerNode in offerNodes)
            {
                // Get PaxSegmentRefID
                string paxSegmentRef = offerNode
                    .SelectSingleNode(".//ct:FareComponent[1]/ct:PaxSegmentRefID", xmlnsManager)?.InnerText;

                if (paxSegmentRef.IsNullOrWhiteSpace())
                    continue;

                // Find PaxSegment
                var paxSegmentNode = xmlDocument.SelectSingleNode($"//ct:PaxSegment[ct:PaxSegmentID='{paxSegmentRef}']", xmlnsManager);

                if (paxSegmentNode == null)
                    continue;

                // Get DatedMarketingSegmentRefId
                string marketingSegmentRef = paxSegmentNode.SelectSingleNode("ct:DatedMarketingSegmentRefId", xmlnsManager)?.InnerText;

                if (marketingSegmentRef.IsNullOrWhiteSpace())
                    continue;

                // Find DatedMarketingSegment
                var marketingSegmentNode = xmlDocument.SelectSingleNode($"//ct:DatedMarketingSegment[ct:DatedMarketingSegmentId='{marketingSegmentRef}']", xmlnsManager);

                if (marketingSegmentNode == null)
                    continue;

                // Get Departure DateTime
                string departureDateTimeText = marketingSegmentNode.SelectSingleNode("ct:Dep/ct:AircraftScheduledDateTime", xmlnsManager)?.InnerText;

                if (!DateTime.TryParse(departureDateTimeText, out var departureDateTime))
                    continue;

                // CONDITION 1
                // Departure date must match
                if (departureDateTime.Date != targetDateParsed.Date)
                    continue;

                // CONDITION 2
                // Departure time must be within range
                var departureTime = departureDateTime.TimeOfDay;

                bool isWithinTimeRange = fromTime <= toTime ? departureTime >= fromTime && departureTime <= toTime : departureTime >= fromTime || departureTime <= toTime;

                if (!isWithinTimeRange)
                    continue;

                // Get DatedOperatingSegmentRefId
                string operatingSegmentRef = marketingSegmentNode.SelectSingleNode("ct:DatedOperatingSegmentRefId", xmlnsManager)?.InnerText;

                if (operatingSegmentRef.IsNullOrWhiteSpace())
                    continue;

                // Find DatedOperatingSegment
                var operatingSegmentNode = xmlDocument.SelectSingleNode($"//ct:DatedOperatingSegment[ct:DatedOperatingSegmentId='{operatingSegmentRef}']", xmlnsManager);

                if (operatingSegmentNode == null)
                    continue;

                // Get Carrier Name
                string carrierName = operatingSegmentNode.SelectSingleNode("ct:CarrierName", xmlnsManager)?.InnerText;

                // CONDITION 3
                // Carrier name must match
                if (!carrierName.Is(requestedCarrier))
                {
                    continue;
                }

                // MATCH FOUND
                matchedNodes.Add(offerNode);
            }

            if (matchedNodes.Count == 0)
                return Enumerable.Empty<Dictionary<string, string>>();

            var nodesToReturn = firstOnly ? matchedNodes.Take(1).ToList() : matchedNodes;

            return nodesToReturn.Select(n => new Dictionary<string, string>
            {
                {
                    "OfferRefID", n.SelectSingleNode("ct:OfferID", xmlnsManager)?.InnerText ?? string.Empty
                },

                {
                    "OfferItemRefID_PTC1", n.SelectSingleNode("ct:OfferItem[1]/ct:OfferItemID", xmlnsManager)?.InnerText ?? string.Empty
                },

                {
                    "TotalAmount", n.SelectSingleNode("ct:OfferItem[1]/ct:Price/ct:TotalAmount", xmlnsManager)?.InnerText ?? string.Empty
                },

                {
                    "CurrencyCode", n.SelectSingleNode("ct:OfferItem[1]/ct:Price/ct:TotalAmount/@CurCode", xmlnsManager)?.InnerText ?? string.Empty

                },
            });
        }
    }
}

