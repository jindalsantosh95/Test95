using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.NDC.Helpers;

namespace VAA.EAPI.Automation.NDC.Tests.OrderChange
{
    public class OrderChangeValidators
    {
        public static bool ValidateOrderChangeSeatTicketDocument(string requestXml, string responseXml, out ValidationResultHelper result, string changeRequest = "")
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
            XmlDocument orderChangeRequestXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager xmlNamespaceManager = orderCreateRequestXmlDocument.CreateXmlNamespaceManager();

            int expectedSeatTicketCreateDocCount = 0;

            if (!string.IsNullOrEmpty(changeRequest))
            {
                orderChangeRequestXmlDocument.LoadXml(changeRequest);
                expectedSeatTicketCreateDocCount = orderChangeRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[2]/ct:SelectedOfferItem", xmlNamespaceManager).Count;
            }

            orderCreateResponseXmlDocument.LoadXml(responseXml);

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            int expectedSeatTicketDocCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:ChangeOrderChoice/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            int expectedTicketDocCount = orderCreateResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager).Count;

            XmlNodeList seattickets = orderCreateResponseXmlDocument.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:IssueInfo/ct:IssueDate='{todaysDate}' and ct:IssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode='VS' and ct:PaymentRefID=../../msg:PaymentFunctions/ct:PaymentProcessingSummary/ct:PaymentID  and ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and  ct:Ticket/ct:Coupon/ct:CouponNumber and ct:Ticket/ct:Coupon/ct:CouponStatusCode and ct:Ticket/ct:ReportingTypeCode='Airline' and ct:Ticket/ct:TicketDocTypeCode='J' or ct:Ticket/ct:TicketDocTypeCode='T'  ]", xmlNamespaceManager);

            if (expectedSeatTicketDocCount + expectedTicketDocCount + expectedSeatTicketCreateDocCount != seattickets.Count)
            {
                result.Message.Add($"Incorrect seat ticket doc returned");
            }

            return result.IsValid;
        }

        //PriceDifferential
        public static bool ValidateNameChangeOrderItemAndServiceDefinition(string reshopRS, string orderChangeRS, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderReshopDoc = new XmlDocument();
            XmlDocument orderChangeDoc = new XmlDocument();

            orderReshopDoc.LoadXml(reshopRS);
            orderChangeDoc.LoadXml(orderChangeRS);

            XmlNamespaceManager xmlNamespaceManager = orderReshopDoc.CreateXmlNamespaceManager();

            string orderID = orderChangeDoc.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            string updatedGivenName = orderReshopDoc.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferItem/ct:PaxNameChange/ct:GivenName", xmlNamespaceManager)?.InnerText;

            string updatedSurName = orderReshopDoc.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferItem/ct:PaxNameChange/ct:Surname", xmlNamespaceManager)?.InnerText;

            string paxRefID = orderReshopDoc.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferItem/ct:PaxNameChange/ct:PaxRefID", xmlNamespaceManager)?.InnerText;

            //validate service definition
            int serviceDefCount = orderChangeDoc.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ct:Name='Name Change' and ct:OwnerCode='VS' and ct:ServiceDefinitionID='SRVDEF1_NAMCHG']", xmlNamespaceManager).Count;

            XmlNodeList actualService = orderReshopDoc.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition/ct:ServiceCode", xmlNamespaceManager);

            if (serviceDefCount != 1)
            {
                result.Message.Add($"Incorrect service definition details returned in the response {orderChangeDoc.SelectSingleNode($"msg:IATA_OrderReshopRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).OuterXml}");
            }

            //Validate updated pax name
            int nameChangeCount = orderChangeDoc.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[ct:Individual/ct:GivenName='{updatedGivenName}' and ct:Individual/ct:Surname='{updatedSurName}' and ct:PaxID='{paxRefID}']", xmlNamespaceManager).Count;

            if (nameChangeCount != 1)
            {
                result.Message.Add($"updated pax name not returned in the order change response Expected given name --> {updatedGivenName}: Expected surname -->{updatedSurName} but actual is {orderChangeDoc.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax[ct:PaxID='{paxRefID}']", xmlNamespaceManager).OuterXml}");
            }

            string paxOrderItemID = orderChangeDoc.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:PaxRefID='{paxRefID}']/ct:OrderItemID", xmlNamespaceManager).InnerText;

            //Validate name change order item
            int nameChangeOrderItem = orderChangeDoc.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:OrderItemID='{paxOrderItemID}NEW' and ct:OwnerCode='VS' and ct:Price/ct:TotalAmount[@CurCode]='0.00' and ct:StatusCode='CLOSED']", xmlNamespaceManager).Count;

            if (nameChangeOrderItem != 1)
            {
                result.Message.Add($"Incorrect order item values returned {orderChangeDoc.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:OrderItemID = '{orderID}-1NEW']", xmlNamespaceManager).OuterXml}");
            }

            //Validate order item service definition
            int orderItemService = orderChangeDoc.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:Service/ct:DeliveryStatusCode='DELIVERED' and ct:Service/ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:ServiceDefinitionRefID='SRVDEF1_NAMCHG' and ct:Service/ct:PaxRefID='{paxRefID}' and ct:Service/ct:ServiceID='SV1' and ct:Service/ct:StatusCode='CONFIRMED']", xmlNamespaceManager).Count;

            if (orderItemService != 1)
            {
                result.Message.Add($"Incorrect order item service values returned {orderChangeDoc.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:OrderItemID = '{orderID}-1NEW']/ct:Service", xmlNamespaceManager).OuterXml}");
            }

            return result.IsValid;
        }

        //PriceDifferential
        public static bool ValidatePriceDifferential(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateorderchangeXmlnsManager = new XmlDocument();

            XmlNamespaceManager orderchangeXmlnsManager = orderCreateRequestXmlDocument.CreateXmlNamespaceManager();
            XmlNamespaceManager orderchangeXmlnsManager1 = orderCreateorderchangeXmlnsManager.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateorderchangeXmlnsManager.LoadXml(responseXml);

            int orderItemCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem", orderchangeXmlnsManager).Count;

            string differenceAmount = "0.00";

            for (int i = 1; i <= orderItemCount; i++)
            {
                string actualBaseAmount = orderCreateorderchangeXmlnsManager.SelectSingleNode($"msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[{i}]/ct:PriceDifferential/ct:TotalAmount/ct:DiffPrice/ct:FareDetail/ct:Price/ct:BaseAmount", orderchangeXmlnsManager1).InnerText;

                if (!actualBaseAmount.Equals(differenceAmount))
                {
                    result.Message.Add($"Invalid Base Amount, was expecting {differenceAmount} but found {actualBaseAmount} instead.");
                }

                string actualTotalAmount = orderCreateorderchangeXmlnsManager.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[{i}]/ct:PriceDifferential/ct:TotalAmount/ct:DiffPrice/ct:FareDetail/ct:Price/ct:TotalAmount", orderchangeXmlnsManager1).InnerText;

                if (!actualTotalAmount.Equals(differenceAmount))
                {
                    result.Message.Add($"Invalid Total Amount, was expecting {differenceAmount} but found {actualTotalAmount} instead.");
                }
            }

            return result.IsValid;
        }

        //PriceDifferential_UnEvenExchange
        public static bool ValidateUnEvenPriceDifferential(string OrderCreateResponse, string responseXml, string OrderChangeResponse, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();

            XmlDocument orderQuoteXmlDocument = new XmlDocument();

            XmlDocument orderChangeXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(OrderCreateResponse);

            orderQuoteXmlDocument.LoadXml(responseXml);

            orderChangeXmlDocument.LoadXml(OrderChangeResponse);

            XmlNamespaceManager orderCreateXmlnsManager = orderCreateRequestXmlDocument.CreateXmlNamespaceManager();

            string orderCreateTotalAmount = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", orderCreateXmlnsManager).InnerText;

            string orderQuotePriceAmount = orderQuoteXmlDocument.SelectSingleNode("//msg:IATA_OrderReshopRS/msg:Response/msg:ReshopResults/msg:ReshopOffers/msg:TotalPrice/msg:TotalAmount", orderCreateXmlnsManager).InnerText;

            string OrderChangeAmount = orderChangeXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", orderCreateXmlnsManager).InnerText;

            decimal createTotalAmount = Convert.ToDecimal(orderCreateTotalAmount);

            decimal quotePriceAmount = Convert.ToDecimal(orderQuotePriceAmount);

            decimal ActualOrderChangeAmount = Convert.ToDecimal(OrderChangeAmount);

            decimal Expectedorderchangeamt = createTotalAmount + quotePriceAmount;

            if (quotePriceAmount > 0 && ActualOrderChangeAmount != Expectedorderchangeamt)
            {

                result.Message.Add($"Add collect transaction: Expected orderchange amt is {Expectedorderchangeamt}, The actual orderchange amt is {ActualOrderChangeAmount}");

            }
            if (quotePriceAmount < 0 && ActualOrderChangeAmount != Expectedorderchangeamt)
            {
                result.Message.Add($"Refund transaction: Expected orderchange amt is {Expectedorderchangeamt}, The actual orderchange amt is {ActualOrderChangeAmount}");
            }

            return result.IsValid;
        }

        //OriginalIssueInfo
        public static bool ValidateOriginalIssueInfo(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateorderchangeXmlnsManager = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager orderchangeXmlnsManager = orderCreateorderchangeXmlnsManager.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateorderchangeXmlnsManager.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            int expectedTicketDocCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax", orderchangeXmlnsManager).Count;

            IGrouping<string, XmlNode>[] tickets = orderCreateorderchangeXmlnsManager.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:Order/ct:OrderItem/ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID  and  ct:Ticket/ct:Coupon/ct:FareBasisCode and ct:Ticket/ct:TicketDocTypeCode='T']", orderchangeXmlnsManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxRefID", orderchangeXmlnsManager).InnerText)
                                                            .ToArray();

            if (!expectedTicketDocCount.Equals(tickets.Length))
            {
                result.Message.Add($"Invalid tickets, actual{tickets.Length} but exepected{expectedTicketDocCount}");

            }

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            bool issueDates = tickets.All(x => x.All(y => y.SelectSingleNode("ct:OriginalIssueInfo/ct:IssueDate", orderchangeXmlnsManager).InnerText.Equals(todaysDate, StringComparison.OrdinalIgnoreCase)));

            if (!issueDates == true)
            {
                result.Message.Add("IssueDate date should be {todaysDate}");

            }

            bool airlineDesigCodes = tickets.All(x => x.All(y => y.SelectSingleNode("ct:OriginalIssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode", orderchangeXmlnsManager).InnerText.Equals("VS")));

            if (!airlineDesigCodes == true)
            {
                result.Message.Add("AirlineDesigCode should be VS");

            }

            bool ticketNumbers = tickets.All(x => x.All(y => y.SelectSingleNode("ct:OriginalIssueInfo/ct:TicketNumber", orderchangeXmlnsManager).InnerText.Equals(null)));

            if (!ticketNumbers == false)
            {
                result.Message.Add("Every ticket doctInfo should have TicketNumber");

            }
            return result.IsValid;
        }

        //ValidateRefundWarning
        public static bool ValidateRefundResidualWarning(string reQuoteXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderChangeRSDcoument = new XmlDocument();
            XmlDocument orderQuoteRSDcoument = new XmlDocument();

            orderChangeRSDcoument.LoadXml(responseXml);

            orderQuoteRSDcoument.LoadXml(reQuoteXml);
            string totalAmount = FindAndReturnDataHelper.ReturnReQuotedOfferrTotalAmount(orderQuoteRSDcoument);

            XmlNamespaceManager orderchangeXmlnsManager = orderChangeRSDcoument.CreateXmlNamespaceManager();

            //Order change 
            string warningCode = orderChangeRSDcoument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Warning/ct:Code", orderchangeXmlnsManager)?.InnerText;

            string warningText = orderChangeRSDcoument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Warning/ct:DescText", orderchangeXmlnsManager)?.InnerText;

            string warningURL = orderChangeRSDcoument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Warning/ct:URL", orderchangeXmlnsManager)?.InnerText;

            string countryCode = orderChangeRSDcoument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount/@CurCode", orderchangeXmlnsManager)?.InnerText;

            if (!warningCode.Is("745") || !warningText.Is($"Please initiate the {totalAmount?.Substring(1)}{countryCode} refund amount by submitting the airline’s manual form. Kindly use the URL provided.") || !warningURL.Is("https://help.virginatlantic.com/gb/en/contact-forms/refund-form.html?webform=ticket-refunds"))
            {
                result.Message.Add($"Incorrect warning message for refund residual Order Change Response, expected code is 745 and expected text is 'Please initiate the {totalAmount}{countryCode} Refund Amount  by submitting the airline’s manual form.Kindly use the URL provided.' but actual code is {warningCode} and actual text is {warningText}");
            }
            
            warningCode = orderQuoteRSDcoument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:Warning/ct:Code", orderchangeXmlnsManager)?.InnerText;

            warningText = orderQuoteRSDcoument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:Warning/ct:DescText", orderchangeXmlnsManager)?.InnerText;

            warningURL = orderQuoteRSDcoument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:Warning/ct:URL", orderchangeXmlnsManager)?.InnerText;

            if (!warningCode.Is("745") || !warningText.Is($"Please initiate the {totalAmount?.Substring(1)}{countryCode} refund amount by submitting the airline’s manual form. Kindly use the URL provided.") || !warningURL.Is("https://help.virginatlantic.com/gb/en/contact-forms/refund-form.html?webform=ticket-refunds"))
            {
                result.Message.Add($"Incorrect warning message for refund residual Order Quote Response, expected code is 745 and expected text is 'Please initiate the {totalAmount?.Substring(1)}{countryCode} refund amount by submitting the airline’s manual form. Kindly use the URL provided.' but actual code is {warningCode} and actual text is {warningText}");
            }

            return result.IsValid;
        }

        //ValidateRetainedDMSSegment
        public static bool ValidateRetainedDMSSegment(string orderCreateRS, string orderChangeRS, int retainSegmentCount, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateorderchangeXmlnsManager = new XmlDocument();
            XmlDocument orderChangeorderchangeXmlnsManager = new XmlDocument();

            XmlNamespaceManager orderchangeXmlnsManager = orderCreateorderchangeXmlnsManager.CreateXmlNamespaceManager();

            orderCreateorderchangeXmlnsManager.LoadXml(orderCreateRS);
            orderChangeorderchangeXmlnsManager.LoadXml(orderChangeRS);

            List<XmlNode> orderCreateDMS = orderCreateorderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", orderchangeXmlnsManager).OfType<XmlNode>().ToList();

            List<XmlNode> orderChangeDMS = orderChangeorderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", orderchangeXmlnsManager).OfType<XmlNode>().ToList();

            int dmsMatchedCount = 0;

            foreach (XmlNode node in orderCreateDMS)
            {
                foreach (XmlNode node2 in orderChangeDMS)
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

        //Validate Aircraft type name and code
        public static bool ValidateAirCarftTypeCode(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument OrderQuoteorderchangeXmlnsManager = new XmlDocument();

            XmlNamespaceManager orderchangeXmlnsManager = OrderQuoteorderchangeXmlnsManager.CreateXmlNamespaceManager();

            OrderQuoteorderchangeXmlnsManager.LoadXml(responseXml);

            XmlNodeList datedOperatingSegments = OrderQuoteorderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", orderchangeXmlnsManager);

            foreach (XmlNode datedOperatingSegment in datedOperatingSegments)
            {
                XmlNode AirCraftTypecodeNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeCode", orderchangeXmlnsManager);

                if (AirCraftTypecodeNode is null)
                {
                    result.Message.Add("AircrafttypeCode node is not found in one of the DatedOperatingSegments");

                }

                XmlNode AircraftTypeNameNode = datedOperatingSegment.SelectSingleNode("ct:CarrierAircraftType/ct:CarrierAircraftTypeName", orderchangeXmlnsManager);

                if (AircraftTypeNameNode is null)
                {
                    result.Message.Add("AircraftTypeName node is not found in one of the DatedOperatingSegments");

                }
            }

            return result.IsValid;
        }

        //validateGST
        public static bool ValidateGST(string requestXml, string responseXml, out ValidationResultHelper result, string gst = "", int gstPaxCount = 0)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateorderchangeXmlnsManager = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager orderchangeXmlnsManager = orderCreateorderchangeXmlnsManager.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateorderchangeXmlnsManager.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            XmlNode expectedPaxList = orderCreateorderchangeXmlnsManager.SelectSingleNode("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList", orderchangeXmlnsManager);
            XmlNode actualPaxList = orderCreateorderchangeXmlnsManager.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList", orderchangeXmlnsManager);

            XmlElement requestPaxList = paxListXmlDocument.CreateElement("Request");
            XmlElement responsePaxList = paxListXmlDocument.CreateElement("Response");

            requestPaxList.AppendChild(paxListXmlDocument.ImportNode(expectedPaxList.CloneNode(true), true));
            responsePaxList.AppendChild(paxListXmlDocument.ImportNode(actualPaxList.CloneNode(true), true));

            XmlElement datalistXmlNode = paxListXmlDocument.CreateElement("Datalist");

            datalistXmlNode.AppendChild(requestPaxList);
            datalistXmlNode.AppendChild(responsePaxList);

            paxListXmlDocument.AppendChild(datalistXmlNode);

            int expectedPaxListCount = expectedPaxList.SelectNodes("ct:Pax", orderchangeXmlnsManager).Count;
            int actualPaxListCount = actualPaxList.SelectNodes("ct:Pax", orderchangeXmlnsManager).Count;
            int validatedPaxListCount = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax", orderchangeXmlnsManager).Count;

            int validatedPaxListCount1 = paxListXmlDocument.SelectNodes("Datalist/Request/ct:PaxList/ct:Pax[ct:Employer/ct:Name = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:Name and ct:Employer/ct:SalesTaxRegistration/ct:CountryCode = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:CountryCode and  ct:Employer/ct:SalesTaxRegistration/ct:SalesTaxRegistrationID = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:SalesTaxRegistrationID and  ct:Employer/ct:SalesTaxRegistration/ct:PostalCode = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:SalesTaxRegistration/ct:PostalCode and ct:Employer/ct:TaxDeductedatSourceCapabilityInd = ../../../Response/ct:PaxList/ct:Pax/ct:Employer/ct:TaxDeductedatSourceCapabilityInd]", orderchangeXmlnsManager).Count;

            if (!string.IsNullOrEmpty(gst) && gstPaxCount > 0 && !gstPaxCount.Equals(validatedPaxListCount1))
            {
                result.Message.Add($"Invalid PaxList, was expecting {gstPaxCount} but found {validatedPaxListCount1} instead.");

            }

            if (!((expectedPaxListCount + actualPaxListCount) / 2).Equals(validatedPaxListCount))
            {
                result.Message.Add($"Invalid PaxList, was expecting {expectedPaxListCount} but found {validatedPaxListCount} instead.");

            }

            string contactInfoRefID = orderCreateorderchangeXmlnsManager.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax/ct:ContactInfoRefID", orderchangeXmlnsManager).InnerText;

            if (!contactInfoRefID.Equals("CI1"))
            {
                result.Message.Add($"Invalid contactInfoRefID in paxList, was expecting CI1 but found {contactInfoRefID} instead.");

            }

            return result.IsValid;
        }

        public static bool ValidateOperatingLegOrderChange(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateorderchangeXmlnsManager = new XmlDocument();

            XmlNamespaceManager orderchangeXmlnsManager = orderCreateorderchangeXmlnsManager.CreateXmlNamespaceManager();

            orderCreateorderchangeXmlnsManager.LoadXml(responseXml);

            int datedMarketingSegmentCount = orderCreateorderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", orderchangeXmlnsManager).Count;
            int datedLegSegmentCount = orderCreateorderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg", orderchangeXmlnsManager).Count;

            if (datedLegSegmentCount == datedMarketingSegmentCount)
            {
                int datedLegSegmentAcutalCount = orderCreateorderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedOperatingLegList/ct:DatedOperatingLeg[ct:Dep/ct:IATA_LocationCode!=ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:IATA_LocationCode and ct:Arrival/ct:IATA_LocationCode=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:IATA_LocationCode and ct:Dep/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Dep/ct:AircraftScheduledDateTime and ct:Arrival/ct:AircraftScheduledDateTime=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:Arrival/ct:AircraftScheduledDateTime and ct:DatedOperatingLegID=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingLegRefID and ct:CarrierAircraftType/ct:CarrierAircraftTypeCode and ct:CarrierAircraftType/ct:CarrierAircraftTypeName and ct:IATA_AircraftType/ct:IATA_AircraftTypeCode]", orderchangeXmlnsManager).Count;
                if (datedLegSegmentCount != datedLegSegmentAcutalCount)
                {
                    result.Message.Add($"Invalid datedLegSegmentAcutalCount, was expecting {datedLegSegmentCount} but found {datedLegSegmentAcutalCount} instead.");

                }
            }

            return result.IsValid;

        }

        public static bool ValidateBaggageAllowanceList(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderchangeXmlnsManager = new XmlDocument();
            orderchangeXmlnsManager.LoadXml(responseXml);

            XmlNamespaceManager orderchangeXmlnsManager11 = orderchangeXmlnsManager.CreateXmlNamespaceManager();

            XmlNodeList baggageAllowanceNodes = orderchangeXmlnsManager.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:BaggageAllowanceList/ct:BaggageAllowance", orderchangeXmlnsManager11);
            foreach (XmlNode baggageAllowance in baggageAllowanceNodes)
            {
                string typeCode = baggageAllowance.SelectSingleNode("ct:TypeCode", orderchangeXmlnsManager11).InnerText;

                if (!typeCode.Equals("Checked") && !typeCode.Equals("CarryOn"))
                {
                    result.Message.Add($"Invalid Type Code Expected: Checked or CarryOn, found {typeCode}");

                }

                XmlNodeList maximumDimensionAllowance = baggageAllowance.SelectNodes("ct:MaximumDimensionAllowance[ct:HeightMeasure and ct:LengthMeasure and ct:LengthUnitOfMeasurement and ct:LinearMeasure and ct:WidthMeasure]", orderchangeXmlnsManager11);

                if (maximumDimensionAllowance.Count != 2)
                {
                    result.Message.Add("Maximum dimension allowances are not returned");

                }

                XmlNodeList weightAllowance = baggageAllowance.SelectNodes("ct:WeightAllowance[ct:MaximumWeightMeasure and ct:WeightUnitOfMeasurement and ct:TotalMaximumWeightMeasure]", orderchangeXmlnsManager11);

                if (weightAllowance.Count != 2)
                {
                    result.Message.Add("Weight allowances are not returned");

                }

                int.TryParse(baggageAllowance.SelectSingleNode("ct:PieceAllowance/ct:TotalQty", orderchangeXmlnsManager11).InnerText, out int totalPieceQuant);
                int.TryParse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[1]/ct:MaximumWeightMeasure", orderchangeXmlnsManager11).InnerText, out int maximumWeightMeasure_M1);
                int.TryParse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[2]/ct:MaximumWeightMeasure", orderchangeXmlnsManager11).InnerText, out int maximumWeightMeasure_M2);
                int.TryParse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[1]/ct:TotalMaximumWeightMeasure", orderchangeXmlnsManager11).InnerText, out int totalMaximumWeightMeasure_T1);
                int.TryParse(baggageAllowance.SelectSingleNode("ct:WeightAllowance[2]/ct:TotalMaximumWeightMeasure", orderchangeXmlnsManager11).InnerText, out int totalMaximumWeightMeasure_T2);

                if (totalPieceQuant == 0 && (totalMaximumWeightMeasure_T1 != 0 || totalMaximumWeightMeasure_T2 != 0))
                {
                    result.Message.Add("TotalMaximumWeightMeasure should be 0 as total quantity is 0 ");

                }
                else if (totalPieceQuant > 0 && (totalMaximumWeightMeasure_T1 != maximumWeightMeasure_M1 * totalPieceQuant))
                {
                    result.Message.Add("TotalMaximumWeightMeasure1 is not as expected ");

                }
                else if (totalPieceQuant > 0 && (totalMaximumWeightMeasure_T2 != maximumWeightMeasure_M2 * totalPieceQuant))
                {
                    result.Message.Add("TotalMaximumWeightMeasure2 is not as expected ");

                }
            }
            return result.IsValid;
        }

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

            XmlNodeList expectedService = requestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:ChangeOrderChoice/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer/ct:SelectedOfferItem[ct:SelectedALaCarteOfferItem/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID]", xmlNamespaceManager);

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

                int segmentCount = requestXmlDocument.SelectNodes($"msg:IATA_OrderChangeRQ/msg:Request/ct:ChangeOrderChoice/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer/ct:SelectedOfferItem[{selectedOfferItemCounter}]/ct:SelectedALaCarteOfferItem/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                int paxCount = requestXmlDocument.SelectNodes($"msg:IATA_OrderChangeRQ/msg:Request/ct:ChangeOrderChoice/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[1]/ct:SelectedOfferItem[{selectedOfferItemCounter}]/ct:PaxRefID", xmlNamespaceManager).Count;

                XmlNodeList actualServiceCount = item.SelectNodes("ct:Service[ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:ServiceDefinitionRefID=../../../ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition/ct:ServiceDefinitionID and ct:BookingRef/ct:BookingEntity/ct:Carrier/ct:AirlineDesigCode='VS' and  ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:StatusCode='CONFIRMED' and ct:ServiceID]", xmlNamespaceManager);

                if (actualServiceCount.Count != segmentCount * paxCount)
                {
                    result.Message.Add("Incorrect pax and segments in the SSR order item");
                }

            }

            return result.IsValid;
        }

        //Validate Unique OrderItemId
        public static bool ValidateUniqueOrderItemId(XmlDocument responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlNamespaceManager orderchangeXmlnsManager = responseXml.CreateXmlNamespaceManager();

            XmlNodeList orderItemNodes = responseXml.SelectNodes("//ct:OrderItem", orderchangeXmlnsManager);
            XmlNodeList paymentFunctionNodes = responseXml.SelectNodes("//msg:PaymentFunctions/ct:OrderAssociation", orderchangeXmlnsManager);

            if (orderItemNodes == null || paymentFunctionNodes == null)
            {
                result.Message.Add($"Either orderItem or paymentFunction is found as Null");
            }

            // Validate uniqueness of OrderItemID
            HashSet<string> orderItemIds = new HashSet<string>();
            if (orderItemNodes != null)
            {
                foreach (XmlNode orderItemNode in orderItemNodes)
                {
                    XmlNode OrderItemIDNode = orderItemNode.SelectSingleNode("ct:OrderItemID", orderchangeXmlnsManager);
                    if (OrderItemIDNode == null)
                    {
                        result.Message.Add("OrderItemID not found in OrderItem node.");
                        continue;
                    }

                    string orderItemId = OrderItemIDNode.InnerText.Trim();
                    if (orderItemIds.Contains(orderItemId))
                    {
                        result.Message.Add($"Duplicate OrderItemID '{orderItemId}' found.");
                    }

                    orderItemIds.Add(orderItemId);
                }
            }

            // Validate presence of OrderItemRefID in payment functions nodes
            if (paymentFunctionNodes != null)
            {
                foreach (XmlNode paymentFunctionNode in paymentFunctionNodes)
                {
                    XmlNode OrderItemRefIDNode = paymentFunctionNode.SelectSingleNode("ct:OrderItemRefID", orderchangeXmlnsManager);
                    if (OrderItemRefIDNode == null)
                    {
                        result.Message.Add("OrderItemRefID not found in PaymentFunctions node.");
                    }
                }
            }

            int i = 0; // Index for orderItemNodes
            int j = 1; // Index for paymentFunctionNodes

            if (orderItemNodes != null && paymentFunctionNodes != null)
            {
                while (i < orderItemNodes.Count && j < paymentFunctionNodes.Count && j < 4)
                {
                    XmlNode orderItemNode = orderItemNodes[i];
                    XmlNode OrderItemIDNode = orderItemNode.SelectSingleNode("ct:OrderItemID", orderchangeXmlnsManager);

                    XmlNode paymentFunctionNode = paymentFunctionNodes[j];
                    XmlNode OrderItemRefIDNode = paymentFunctionNode.SelectSingleNode("ct:OrderItemRefID", orderchangeXmlnsManager);

                    // Set InnerText of OrderItemRefIDNode to match InnerText of OrderItemIDNode
                    if (OrderItemRefIDNode != null && OrderItemIDNode != null)
                    {
                        OrderItemRefIDNode.InnerText = OrderItemIDNode.InnerText;
                    }

                    i++;
                }
            }
            // Return the Result
            return result.IsValid;
        }

        public static bool ValidateRfcOrderViewRS(string createRsXml, string changeRsXml, out ValidationResultHelper result, string flow = "")
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRsDoc = new XmlDocument();
            XmlDocument orderChangeRsDoc = new XmlDocument();

            orderCreateRsDoc.LoadXml(createRsXml);
            orderChangeRsDoc.LoadXml(changeRsXml);

            XmlNamespaceManager xmlnsManager = orderCreateRsDoc.CreateXmlNamespaceManager();

            XmlNodeList orderItem = orderChangeRsDoc.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:FareDetail]", xmlnsManager);

            string orderId = orderCreateRsDoc.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlnsManager).InnerText;

            string actualOrderId = orderChangeRsDoc.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlnsManager).InnerText;

            Dictionary<string, int> result1 = FindAndReturnDataHelper.ReturnPaxCount(orderCreateRsDoc);

            if (!actualOrderId.Is(orderId))
            {
                result.Message.Add($"Incorrect Orderid Expected -> {orderId} but actual -> {actualOrderId}");
            }

            foreach (XmlNode orderItemNode in orderItem)
            {
                XmlNodeList fareDeatilPax = orderItemNode.SelectNodes("ct:FareDetail[ct:PaxRefID]", xmlnsManager);

                Dictionary<string, int> pax = FindAndReturnDataHelper.ReturnPaxCount(orderCreateRsDoc);

                if ((fareDeatilPax[0].InnerText.Is("Adult_1") && fareDeatilPax.Count != pax["Adult"]) || (fareDeatilPax[0].InnerText.Is("Young_1") && fareDeatilPax.Count != pax["YoungAdult"]) || (fareDeatilPax[0].InnerText.Is("Child_1") && fareDeatilPax.Count != pax["Child"]) || (fareDeatilPax[0].InnerText.Is("Infant_1") && fareDeatilPax.Count != pax["Infant"]))
                {
                    result.Message.Add($"Incorrect pax in faredeatil Expected {orderItemNode.OuterXml}");
                }

                //validate DiffPrice

                decimal diffPriceBaseAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:BaseAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal diffPriceDueByAirlineAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:DueByAirlineAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal diffPriceTotalTaxAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal diffPriceTotalAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:DiffPrice/ct:Price/ct:TotalAmount", xmlnsManager).InnerText.Parse<decimal>();

                if (flow.Is("rfc") && (diffPriceDueByAirlineAmount != Math.Abs(diffPriceTotalTaxAmount + diffPriceBaseAmount)) || (diffPriceTotalAmount != diffPriceTotalTaxAmount + diffPriceBaseAmount))
                {
                    result.Message.Add($"Incorrect amount in Differential price {orderItemNode.OuterXml}");
                }

                if ((diffPriceTotalAmount != diffPriceTotalTaxAmount + diffPriceBaseAmount) || (diffPriceDueByAirlineAmount > Math.Abs(diffPriceTotalAmount)))
                {
                    result.Message.Add($"Incorrect amount in Differential price {orderItemNode.OuterXml}");
                }

                // validate newprice
                decimal newPriceBaseAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:NewPrice/ct:Price/ct:BaseAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal newPriceTotalTaxAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal newPriceTotalAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:NewPrice/ct:Price/ct:TotalAmount", xmlnsManager).InnerText.Parse<decimal>();

                if (newPriceBaseAmount != 0 || newPriceTotalTaxAmount != 0 || newPriceTotalAmount != 0)
                {
                    result.Message.Add($"Incorrect amount in New price price {orderItemNode.OuterXml}");
                }

                //validate oldprice
                decimal oldPriceBaseAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:BaseAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal oldPriceTotalTaxAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TaxSummary/ct:TotalTaxAmount", xmlnsManager).InnerText.Parse<decimal>();

                XmlNodeList oldPriceTax = orderItemNode.SelectNodes("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TaxSummary/ct:Tax", xmlnsManager);

                decimal oldPriceTotalAmount = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:OldPrice/ct:Price/ct:TotalAmount", xmlnsManager).InnerText.Parse<decimal>();

                decimal expectedTotalTaxAmount = 0;

                foreach (XmlNode taxNode in oldPriceTax)
                {
                    decimal amount = taxNode.SelectSingleNode("ct:Amount", xmlnsManager).InnerText.Parse<decimal>();

                    expectedTotalTaxAmount = expectedTotalTaxAmount + amount;
                }

                if ((oldPriceTotalAmount != oldPriceBaseAmount + oldPriceTotalTaxAmount) || (expectedTotalTaxAmount != oldPriceTotalTaxAmount))
                {
                    result.Message.Add($"Incorrect amount in Old price {orderItemNode.OuterXml}");
                }

                if ((oldPriceTotalAmount != Math.Abs(diffPriceTotalAmount)) || (diffPriceDueByAirlineAmount > oldPriceTotalAmount))
                {
                    result.Message.Add($"Incorrect amount in Differential price {orderItemNode.OuterXml}");
                }

                //Validate DifferentialTypeCode

                string differentialTypeCode = orderItemNode.SelectSingleNode("ct:PriceDifferential/ct:DifferentialTypeCode", xmlnsManager).InnerText;

                if (diffPriceTotalAmount < 0 && !differentialTypeCode.Is("Refund"))
                {
                    result.Message.Add($"Incorrect differentialTypeCode expected refund but actual is {differentialTypeCode}");
                }

                //get servicenodes

                int serviceNodes = orderItemNode.SelectNodes("ct:Service", xmlnsManager).Count;

                int deliveryStatusCode = orderItemNode.SelectNodes("ct:Service[ct:DeliveryStatusCode='REMOVED']", xmlnsManager).Count;

                int statusCode = orderItemNode.SelectNodes("ct:Service[ct:StatusCode='CANCELLED']", xmlnsManager).Count;

                int segmentServiceCount = orderItemNode.SelectNodes("ct:Service[ct:PaxRefID and ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID and ct:ServiceID]", xmlnsManager).Count;

                int flightServiceCount = orderItemNode.SelectNodes("ct:Service[ct:PaxRefID and ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:OrderFlightAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID  and ct:ServiceID and ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:ServiceDefinitionRefID]", xmlnsManager).Count;

                int serviceRefIDcount = orderItemNode.SelectNodes("ct:Service[ct:ServiceRefID]", xmlnsManager).Count;

                if (deliveryStatusCode != serviceNodes)
                {
                    result.Message.Add($"deliveryStatusCode is not returned in all services {orderItemNode.OuterXml}");
                }

                if (statusCode != serviceNodes)
                {
                    result.Message.Add($"statusCode is not returned in all services {orderItemNode.OuterXml}");
                }

                if (flightServiceCount != serviceNodes - segmentServiceCount)
                {
                    result.Message.Add($"Actual flightServiceCount {flightServiceCount} Expected flight services count{serviceNodes - segmentServiceCount}");
                }

                if (serviceRefIDcount != flightServiceCount)
                {
                    result.Message.Add($"Actual serviceRefID {serviceRefIDcount} Expected ServiceRefID count{serviceNodes - segmentServiceCount}");
                }

            }

            return result.IsValid;
        }

        public static bool ValidateOrderItemID(string requestXml, string responseXml, string orderQuoteRsXml, out ValidationResultHelper result, string exchangeType = "")
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderChangeResponseXmlDocument = new XmlDocument();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderChangeResponseXmlDocument.LoadXml(responseXml);
            orderQuoteResponseXmlDocument.LoadXml(orderQuoteRsXml);

            XmlNamespaceManager xmlNamespaceManager = orderChangeResponseXmlDocument.CreateXmlNamespaceManager();

            double? expectedTotalPrice = orderCreateRequestXmlDocument.SelectSingleNode("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[1]/ct:TotalOfferPriceAmount", xmlNamespaceManager)?.InnerText.Parse<double>();
            double actualTotalPrice = orderChangeResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:TotalPrice/ct:TotalAmount", xmlNamespaceManager).InnerText.Parse<double>();

            double requotedOfferAmount = FindAndReturnDataHelper.ReturnReQuotedOfferrTotalAmount(orderQuoteResponseXmlDocument).Parse<double>();

            if (Math.Round(Convert.ToDecimal(actualTotalPrice), 2) != Math.Round(Convert.ToDecimal(expectedTotalPrice), 2) + Math.Round(Convert.ToDecimal(requotedOfferAmount), 2))
            {
                result.Message.Add($"expected price  {expectedTotalPrice} but actual is {actualTotalPrice}");
            }

            int expectedOrderItemCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderCreateRQ/msg:Request/ct:CreateOrder/ct:AcceptSelectedQuotedOfferList/ct:SelectedPricedOffer[1]/ct:SelectedOfferItem", xmlNamespaceManager).Count;

            XmlNodeList actualOrderItem = orderChangeResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem[ct:FareDetail]", xmlNamespaceManager);

            if (actualOrderItem.Count != expectedOrderItemCount * 2)
            {
                result.Message.Add($"Expected order item count {expectedOrderItemCount * 2} but actual {actualOrderItem.Count}");
            }

            int taxCount = orderChangeResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price/ct:TaxSummary/ct:Tax", xmlNamespaceManager).Count;

            int descText = orderChangeResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price/ct:TaxSummary/ct:Tax/ct:DescText", xmlNamespaceManager).Count;

            int taxCode = orderChangeResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderItem/ct:Price/ct:TaxSummary/ct:Tax/ct:TaxCode", xmlNamespaceManager).Count;

            if (taxCount != descText || taxCode != taxCount)
            {
                result.Message.Add($"Expected DescText and TaxCode count in Taxsummary {taxCount} but descText {descText} and taxCode {taxCode}.");
            }

            string orderID = orderChangeResponseXmlDocument.SelectSingleNode("msg:IATA_OrderViewRS/msg:Response/ct:Order/ct:OrderID", xmlNamespaceManager).InnerText;

            int segmentCount = orderChangeResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[not(contains(ct:DatedMarketingSegmentId, 'OLD'))]", xmlNamespaceManager).Count;

            int oldSegmentCount = orderChangeResponseXmlDocument.SelectNodes("msg:IATA_OrderViewRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[contains(ct:DatedMarketingSegmentId, 'OLD')]", xmlNamespaceManager).Count;

            int orderItemCounter = 0;

            foreach (XmlNode orderItem in actualOrderItem)
            {
                orderItemCounter++;
                string orderItemValue = orderItem.SelectSingleNode("ct:OrderItemID", xmlNamespaceManager).InnerText;
                int paxCount = orderItem.SelectNodes("ct:FareDetail/ct:PaxRefID", xmlNamespaceManager).Count;

                int serviceExpectedCount = orderItem.SelectNodes("ct:Service", xmlNamespaceManager).Count;
                string orderItemStatus = orderItem.SelectSingleNode("ct:StatusCode", xmlNamespaceManager).InnerText;

                int totalOrderItemServiceCount = orderItem.SelectNodes("ct:Service", xmlNamespaceManager).Count;

                int segmentServiceCount = orderItem.SelectNodes($"ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID and ct:ServiceID]", xmlNamespaceManager).Count;

                int flightServiceCount = orderItem.SelectNodes($"ct:Service[ct:PaxRefID=../../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:OrderFlightAssociations/ct:PaxSegmentRef/ct:PaxSegmentRefID and ct:ServiceID and ct:OrderServiceAssociation/ct:ServiceDefinitionRef/ct:ServiceDefinitionRefID]", xmlNamespaceManager).Count;

                int serviceRefIDcount = orderItem.SelectNodes($"ct:Service[ct:ServiceRefID]", xmlNamespaceManager).Count;

                if (orderItemCounter > expectedOrderItemCount)
                {
                    int serviceStatus = orderItem.SelectNodes("ct:Service[ct:DeliveryStatusCode='READY TO PROCEED' and ct:StatusCode='CONFIRMED']", xmlNamespaceManager).Count;

                    if (serviceStatus != serviceExpectedCount || !orderItemStatus.Is("ACTIVE"))
                    {
                        result.Message.Add($"Incorrect service deliveryStatusCode/Status Code/ order item status code in orderitem services {orderItem.OuterXml}");
                    }

                }
                else
                {
                    int cancelledServiceStatus = orderItem.SelectNodes("ct:Service[ct:DeliveryStatusCode='REMOVED' and ct:StatusCode='CANCELLED']", xmlNamespaceManager).Count;

                    int transferredServiceStatus = orderItem.SelectNodes("ct:Service[ct:DeliveryStatusCode='READY TO PROCEED' and ct:StatusCode='TRANSFERRED']", xmlNamespaceManager).Count;

                    if (cancelledServiceStatus + transferredServiceStatus != serviceExpectedCount)
                    {
                        result.Message.Add($"Incorrect service deliveryStatusCode/Status Code/ order item status code in orderitem services {orderItem.OuterXml}");
                    }

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

        public static bool IsValidSurnameChange(string name, string changeName, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            string originalName = name;
            string originalChangeName = changeName;

            if (name.Length > changeName.Length)
                changeName = changeName.PadRight(name.Length, char.MinValue);
            else if (changeName.Length > name.Length)
                name = name.PadRight(changeName.Length, char.MinValue);

            char[] oneChars = name.ToCharArray();
            char[] twoChars = changeName.ToCharArray();

            int numberOfChanges = 0;

            var map = oneChars.Zip(twoChars)
                              .Select(x => new
                              {
                                  Old = x.First,
                                  New = x.Second,
                                  Changed = !x.First.ToString().Is(x.Second.ToString()),
                                  Type = x.First.ToString().Is(x.Second.ToString()) ? "Match" : x.First == char.MinValue ? "Addition" : x.Second == char.MinValue ? "Subtraction" : "Substitution"
                              })
                              .ToArray();

            numberOfChanges = map.Count(x => x.Changed);

            bool contains = originalChangeName.Contains(originalName);
            bool sameLength = originalName.Length == originalChangeName.Length;

            if (numberOfChanges > 3)
            {
                if (!contains)
                    result.Message.Add($"Number of changes are >3 but the Original Name is not contained within the change. Original {originalName} and Change {originalChangeName}");
            }
            else
            {
                if (!sameLength && !contains)
                    result.Message.Add($"Number of changes are =<3 but the Original length is not the same as before. Original {originalName} and Change {originalChangeName}");
            }

            return result.IsValid;
        }

        public static bool ValidateSplitPNRErrorRS(string responseXml, out ValidationResultHelper result, string code = "", string desc = "", string tagText = "", string typeCode = "")
        {
            result = new ValidationResultHelper();

            XmlDocument responseXmlDocument = new XmlDocument();

            responseXmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = responseXmlDocument.CreateXmlNamespaceManager();

            XmlNodeList error = responseXmlDocument.SelectNodes($"//msg:Error[ct:DescText='{desc}' and ct:Code='{code}' and ct:TagText='{tagText}' and ct:TypeCode='{typeCode}']", xmlNamespaceManager);

            if (error.Count == 0)
            {
                result.Message.Add($"Incorrect error response returned {responseXmlDocument.SelectSingleNode("//msg:Error", xmlNamespaceManager)?.OuterXml}");
            }

            return result.IsValid;
        }

        //OriginalIssueInfo
        public static bool ValidateOrderCancelPaymentFunctions(string requestXml, string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument orderCreateRequestXmlDocument = new XmlDocument();
            XmlDocument orderCreateorderchangeXmlnsManager = new XmlDocument();

            orderCreateRequestXmlDocument.LoadXml(requestXml);

            XmlNamespaceManager orderchangeXmlnsManager = orderCreateorderchangeXmlnsManager.CreateXmlNamespaceManager();

            orderCreateRequestXmlDocument.LoadXml(requestXml);
            orderCreateorderchangeXmlnsManager.LoadXml(responseXml);

            XmlDocument paxListXmlDocument = new XmlDocument();

            int expectedTicketDocCount = orderCreateRequestXmlDocument.SelectNodes("msg:IATA_OrderChangeRQ/msg:Request/ct:DataLists/ct:PaxList/ct:Pax", orderchangeXmlnsManager).Count;

            IGrouping<string, XmlNode>[] tickets = orderCreateorderchangeXmlnsManager.SelectNodes($"msg:IATA_OrderViewRS/msg:Response/ct:TicketDocInfo[ct:PaxRefID=../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID  and ct:Ticket/ct:Coupon/ct:SoldAirlineInfo/ct:PaxSegmentRefID = ../ct:Order/ct:OrderItem/ct:Service/ct:OrderServiceAssociation/ct:PaxSegmentRef/ct:PaxSegmentRefID  and  ct:Ticket/ct:Coupon/ct:FareBasisCode]", orderchangeXmlnsManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:PaxRefID", orderchangeXmlnsManager).InnerText)
                                                            .ToArray();

            if (!expectedTicketDocCount.Equals(tickets.Length))
            {
                result.Message.Add($"Invalid tickets, actual{tickets.Length} but exepected{expectedTicketDocCount}");
            }

            string todaysDate = DateTime.Now.ToString("yyyy-MM-dd");

            bool issueDates = tickets.All(x => x.All(y => y.SelectSingleNode("ct:OriginalIssueInfo/ct:IssueDate", orderchangeXmlnsManager).InnerText.Is(todaysDate)));

            if (!issueDates == true)
            {
                result.Message.Add($"IssueDate date should be {todaysDate}");
            }

            bool airlineDesigCodes = tickets.All(x => x.All(y => y.SelectSingleNode("ct:OriginalIssueInfo/ct:IssuingCarrier/ct:AirlineDesigCode", orderchangeXmlnsManager).InnerText.Equals("VS")));

            if (!airlineDesigCodes == true)
            {
                result.Message.Add("AirlineDesigCode should be VS");
            }

            bool ticketNumbers = tickets.All(x => x.All(y => y.SelectSingleNode("ct:OriginalIssueInfo/ct:TicketNumber", orderchangeXmlnsManager).InnerText.Equals(null)));

            if (!ticketNumbers == false)
            {
                result.Message.Add("Every ticket doctInfo should have TicketNumber");
            }
            return result.IsValid;
        }


    }
}
