using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Delta.Endpoint
{
    public static class AIR4Facade
    {

        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage RetrieveSpecificAvailability(string appChannelName, string applicationId, string airlineCode, string classOfService, string destinationStation, string flightNumber, string originStation, string tripDate)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\ScheduleAvailbility\\RetrieveSpecificAvailability.xml"), appChannelName, applicationId, airlineCode, classOfService, destinationStation, flightNumber, originStation, tripDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "RetrieveSpecificAvailability" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ScheduleAvailability"), request, headers);
        }

        public static HttpResponseMessage RetrieveSpecificAvailabilityWithSessionId(string appChannelName, string applicationId, string airlineCode, string classOfService, string destinationStation, string flightNumber, string originStation, string tripDate, string sessionId)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\ScheduleAvailbility\\RetrieveSpecificAvailabilityWithSessionId.xml"), appChannelName, applicationId, airlineCode, classOfService, destinationStation, flightNumber, originStation, tripDate, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "RetrieveSpecificAvailability" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ScheduleAvailability"), request, headers);
        }

        public static HttpResponseMessage RetrieveBetweenCities(string appchannelname, string applicationid, string airlinecode, string destinationstation, string originstation, string tripdate)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\Air4\\Scheduleavailbility\\RetrieveBetweenCities.xml"), appchannelname, applicationid, airlinecode, destinationstation, originstation, tripdate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "RetrieveBetweenCities" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ScheduleAvailability"), request, headers);
        }

        public static HttpResponseMessage CreateContextFacade(string appChannelName, string applicatonId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\Create.xml"), appChannelName, applicatonId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "create" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/hostcontext"), request, headers);
        }

        public static HttpResponseMessage SwingCityDutyCodeFacade(string sessionId, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\SwingCityDutyCode.xml"), sessionId, appChannelName, applicationId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "SwingCityDutyCode" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/hostcontext"), request, headers);
        }

        public static HttpResponseMessage DestroyContextFacade(string sessionId, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\Destroy.xml"), sessionId, appChannelName, applicationId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "destroy" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/hostcontext"), request, headers);
        }

        public static HttpResponseMessage CreateSwingFacade(string appChannelName, string applicatonId, string cityCode, string cityFlag = "")
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\CreateAndSwing.xml"), appChannelName, applicatonId, cityCode);

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(request);

            //For Error Handling scenario, replacing city code with invalid value
            if (!cityFlag.IsNullOrEmpty())
            {
                XmlNamespaceManager createSwingRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var cityNode = xmlDocument?.SelectSingleNode("//v4:HostContextParams/@City", createSwingRequestManager);

                cityNode.Value = "LO";
            }

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "CreateAndSwing" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/hostcontext"), request, headers);
        }

        public static HttpResponseMessage RetrievePnrCommon(string appChannelName, string applicationId, string recordLocator, out string requestRetrieveFacade, string stampingInfoFlag = "")
        {
            requestRetrieveFacade = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\RetrievePnr.xml"), appChannelName, applicationId, recordLocator);

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(requestRetrieveFacade);

            //For other AppChannel scenario except NDC , removing the StampingInfo node from request

            if (!stampingInfoFlag.IsNullOrEmpty())
            {
                XmlNamespaceManager dividePnrRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var stampingInfoNode = xmlDocument?.SelectSingleNode("//stamping:StampingInfo", dividePnrRequestManager);

                stampingInfoNode?.ParentNode?.RemoveChild(stampingInfoNode);
            }

            requestRetrieveFacade = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Retrieve" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestRetrieveFacade, headers);
        }

        public static HttpResponseMessage SetupContextPnrCommon(string appChannelNameSetupContext, string applicationIdSetupContext, out string setUpRequest, string recordLocator)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\SetupContext.xml"), appChannelNameSetupContext, applicationIdSetupContext, recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "SetupContext" }
            };

            setUpRequest = request;

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), request, headers);
        }

        public static HttpResponseMessage InsertSegmentFacade(string appChannelName, string applicationId, string sessionId, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, string numberParty, string status)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SegmentV4\\Insert.xml"), appChannelName, applicationId, sessionId, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber, numberParty, status);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Insert" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/segment"), request, headers);
        }

        public static HttpResponseMessage TicketingProcessEmailReceiptFacade(string appChannelName, string applicationId, string sessionId, string emailId, out string emailReceiptRequestFacade)
        {
            emailReceiptRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\EmailReceipt.xml"), appChannelName, applicationId, sessionId, emailId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "EmailReceipt" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), emailReceiptRequestFacade, headers);
        }

        public static HttpResponseMessage TicketingProcessRetrieveFacade(string appChannelName, string applicationId, string electronicDocumentNumber, out string retrieveRequestFacade, string sessionId = "")
        {
            retrieveRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\Retrieve.xml"), appChannelName, applicationId, electronicDocumentNumber, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Retrieve" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), retrieveRequestFacade, headers);
        }
        public static HttpResponseMessage TicketingProcessPerformRevenueFacade(string applicationId, string sessionId, string cvccIdNumber, string creditCardExpiration, string creditCardNumber, string creditCardType, string currencyCode, string customerFirstName, string customerLastName, string paymentAmount, out string performRevenueRequestFacade)
        {
            performRevenueRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\PerformRevenue.xml"), applicationId, sessionId, cvccIdNumber, creditCardExpiration, creditCardNumber, creditCardType, currencyCode, customerFirstName, customerLastName, paymentAmount);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "PerformRevenue" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), performRevenueRequestFacade, headers);
        }

        public static HttpResponseMessage TicketingProcessPerformRevenueItravelFacade(string appChannelName, string applicationId, string sessionId, string formOfPayment, out string performRevenueRequestFacade)
        {
            performRevenueRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\PerformRevenueItravel.xml"), appChannelName, applicationId, sessionId, formOfPayment);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "PerformRevenue" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), performRevenueRequestFacade, headers);
        }

        public static HttpResponseMessage MaintainTicketingInstructionsForToursFacade(string appChannelName, string applicationId, string sessionId, string toursCode, out string ticketingForToursRequestFacade)
        {
            ticketingForToursRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\Tours.xml"), appChannelName, applicationId, sessionId, toursCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "MaintainTicketingInstructionsForTours" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), ticketingForToursRequestFacade, headers);
        }

        public static HttpResponseMessage TicketingProcessModifyTicketingTimeLimitFacade(string appChannelName, string applicationId, string sessionId, string ticketingTimeLimitInfo, string addTicketingInfoIndicator, out string modifyTicketingTimeLimitRequestFacade)
        {
            modifyTicketingTimeLimitRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\ModifyTicketing.xml"), appChannelName, applicationId, sessionId, ticketingTimeLimitInfo, addTicketingInfoIndicator);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "ModifyTicketingTimeLimit" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), modifyTicketingTimeLimitRequestFacade, headers);
        }

        public static HttpResponseMessage FacadeTicket(X509Certificate2 x509Certificate, string appChannelName, string applicationId)
        {
            var rawRequest = string.Format(File.ReadAllText("requests\\Body\\EIE\\TicketingProcessV3\\Retrieve.xml"), appChannelName, applicationId);

            string request = XDocument.Parse(rawRequest).ToString();

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            rawRequest = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Retrieve" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), rawRequest, headers);
        }

        public static HttpResponseMessage DeleteSegmentFacade(string appChannelName, string applicationId, string sessionId, string segmentNumber)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SegmentV4\\Delete.xml"), appChannelName, applicationId, sessionId, segmentNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Delete" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/segment"), request, headers);
        }

        public static HttpResponseMessage CreatePnrFacade(string appChannelName, string applicationId, string arrivalPoint, string arrivalTime, string classOfService, string departureDate, string departurePoint, string departureTime, string flightNumber)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\CreatePnr.xml"), appChannelName, applicationId, arrivalPoint, arrivalTime, classOfService, departureDate, departurePoint, departureTime, flightNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Create" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), request, headers);
        }
        public static HttpResponseMessage TearDownContext(string appChannelName, string applicationId, out string teardownRequest, string sessionId, string requestFlag = "", string stampingInfoFlag = "")
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\TearDownContext.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(request);

            //for NewPNR scenario, removing the EndRequest node from request
            if (!string.IsNullOrEmpty(requestFlag))
            {
                XmlNamespaceManager teardownRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var endRequestFacade = xmlDocument?.SelectSingleNode("//v3:EndRequest", teardownRequestManager);

                endRequestFacade?.ParentNode?.RemoveChild(endRequestFacade);
            }

            //For other AppChannel scenario except NDC , removing the StampingInfo node from request
            if (!stampingInfoFlag.IsNullOrEmpty())
            {
                XmlNamespaceManager tearPnrRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var stampingInfoNode = xmlDocument?.SelectSingleNode("//stamping:StampingInfo", tearPnrRequestManager);

                stampingInfoNode?.ParentNode?.RemoveChild(stampingInfoNode);
            }

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "TeardownContext" }
            };

            teardownRequest = request;

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), request, headers);
        }

        public static HttpResponseMessage CheckInBaggageAddAndPrintIP(string applicatioName, string applicationId, string sessionId, string actionCode)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Checkin-Baggage-AcceptAndPrint\\Execute_IP.xml"), applicatioName, applicationId, sessionId, actionCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Execute" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/checkinbaggageacceptandprint"), request, headers);

        }
        public static HttpResponseMessage CheckInBaggageAddAndPrintUP(string applicatioName, string applicationId, string sessionId, string actionCode)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Checkin-Baggage-AcceptAndPrint\\Execute_UP.xml"), applicatioName, applicationId, sessionId, actionCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Execute" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/checkinbaggageacceptandprint"), request, headers);
        }

        public static HttpResponseMessage CheckInBaggageAddAndPrintAP(string applicatioName, string applicationId, string sessionId, string actionCode)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Checkin-Baggage-AcceptAndPrint\\Execute_AP.xml"), applicatioName, applicationId, sessionId, actionCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Execute" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/checkinbaggageacceptandprint"), request, headers);
        }

        public static HttpResponseMessage DividePnrCommon(string appChannelName, string applicationId, string sessionId, out string requestDivideFacade, string stampingInfoFlag = "")
        {
            requestDivideFacade = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\DividePnr.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(requestDivideFacade);

            //For other AppChannel scenario except NDC , removing the StampingInfo node from request
            if (!stampingInfoFlag.IsNullOrEmpty())
            {
                XmlNamespaceManager dividePnrRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var stampingInfoNode = xmlDocument?.SelectSingleNode("//stamping:StampingInfo", dividePnrRequestManager);

                stampingInfoNode?.ParentNode?.RemoveChild(stampingInfoNode);
            }

            requestDivideFacade = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Divide" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestDivideFacade, headers);
        }

        public static HttpResponseMessage RefreshPnrCommon(string appChannelName, string applicationId, string sessionId, out string requestRefreshFacade)
        {
            requestRefreshFacade = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\RefreshPnr.xml"), appChannelName, applicationId, sessionId);

            //There is an unrecognized white space coming in RQ sample due to that converting to Xml doc
            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(requestRefreshFacade);

            requestRefreshFacade = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Refresh" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestRefreshFacade, headers);
        }

        public static HttpResponseMessage EndPnrCommon(string appChannelName, string applicationId, string sessionId, string airline, string redisplayFlag, out string requestEndFacade)
        {
            requestEndFacade = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\EndPnr.xml"), appChannelName, applicationId, sessionId, airline, redisplayFlag);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "End" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestEndFacade, headers);
        }

        public static HttpResponseMessage PnrListRetrieveWithSeatStatusFacade(string flightLegArrivalAirportCd, string flightLegArrivalDate, string flightLegDepartureAirportCd, string flightLegDepartureDate, string flightNumber, string applicationName)

        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PnrList\\PnrListRetrieveWithSeatStatus.xml"), flightLegArrivalAirportCd, flightLegArrivalDate, flightLegDepartureAirportCd, flightLegDepartureDate, flightNumber, applicationName);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveWithSeatStatus" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrlist"), request, headers);
        }

        public static HttpResponseMessage PnrListRetrieveWithSsrFacade(string arrivalAirport, string departureAirport, string departureDate, string flightNumber, string appChannelName, string applicationId, string ssrTypeCode)

        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PnrList\\PnrListRetrieveWithSsr.xml"), arrivalAirport, departureAirport, departureDate, flightNumber, appChannelName, applicationId, ssrTypeCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveWithSSR" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrlist"), request, headers);
        }

        public static HttpResponseMessage PnrListRetrieveFacade(string departureAirportCd, string flightnum, string depDate, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PnrList\\PnrListRetrieve.xml"), departureAirportCd, flightnum, depDate, appChannelName, applicationId);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Retrieve" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrlist"), request, headers);
        }

        public static HttpResponseMessage PnrListRetrieveElitePassengersFacade(string applicationName, string applicationId, string flightLegArrivalAirportCd, string flightLegDepartureAirportCd, string flightLegDepartureDate, string flightNumber, out string requestRetrieveEilteFacade)
        {
            requestRetrieveEilteFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PnrList\\RetrieveElitePassengers.xml"), applicationName, applicationId, flightLegArrivalAirportCd, flightLegDepartureAirportCd, flightLegDepartureDate, flightNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveElitePassengers" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrlist"), requestRetrieveEilteFacade, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestAddFacade(string appChannelName, string applicationId, string sessionId, string requestCode, string numberParty, out string requestAddFacade)
        {
            requestAddFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\Add.xml"), appChannelName, applicationId, sessionId, requestCode, numberParty);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Add" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestAddFacade, headers);
        }

        public static HttpResponseMessage AnyDemandCodeFacade(string appChannelName, string applicationId, string demandCode, out string requestDemandCodeFacade, string sessionId)
        {
            requestDemandCodeFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\AcsGenericRequest\\AnyDemandCode.xml"), appChannelName, applicationId, demandCode, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AnyDemandCode" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/acsgenericrequest"), requestDemandCodeFacade, headers);
        }

        public static HttpResponseMessage RetrievePassengerBagFlightFacade(string applicationName, string applicationId, string location, string sessionId, out string retrieveBagRequestFacade)
        {
            retrieveBagRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Baggage\\RetrievePassengerBagFlight.xml"), applicationName, applicationId, location, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrievePassengerBagFlight" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/baggage"), retrieveBagRequestFacade, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestAutoAddFacade(string appChannelName, string applicationId, string pnr, string requestCode, out string requestAutoAddFacade)
        {
            requestAutoAddFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\AutoAdd.xml"), appChannelName, applicationId, pnr, requestCode);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AutoAdd" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestAutoAddFacade, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestAutoAddFacadePOPS(string applicationId, string pnr, string requestCode, out string requestAutoAddFacade)
        {
            requestAutoAddFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\AutoAddPOPS.xml"), applicationId, pnr, requestCode);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AutoAdd" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestAutoAddFacade, headers);
        }

        public static HttpResponseMessage DestroyTagBaggageFacade(string applicationName, string applicationId, string sessionId, string bagTagAirlineCode, string bagTagNumber, string city, out string destroyTagRequestFacade)
        {
            destroyTagRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Baggage\\DestroyTag.xml"), applicationName, applicationId, sessionId, bagTagAirlineCode, bagTagNumber, city);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "DestroyTag" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/baggage"), destroyTagRequestFacade, headers);
        }

        public static HttpResponseMessage SetupContextSsbdFacade(string applicationId, string pnr, string city, out string requestFacadeSsbd)
        {
            requestFacadeSsbd = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\SetupContextForSSBD.xml"), applicationId, pnr, city);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "SetupContext" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestFacadeSsbd, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestAutoDeleteFacade(string appChannelName, string applicationId, string pnr, string airlineCode, string recordNumber, out string requestAutoDeleteFacade)
        {
            requestAutoDeleteFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\AutoDelete.xml"), appChannelName, applicationId, pnr, airlineCode, recordNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AutoDelete" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestAutoDeleteFacade, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestAutoDeleteFacadePOPS(string applicationId, string pnr, out string requestAutoDeleteFacade)
        {
            requestAutoDeleteFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\AutoDeletePOPS.xml"), applicationId, pnr);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AutoDelete" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestAutoDeleteFacade, headers);
        }

        public static HttpResponseMessage PassengerAddPhoneFacade(string appChannelName, string appChannelId, string sessionId, string phoneNumber, out string addPhoneRequestFacade)
        {
            addPhoneRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\AddPhone.xml"), appChannelName, appChannelId, sessionId, phoneNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AddPhone" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), addPhoneRequestFacade, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestDeleteFacade(string appChannelName, string appChannelId, string sessionId, string requestCode, out string requestDeleteFacade)
        {
            requestDeleteFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\Delete.xml"), appChannelName, appChannelId, sessionId, requestCode);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Delete" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestDeleteFacade, headers);
        }

        public static HttpResponseMessage SpecialServiceRequestDeleteFacadePOPS(string appChannelId, string sessionId, string requestCode, out string requestDeleteFacade)
        {
            requestDeleteFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SpecialServiceRequest\\DeletePOPS.xml"), appChannelId, sessionId, requestCode);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Delete" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/specialservicerequest"), requestDeleteFacade, headers);
        }

        public static HttpResponseMessage RetrieveMapFacade(string appChannelName, string applicationId, string airlineCode, string departure, string arrival, string flightNumber, string date, out string retrieveMap)
        {
            retrieveMap = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SeatV7\\SeatV7.xml"), appChannelName, applicationId, airlineCode, departure, arrival, flightNumber, date);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveMap" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatv7"), retrieveMap, headers);
        }
        public static HttpResponseMessage RetrievePassengerMapFacade(string appChannelName, string applicationId, string airlineCode, string departure, string arrival, string flightNumber, string date, out string retrieveMap)
        {
            retrieveMap = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SeatV8\\RetrievePassengerMaps.xml"), appChannelName, applicationId, airlineCode, departure, arrival, flightNumber, date);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrievePassengerMap" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatv8"), retrieveMap, headers);
        }

        public static HttpResponseMessage SeatV8RetrievePassengerMap(string appChannelName, string applicationId, string recordLocator, out string retrievePassengerMapRequest)
        {
            retrievePassengerMapRequest = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SeatV8\\RetrievePassengerMap.xml"), appChannelName, applicationId, recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrievePassengerMap" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatv8"), retrievePassengerMapRequest, headers);
        }

        public static HttpResponseMessage PassengerAddFacade(string appChannelName, string applicationId, string sessionId, out string addRequestFacade)
        {
            addRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\Add.xml"), appChannelName, applicationId, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Add" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), addRequestFacade, headers);
        }

        public static HttpResponseMessage LoyaltyProfileSyncAddFacade(string appChannelName, string applicationId, string loyaltyNumber, out string requestLoyaltyAddFacade, bool isDigital = false)
        {
            requestLoyaltyAddFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\LoyaltyProfileSync\\Add.xml"), appChannelName, applicationId, loyaltyNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Add" }
           };

            return !isDigital ? WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltyprofilesync"), requestLoyaltyAddFacade, headers) :
             WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl("/digital/loyaltyprofilesync"), requestLoyaltyAddFacade, headers);
        }

        public static HttpResponseMessage LoyaltyProfileSyncUpdateFacade(string appChannelName, string applicationId, string loyaltyNumber, string addressLineOne, string addressLineTwo, string phoneNumber, string emailAddress, out string requestLoyaltyUpdateFacade)
        {
            requestLoyaltyUpdateFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\LoyaltyProfileSync\\Update.xml"), appChannelName, applicationId, loyaltyNumber, addressLineOne, addressLineTwo, phoneNumber, emailAddress);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Update" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltyprofilesync"), requestLoyaltyUpdateFacade, headers);
        }

        public static HttpResponseMessage RetrieveFlightCloseOutWeightBalance(string appChannelName, string applicationId, string flightBoardCity, string deptDate, string flightNumber, out string requestFlightCloseOutFacade)
        {
            requestFlightCloseOutFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\FlightOperationsV2\\RetrieveFlightCloseOutWeightBalance.xml"), appChannelName, applicationId, flightBoardCity, deptDate, flightNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveFlightCloseOutWeightBalance" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/flightoperations"), requestFlightCloseOutFacade, headers);
        }

        public static HttpResponseMessage PassengerManageMultipleInfantV2Facade(string appChannelName, string appChannelId, string sessionId, string passengerGivenName, string passengerSurname, string birthDay, string birthMonth, string birthYear, out string multipleInfantRequestFacade)
        {
            multipleInfantRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\ManageMultipleInfantV2.xml"), appChannelName, appChannelId, sessionId, passengerGivenName, passengerSurname, birthDay, birthMonth, birthYear);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "ManageMultipleInfantV2" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), multipleInfantRequestFacade, headers);
        }

        public static HttpResponseMessage AddSpecialRemarkFacade(string appChannelName, string applicatonId, string sessionId, string remarkFreeformText, out string requestAddFacade)
        {
            requestAddFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AddSpecialRemark.xml"), appChannelName, applicatonId, sessionId, remarkFreeformText);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "AddSpecialRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestAddFacade, headers);
        }

        public static HttpResponseMessage AddGenericRemarkFacade(string appChannelName, string applicatonId, string sessionId, string genericremarkFreeformText, out string requestAddfacade)
        {
            requestAddfacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AddGenericRemark.xml"), appChannelName, applicatonId, sessionId, genericremarkFreeformText);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "AddGenericRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestAddfacade, headers);
        }

        public static HttpResponseMessage SetupContextPnrCommonFacade(string appChannelNameSetupContext, string applicationIdSetupContext, string recordLocator)
        {
            var request = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\SetupContext.xml"), appChannelNameSetupContext, applicationIdSetupContext, recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "SetupContext" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), request, headers);
        }

        public static HttpResponseMessage PassengerDeleteFacade(string appChannelName, string applicationId, string sessionId, out string deleteRequestFacade)
        {
            deleteRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\Delete.xml"), appChannelName, applicationId, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Delete" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), deleteRequestFacade, headers);
        }

        public static HttpResponseMessage PassengerAddPTCCodesFacade(string appChannelName, string applicationId, string sessionId, string addPTCCode, out string addPTCCodeRequestFacade)
        {
            addPTCCodeRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\AddPTCCodes.xml"), appChannelName, applicationId, sessionId, addPTCCode);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AddPTCCodes" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), addPTCCodeRequestFacade, headers);
        }

        public static HttpResponseMessage PassengerDeletePTCCodesFacade(string appChannelName, string applicationId, string sessionId, out string deletePTCCodesRequestFacade)
        {
            deletePTCCodesRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\DeletePTCCodes.xml"), appChannelName, applicationId, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "DeletePTCCodes" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), deletePTCCodesRequestFacade, headers);
        }

        public static HttpResponseMessage PassengerModifyFacade(string appChannelName, string applicationId, string sessionId, string facadeCreateRloc, out string modifyPassengerRequestFacade)
        {
            modifyPassengerRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\Modify.xml"), appChannelName, applicationId, sessionId, facadeCreateRloc);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Modify" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), modifyPassengerRequestFacade, headers);
        }

        public static HttpResponseMessage TicketingProcessRetrieveArchived(string appChannelName, string applicationId, string ticketNumber, out string retrieveArchivedRequestFacade)
        {
            retrieveArchivedRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\RetrieveArchived.xml"), appChannelName, applicationId, ticketNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveArchived" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), retrieveArchivedRequestFacade, headers);
        }

        public static HttpResponseMessage PassengerManageTravelDocsAndSecureFlightFacade(string appChannelName, string applicationId, string sessionId, string passengerGivenName, string passengerSurName, out string manageTravelDocRequestFacade)
        {
            manageTravelDocRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\ManageTravelDocsAndSecureFlight.xml"), appChannelName, applicationId, sessionId, passengerGivenName, passengerSurName);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "ManageTravelDocsAndSecureFlight" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), manageTravelDocRequestFacade, headers);
        }

        public static HttpResponseMessage ManageMultipleTravelDocsAndSecureFlightFacade(string appChannelName, string applicationId, string sessionId, string passengerGivenName, string passengerSurName, string birthDay, string birthMonth, string birthYear, string gender, out string manageMultipleTravelDocRequestFacade)
        {
            manageMultipleTravelDocRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\ManageMultipleTravelDocsAndSecureFlight.xml"), appChannelName, applicationId, sessionId, passengerGivenName, passengerSurName, birthDay, birthMonth, birthYear, gender, passengerGivenName, passengerSurName);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "ManageMultipleTravelDocsAndSecureFlight" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), manageMultipleTravelDocRequestFacade, headers);
        }

        public static HttpResponseMessage AutoAddRemarkFacade(string appChannelName, string applicationId, string sessionId, string remarkText, string typeOfRemark, string airlineCode, out string requestAutoAddRemarkFacade)
        {
            requestAutoAddRemarkFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AutoAddRemark.xml"), appChannelName, applicationId, sessionId, remarkText, typeOfRemark, airlineCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "AutoAddRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestAutoAddRemarkFacade, headers);
        }

        public static HttpResponseMessage AddOsiRemarkFacade(string appChannelName, string applicationId, string sessionId, string remarkText, string airlineCode, out string requestAddOsiRemarkFacade)
        {
            requestAddOsiRemarkFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AddOSIRemark.xml"), appChannelName, applicationId, sessionId, remarkText, airlineCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "AddOSIRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestAddOsiRemarkFacade, headers);
        }

        public static HttpResponseMessage PassengerUpdateGroupSFPDInfoFacade(string appChannelName, string applicationId, string air4CreateRloc, out string updateGroupSFPDRequestFacade)
        {
            updateGroupSFPDRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\UpdateGroupSFPDInfo.xml"), appChannelName, applicationId, air4CreateRloc);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "UpdateGroupSFPDInfo" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), updateGroupSFPDRequestFacade, headers);
        }

        public static HttpResponseMessage PassengerUpdateSFPDFacade(string appChannelName, string applicationVersion, string transactionId, string loyaltyAccountNumber, out string updateSFPDRequestFacade)
        {
            updateSFPDRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\UpdateSFPD.xml"), appChannelName, applicationVersion, transactionId, loyaltyAccountNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "UpdateSecureFlightData" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), updateSFPDRequestFacade, headers);
        }

        public static HttpResponseMessage TicketingProcessRepriceFacade(string applicationId, string sessionId, out string repriceRequestFacade)
        {
            repriceRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\TicketingProcessV3\\Reprice.xml"), applicationId, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
           {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Reprice" }
           };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticketingprocess"), repriceRequestFacade, headers);
        }

        public static HttpResponseMessage ConvertMilesToCurrencyFacade(string appChannelName, string applicationId, string CountryCode, string CurrencyCode, string TotalFareAmount, out string requestloyalityFacade)
        {
            requestloyalityFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\LoyaltyMiles\\ConvertMilesToCurrency.xml"), appChannelName, applicationId, CountryCode, CurrencyCode, TotalFareAmount);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "ConvertMilesToCurrency" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltymiles"), requestloyalityFacade, headers);
        }

        public static HttpResponseMessage CurrencyConvertExecuteFacade(string applicationId, string CountryCode, string CurrencyCode, string TotalFareAmount, out string requestExecuteFacade)
        {
            requestExecuteFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\LoyaltyMiles\\CurrencyConvertExecute.xml"), applicationId, CountryCode, CurrencyCode, TotalFareAmount);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Execute" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/currencyconvert"), requestExecuteFacade, headers);
        }

        public static HttpResponseMessage DeleteOsiRemarkFacade(string appChannelName, string applicationId, string sessionId, string recordNumber, string airlineCode, out string requestDeleteOsiRemarkFacade)
        {
            requestDeleteOsiRemarkFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\DeleteOSIRemark.xml"), appChannelName, applicationId, sessionId, recordNumber, airlineCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "DeleteOSIRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestDeleteOsiRemarkFacade, headers);
        }

        public static HttpResponseMessage AutoModifyRemarkFacade(string appChannelName, string applicationId, string sessionId, string modifyRemarkText, string typeOfRemark, string remarkInfoRecNo, out string requestAutoModifyRemarkFacade)
        {
            requestAutoModifyRemarkFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AutoModifyRemark.xml"), appChannelName, applicationId, sessionId, modifyRemarkText, typeOfRemark, remarkInfoRecNo);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "AutoModifyRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestAutoModifyRemarkFacade, headers);
        }

        public static HttpResponseMessage ModifyStatusSegmentFacade(string appChannelName, string applicationId, string facadeSessionId, string segmentNumber, string status, out string requestModifyStatusFacade)
        {
            requestModifyStatusFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\ModifyStatus.xml"), appChannelName, applicationId, facadeSessionId, segmentNumber, status);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "ModifyStatus" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/segment"), requestModifyStatusFacade, headers);
        }

        public static HttpResponseMessage SeatV8AddFacade(string appChannelName, string applicationId, string offerId, string offerItemNumber, string seatNumber, string chargeAmount, out string addRequestFacade)
        {
            addRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SeatV8\\Add.xml"), appChannelName, applicationId, offerId, offerItemNumber, seatNumber, chargeAmount);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Add" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatv8"), addRequestFacade, headers);
        }

        public static HttpResponseMessage DeleteGenericRemarkFacade(string appChannelName, string applicationId, string sessionId, string remarkInfoRecNo, out string requestDeleteGenericRemarkFacade)
        {
            requestDeleteGenericRemarkFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\DeleteGenericRemark.xml"), appChannelName, applicationId, sessionId, remarkInfoRecNo);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "DeleteGenericRemark" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/remark"), requestDeleteGenericRemarkFacade, headers);
        }

        public static HttpResponseMessage AddPnrQueueFacade(string appChannelName, string applicationId, string sessionId, string queueNumber, out string addPnrRequestFacade)
        {
            addPnrRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\QueueV2\\AddPnrQueue.xml"), appChannelName, applicationId, sessionId, queueNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AddPnrQueue" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/queue"), addPnrRequestFacade, headers);
        }

        public static HttpResponseMessage FaresV5BookPriceStoreFareFacade(string appChannelName, string applicationId, string sessionId, string requestType, out string bookPriceStoreFareRequestFacade)
        {
            bookPriceStoreFareRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\FaresV5\\BookPriceStoreFare.xml"), appChannelName, applicationId, sessionId, requestType);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "BookPriceStoreFare" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/fares"), bookPriceStoreFareRequestFacade, headers);
        }

        public static HttpResponseMessage BookingProcessFacade(string appChannelName, string applicationId, string arrivalPoint, string classOfService, string departureDate, string departurePoint, string flightNumber, string returnDepartureDate, out string requestBookingProcessCreateGroupFacade)
        {
            requestBookingProcessCreateGroupFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\BookingprocessV3\\CreateGroup.xml"), appChannelName, applicationId, arrivalPoint, classOfService, departureDate, departurePoint, flightNumber, returnDepartureDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "CreateGroup" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/bookingprocess"), requestBookingProcessCreateGroupFacade, headers);
        }

        public static HttpResponseMessage CancelItineraryFacade(string appChannelName, string applicationId, string facadeSessionId, out string requestCancelItineraryFacade)
        {
            requestCancelItineraryFacade = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\ItineraryV5\\CancelItinerary.xml"), appChannelName, applicationId, facadeSessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "CancelItinerary" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/Itinerary"), requestCancelItineraryFacade, headers);
        }

        public static HttpResponseMessage BookingProcessPerformRevenueFacade(string airlineCode, string arrivalPoint, string arrivalTime, string classOfService, string depatureDate, string departurePoint, string departureTime, string flightNumber, string appChannelName, string applicationId, string returnDepartureDate, out string performRevenueRequestFacade)
        {
            performRevenueRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\BookingprocessV3\\PerformRevenue.xml"), airlineCode, arrivalPoint, arrivalTime, classOfService, depatureDate, departurePoint, departureTime, flightNumber, appChannelName, applicationId, returnDepartureDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "PerformRevenue" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/bookingprocess"), performRevenueRequestFacade, headers);
        }

        public static HttpResponseMessage AddPnrPprPassengerFacade(string appChannelName, string applicationId, string sessionId, string employeePprID, out string addPnrPprPassengerRequestFacade)
        {
            addPnrPprPassengerRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PrimaryPassRiderV3\\AddPnrPprPassenger.xml"), appChannelName, applicationId, sessionId, employeePprID);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "AddPnrPprPassenger" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/primarypassrider"), addPnrPprPassengerRequestFacade, headers);
        }

        public static HttpResponseMessage RetrievePnrQueueFacade(string appChannelName, string applicationId, string sessionId, string queueNumber, out string retrievePnrRequestFacade)
        {
            retrievePnrRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\QueueV2\\RetrievePnrQueue.xml"), appChannelName, applicationId, sessionId, queueNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrievePnrQueue" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/queue"), retrievePnrRequestFacade, headers);
        }

        public static HttpResponseMessage CustomerNotificationCleanPnrFacade(string applicationId, string recordLocator, out string cleanPnrRequestFacade)
        {
            cleanPnrRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\CustomerNotifications\\CleanPnr.xml"), applicationId, recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "CleanPnr" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customernotification"), cleanPnrRequestFacade, headers);
        }

        public static HttpResponseMessage LookUpArrayOfTokensFacade(string appChannelName, string NetworkAddress, string SysCode, out string reqLookUpArrayOfTokensFacade)
        {
            reqLookUpArrayOfTokensFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PCITokenManager\\LookUpArrayOfTokens.xml"), appChannelName, NetworkAddress, SysCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "LookUpArrayOfTokens" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pcitokenmanager"), reqLookUpArrayOfTokensFacade, headers);
        }

        public static HttpResponseMessage PassengerRetrieveTimaticInfoFacade(string applicationId, string recordLocator, string customerId, string pnrCreationDate, string firstName, string lastName, out string retrieveTimaticInfoRequestFacade)
        {
            retrieveTimaticInfoRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PassengerV4\\RetrieveTimaticInfo.xml"), applicationId, recordLocator, customerId, pnrCreationDate, firstName, lastName);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveTimaticInfo" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/passenger"), retrieveTimaticInfoRequestFacade, headers);
        }

        public static HttpResponseMessage RetrieveFlightAvailabilityFacade(string appChannelName, string applicationId, string flightNumber, string flightOriginDate, out string requestRetrieveFlightAvailabilityFacade)
        {
            requestRetrieveFlightAvailabilityFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Inventory\\RetrieveFlightAvailability.xml"), appChannelName, applicationId, flightNumber, flightOriginDate);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveFlightAvailability" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/inventory"), requestRetrieveFlightAvailabilityFacade, headers);
        }

        public static HttpResponseMessage ResAnyDemandCodeFacade(string appChannelName, string applicationId, string sessionId, string resDemandCode, out string requestResDemandCodeFacade)
        {
            requestResDemandCodeFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\RESGenericRequest\\AnyDemandCode.xml"), appChannelName, applicationId, sessionId, resDemandCode);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AnyDemandCode" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/resgeneric"), requestResDemandCodeFacade, headers);
        }

        public static HttpResponseMessage CreditCardRetrieveFacade(string appChannelName, string applicationId, string air4SessionId, out string requestCreditCardRetrieveFacade)
        {
            requestCreditCardRetrieveFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\CreditCardV2\\Retrieve.xml"), appChannelName, applicationId, air4SessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RetrieveEnhancedCreditCardAcceptance" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/creditcard"), requestCreditCardRetrieveFacade, headers);
        }

        public static HttpResponseMessage ToolsExecuteSoldSeatRetrieveFacade(string appChannelName, string applicationId, string facadeSessionId, string flightNumber, string segDepartureDate, out string requestExecuteSoldSeatFacade)
        {
            requestExecuteSoldSeatFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Tools\\SeatsSoldCountbyClassRetrieve.xml"), appChannelName, applicationId, facadeSessionId, flightNumber, segDepartureDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Execute" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatsoldcount"), requestExecuteSoldSeatFacade, headers);
        }

        public static HttpResponseMessage PaymentOrderCreateFacade(string applicationId, string paymentReferenceId, string amount, string currencyCode, string TransactionSequenceNumber, out string createRequestFacade)
        {
            createRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\PaymentOrder\\Create.xml"), applicationId, paymentReferenceId, amount, currencyCode, TransactionSequenceNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Create" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/paymentorder"), createRequestFacade, headers);
        }

        public static HttpResponseMessage RemovePnrQueueFacade(string appChannelName, string applicationId, string sessionId, out string removePnrRequestFacade)
        {
            removePnrRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\QueueV2\\RemovePnrQueue.xml"), appChannelName, applicationId, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "RemovePnrQueue" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/queue"), removePnrRequestFacade, headers);
        }

        public static HttpResponseMessage ToolsExecuteGroupPaxCountRetrieveFacade(string appChannelName, string applicationId, string facadeSessionId, string flightNumber, string segDepartureDate, out string requestExecuteGroupPaxCountFacade)
        {
            requestExecuteGroupPaxCountFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\Tools\\GroupPassengersCountbyClassRetrieve.xml"), appChannelName, applicationId, facadeSessionId, flightNumber, segDepartureDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Execute" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/grouppassengerscount"), requestExecuteGroupPaxCountFacade, headers);
        }

        public static HttpResponseMessage AutoInsertSegmentFacade(string appChannelName, string applicationId, string facadeSessionId, string facadePnr, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, out string reqAutoInsertSegmentFacade)
        {
            reqAutoInsertSegmentFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SegmentV4\\AutoInsert.xml"), appChannelName, applicationId, facadeSessionId, facadePnr, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AutoInsert" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/segment"), reqAutoInsertSegmentFacade, headers);
        }

        public static HttpResponseMessage SegmentCancelAndRebookFacade(string appChannelName, string applicationId, string sessionId, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, string returnDate, string cancelTripDate, string cancelReturnDate, out string cancelAndRebookRequestFacade)
        {
            cancelAndRebookRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SegmentV4\\CancelAndRebook.xml"), appChannelName, applicationId, sessionId, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber, returnDate, cancelTripDate, cancelReturnDate);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "CancelAndRebook" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/segment"), cancelAndRebookRequestFacade, headers);
        }

        public static HttpResponseMessage SegmentAutoModifyStatusFacade(string appChannelName, string applicationId, string recordLocator, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, string status, out string autoModifyStatusFacadeRequest)
        {
            autoModifyStatusFacadeRequest = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SegmentV4\\AutoModifyStatus.xml"), appChannelName, applicationId, recordLocator, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber, status);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "AutoModifyStatus" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/segment"), autoModifyStatusFacadeRequest, headers);
        }

        public static HttpResponseMessage SeatV8DeleteFacade(string appChannelName, string applicationId, string recordLocator, string orderId, string arrivalAirportCd, string classOfService, string departureAirportCd, string departureDate, out string deleteFacadeRequest)
        {
            deleteFacadeRequest = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SeatV8\\Delete.xml"), appChannelName, applicationId, recordLocator, orderId, arrivalAirportCd, classOfService, departureAirportCd, departureDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Delete" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatv8"), deleteFacadeRequest, headers);
        }

        public static HttpResponseMessage SeatV8ModifyFacade(string appChannelName, string applicationId, string offerId, string offerItemNumber, string seatNumber, string chargeAmount, string amountDecimalPlacesout, out string modifyRequestFacade)
        {
            modifyRequestFacade = string.Format(File.ReadAllText("requests\\Body\\AIR4\\SeatV8\\Modify.xml"), appChannelName, applicationId, offerId, offerItemNumber, seatNumber, chargeAmount, amountDecimalPlacesout);

            var headers = new System.Collections.Specialized.NameValueCollection
             {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Modify" }
             };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/seatv8"), modifyRequestFacade, headers);
        }

        public static HttpResponseMessage IgnoreFacade(string appChannelName, string applicationId, string sessionId, out string ignoreRequestFacade)
        {
            ignoreRequestFacade = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\Ignore.xml"), appChannelName, applicationId, sessionId);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Ignore" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), ignoreRequestFacade, headers);
        }

        public static HttpResponseMessage SetupContextFacadeNDC(string recordLocator, out string requestFacadeNdc)
        {
            requestFacadeNdc = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\SetupContextForNDC.xml"), recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "SetupContext" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestFacadeNdc, headers);
        }

        public static HttpResponseMessage CreatePnrFacadeNDC(string appChannelName, string applicationId, string arrivalPoint, string arrivalTime, string classOfService, string departureDate, string departurePoint, string departureTime, string flightNumber, out string requestCreatePnrNdcFacade)

        {
            requestCreatePnrNdcFacade = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\CreatePnrForNDC.xml"), appChannelName, applicationId, arrivalPoint, arrivalTime, classOfService, departureDate, departurePoint, departureTime, flightNumber);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                 { "Content-Type", "text/xml" },
                 { "SOAPAction", "Create" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), requestCreatePnrNdcFacade, headers);
        }

        public static HttpResponseMessage Decode(string applicationId, string recordLocator, out string decodeFacadeRequest)
        {
            decodeFacadeRequest = string.Format(File.ReadAllText("requests\\Body\\Air4\\PNRCommonV6\\Decode.xml"), applicationId, recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Decode" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/pnrcommon"), decodeFacadeRequest, headers);
        }

        public static HttpResponseMessage Refund(string appChannelName, string applicationId, string sessionId, string ticketnumber, out string refundFacadeRequest)
        {
            refundFacadeRequest = string.Format(File.ReadAllText("requests\\Body\\Air4\\TicketV3\\Refund.xml"), appChannelName, applicationId, sessionId, ticketnumber);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Refund" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticket"), refundFacadeRequest, headers);
        }

        public static HttpResponseMessage Arefund(string appChannelName, string applicationId, string sessionId, string ticketnumber, string pnrcreationDate, out string refundFacadeRequest)
        {
            refundFacadeRequest = string.Format(File.ReadAllText("requests\\Body\\Air4\\TicketV3\\ActualRefund.xml"), appChannelName, applicationId, sessionId, ticketnumber, pnrcreationDate);

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "Refund" }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/ticket"), refundFacadeRequest, headers);
        }
    }
}