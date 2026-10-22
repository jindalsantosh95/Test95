using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Delta.Endpoint
{
    public static class Soa
    {
        public static HttpResponseMessage PnrCommonV6Retrieve(string rloc, X509Certificate2 x509Certificate)
        {
            string requestPath = Path.Combine("Requests", "Body", "SOA", "PnrCommonrRetrieveV6.xml");
            var request = string.Format(File.ReadAllText(requestPath), rloc);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Content-Type", "text/xml");

            return WebRequest.Xml.PostSoap(Config.Soa.GetUrl("/pnrcommon/v6"), request, headers);
        }

        public static HttpResponseMessage PnrCommonV6SetupContext(string rloc, X509Certificate2 x509Certificate)
        {
            string requestPath = Path.Combine("Requests", "Body", "SOA", "PnrCommonSetupContext.xml");
            var request = string.Format(File.ReadAllText(requestPath), rloc);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Content-Type", "text/xml");

            return WebRequest.Xml.PostSoap(Config.Soa.GetUrl($"/pnrcommon/v6"), request, headers);
        }

        public static HttpResponseMessage PassengerV4MultipleTravelDocsAndSecureFlight(string sessionId, string firstName, string lastName, string passengerNameNumber, X509Certificate2 x509Certificate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\SOA\\PassengerV4ManageMultipleTravelDocs.xml"), sessionId, firstName, lastName, passengerNameNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Content-Type", "text/xml");

            return WebRequest.Xml.PostSoap(Config.Soa.GetUrl($"/passenger/v4"), request, headers);
        }
        public static HttpResponseMessage TicketRetrieve(string rloc, X509Certificate2 x509Certificate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\SOA\\TicketRetrive.xml"), rloc);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Content-Type", "text/xml");

            return WebRequest.Xml.PostSoap(Config.Soa.GetUrl($"/ticketprocess/v6"), request, headers);
        }

        //Need to look at the key vault and understand what headers are required along with passing through the key value
        public static HttpResponseMessage BookingProcessV3_PerformRevenue()
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", string.Empty);

            return WebRequest.Xml.PostSoap(Config.Soa.GetUrl($"/bookingprocess/v3"), string.Empty, headers);
        }

        public static HttpResponseMessage Brand(string bearer)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Json.Post(Config.SoaBrand.GetUrl($"/rm/shopping/v1/brands/products/segments/cos"), Factory.Soa.SOABrand(), headers);
        }

        public static HttpResponseMessage RetrieveSpecificAvailabilityV2(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string airlineCode, string classOfService, string destinationStation, string flightNumber, string originStation, string tripDate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\ScheduleAvailbility\\RetrieveSpecificAvailability.xml"), appChannelName, applicationId, airlineCode, classOfService, destinationStation, flightNumber, originStation, tripDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/schedulesavailability/v2"), request);
        }

        public static HttpResponseMessage RetrieveBetweenCitiesV2(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string airlineCode, string destinationStation, string originStation, string tripDate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\ScheduleAvailbility\\RetrieveBetweenCities.xml"), appChannelName, applicationId, airlineCode, destinationStation, originStation, tripDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/schedulesavailability/v2"), request);

        }

        public static HttpResponseMessage SeatV8(X509Certificate2 x509Certificate, string arrivalAirportCode, string classOfService, string departureAirportCode, string departureDate,
            string marketingCarrierCode, string marketingFlightNumber, string OperatingCarrierCode, string operatingFlightNumber, string segmentNumber, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, string flightDepartureTime, string currencyCode)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\SOA\\SeatV8.xml"), arrivalAirportCode, classOfService, departureAirportCode, departureDate,
                marketingCarrierCode, marketingFlightNumber, OperatingCarrierCode, operatingFlightNumber, segmentNumber, flightDepartureTime, currencyCode);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "RetrievePassengerMap");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            var node = xmlDocument.SelectSingleNode("//v1:RetrievePassengerMapRequest", xmlDocument.CreateXmlNamespaceManager());

            for (int i = 0; i < numberOfadults; i++)
            {
                XmlElement travelerInfo = xmlDocument.CreateElement("v11", "TravelerInfo", "http://schemas.delta.com/common/ancillaryproductcommontypes/v1");

                travelerInfo.SetAttribute("AssociationId", "");
                travelerInfo.SetAttribute("PaxType", "ADT");

                node.AppendChild(travelerInfo);
            }

            for (int i = 0; i < numberOfyouths; i++)
            {
                XmlElement travelerInfo = xmlDocument.CreateElement("v11", "TravelerInfo", "http://schemas.delta.com/common/ancillaryproductcommontypes/v1");

                travelerInfo.SetAttribute("AssociationId", "");
                travelerInfo.SetAttribute("PaxType", "GBE");

                node.AppendChild(travelerInfo);
            }

            for (int i = 0; i < numberOfchildren; i++)
            {
                XmlElement travelerInfo = xmlDocument.CreateElement("v11", "TravelerInfo", "http://schemas.delta.com/common/ancillaryproductcommontypes/v1");

                travelerInfo.SetAttribute("AssociationId", "");
                travelerInfo.SetAttribute("PaxType", "CHD");

                node.AppendChild(travelerInfo);
            }

            for (int i = 0; i < numberOfinfants; i++)
            {
                XmlElement travelerInfo = xmlDocument.CreateElement("v11", "TravelerInfo", "http://schemas.delta.com/common/ancillaryproductcommontypes/v1");

                travelerInfo.SetAttribute("AssociationId", "");
                travelerInfo.SetAttribute("PaxType", "INF");

                node.AppendChild(travelerInfo);
            }

            var travelerInfoNodes = xmlDocument.SelectNodes("//v11:TravelerInfo", xmlDocument.CreateXmlNamespaceManager());

            var id = 1;

            foreach (XmlNode item in travelerInfoNodes)
                item.Attributes["AssociationId"].Value = (id++).ToString("D2");

            xmlDocument.SignSoapXmlLegacy(x509Certificate);

            return WebRequest.Xml.PostSoap(Config.SoaSeatV8.GetUrl("/seat/v8"), xmlDocument.OuterXml, headers);
        }

        public static HttpResponseMessage CreateContext(X509Certificate2 x509Certificate, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\Create.xml"), appChannelName, applicationId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "create" }
            };

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/hostcontext/v2"), request, headers);
        }

        public static HttpResponseMessage SwingCityDutyCodeVerify(X509Certificate2 x509Certificate, string sessionId, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\SwingCityDutyCode.xml"), sessionId, appChannelName, applicationId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "SwingCityDutyCode" }
            };

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/hostcontext/v2"), request, headers);
        }

        public static HttpResponseMessage DestroyContext(X509Certificate2 x509Certificate, string sessionId, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\Destroy.xml"), sessionId, appChannelName, applicationId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "destroy" }
            };

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/hostcontext/v2"), request, headers);
        }

        public static HttpResponseMessage CreateAndSwing(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string cityCode)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\HostContextV2\\CreateAndSwing.xml"), appChannelName, applicationId, cityCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "CreateAndSwing" }
            };

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/hostcontext/v2"), request, headers);
        }

        public static HttpResponseMessage RetrievePnrCommon(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string recordLocator, string stampingInfoFlag = "")
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\RetrievePnr.xml"), appChannelName, applicationId, recordLocator);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            //For other AppChannels scenario except NDC , removing the StampingInfo node from request

            if (!string.IsNullOrEmpty(stampingInfoFlag))
            {
                XmlNamespaceManager retrievePnrRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var stampingInfoNode = xmlDocument?.SelectSingleNode("//stamping:StampingInfo", retrievePnrRequestManager);

                stampingInfoNode?.ParentNode?.RemoveChild(stampingInfoNode);
            }

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);
        }

        public static HttpResponseMessage SetupContextPnrCommon(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string recordLocator)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\SetupContext.xml"), appChannelName, applicationId, recordLocator);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);

        }

        public static HttpResponseMessage PnrListRetrieve(X509Certificate2 x509Certificate, string departureAirportCd, string flightnum, string depDate, string appChannelName, string applicationId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PnrList\\PnrListRetrieve.xml"), departureAirportCd, flightnum, depDate, appChannelName, applicationId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrlist"), request);
        }

        public static HttpResponseMessage SetupContextForSSBD(X509Certificate2 x509Certificate, string applicationId, string pnr, string city)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\SetupContextForSSBD.xml"), applicationId, pnr, city);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);

        }

        public static HttpResponseMessage CheckInBaggageAddAndPrintIP(X509Certificate2 x509Certificate, string applicationName, string applicationId, string sessionId, string actionCode)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Checkin-Baggage-AcceptAndPrint\\Execute_IP.xml"), applicationName, applicationId, sessionId, actionCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("31521/checkinbaggageacceptandprint"), request);

        }

        public static HttpResponseMessage CheckInBaggageAddAndPrintUP(X509Certificate2 x509Certificate, string applicationName, string applicationId, string sessionId, string actionCode)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Checkin-Baggage-AcceptAndPrint\\Execute_UP.xml"), applicationName, applicationId, sessionId, actionCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/checkinbaggageacceptandprint"), request);

        }

        public static HttpResponseMessage CheckInBaggageAddAndPrintAP(X509Certificate2 x509Certificate, string applicationName, string applicationId, string sessionId, string actionCode)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Checkin-Baggage-AcceptAndPrint\\Execute_AP.xml"), applicationName, applicationId, sessionId, actionCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/checkinbaggageacceptandprint"), request);

        }

        public static HttpResponseMessage RetrievePassengerBagFlight(X509Certificate2 x509Certificate, string applicationName, string applicationId, string location, string sessionId, out string retrieveBagRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Baggage\\RetrievePassengerBagFlight.xml"), applicationName, applicationId, location, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            retrieveBagRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.BaggageForZuluEnvt.GetUrl("30516/baggage"), retrieveBagRequest);
        }

        public static HttpResponseMessage PnrListRetrieveWithSsr(X509Certificate2 x509Certificate, string arrivalAirport, string departureAirport, string departureDate, string flightNumber, string appChannelName, string applicationId, string ssrTypeCode)

        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PnrList\\PnrListRetrieveWithSsr.xml"), arrivalAirport, departureAirport, departureDate, flightNumber, appChannelName, applicationId, ssrTypeCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrlist"), request);
        }

        public static HttpResponseMessage PnrListRetrieveWithSeatStatus(X509Certificate2 x509Certificate, string flightLegArrivalAirportCd, string flightLegArrivalDate, string flightLegDepartureAirportCd, string flightLegDepartureDate, string flightNumber, string applicationName)

        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PnrList\\PnrListRetrieveWithSeatStatus.xml"), flightLegArrivalAirportCd, flightLegArrivalDate, flightLegDepartureAirportCd, flightLegDepartureDate, flightNumber, applicationName);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrlist"), request);
        }

        public static HttpResponseMessage DestroyTagBaggage(X509Certificate2 x509Certificate, string applicationName, string applicationId, string sessionId, string bagTagAirlineCode, string bagTagNumber, string city, out string destroyTagRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Baggage\\DestroyTag.xml"), applicationName, applicationId, sessionId, bagTagAirlineCode, bagTagNumber, city);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            destroyTagRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.BaggageForZuluEnvt.GetUrl("30516/baggage"), destroyTagRequest);
        }

        public static HttpResponseMessage CreatePnrAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string arrivalPoint, string arrivalTime, string classOfService, string departureDate, string departurePoint, string departureTime, string flightNumber)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\CreatePnr.xml"), appChannelName, applicationId, arrivalPoint, arrivalTime, classOfService, departureDate, departurePoint, departureTime, flightNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);
        }

        public static HttpResponseMessage TearDownContext(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string teardownRequest, string requestFlag = "", string stampingInfoFlag = "")
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

            //For other AppChannels scenario except NDC , removing the StampingInfo node from request
            if (!string.IsNullOrEmpty(stampingInfoFlag))
            {
                XmlNamespaceManager retrievePnrRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var stampingInfoNode = xmlDocument?.SelectSingleNode("//stamping:StampingInfo", retrievePnrRequestManager);

                stampingInfoNode?.ParentNode?.RemoveChild(stampingInfoNode);
            }

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            teardownRequest = XDocument.Parse(request).ToString();

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "TeardownContext" }
            };

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request, headers);
        }

        public static HttpResponseMessage DividePnrCommon(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string stampingInfoFlag = "")
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\DividePnr.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            //For other AppChannel scenario except NDC , removing the StampingInfo node from request

            if (!string.IsNullOrEmpty(stampingInfoFlag))
            {
                XmlNamespaceManager dividePnrRequestManager = xmlDocument.CreateXmlNamespaceManager();

                var stampingInfoNode = xmlDocument?.SelectSingleNode("//stamping:StampingInfo", dividePnrRequestManager);

                stampingInfoNode?.ParentNode?.RemoveChild(stampingInfoNode);
            }

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);
        }

        public static HttpResponseMessage RefreshPnrCommon(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\RefreshPnr.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);
        }

        public static HttpResponseMessage EndPnrCommon(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string airline, string redisplayFlag)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\EndPnr.xml"), appChannelName, applicationId, sessionId, airline, redisplayFlag);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), request);
        }

        public static HttpResponseMessage TicketingProcessEmailReceipt(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string emailId, out string emailReceiptRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\EmailReceipt.xml"), appChannelName, applicationId, sessionId, emailId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            emailReceiptRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), emailReceiptRequest);
        }

        public static HttpResponseMessage TicketingProcessMaintainTicketingInstructionsForTours(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string toursCode, out string ticketingForToursRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\Tours.xml"), appChannelName, applicationId, sessionId, toursCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            ticketingForToursRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), ticketingForToursRequest);
        }

        public static HttpResponseMessage TicketingProcessModifyTicketingTimeLimit(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string ticketingTimeLimitInfo, string addTicketingInfoIndicator, out string modifyTicketingTimeLimitRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\ModifyTicketing.xml"), appChannelName, applicationId, sessionId, ticketingTimeLimitInfo, addTicketingInfoIndicator);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            modifyTicketingTimeLimitRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), modifyTicketingTimeLimitRequest);
        }

        public static HttpResponseMessage InsertSegment(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, string numberParty, string status)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\Insert.xml"), appChannelName, applicationId, sessionId, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber, numberParty, status);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/segment/v4"), request);
        }

        public static HttpResponseMessage DeleteSegment(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string segmentNumber)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\Delete.xml"), appChannelName, applicationId, sessionId, segmentNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/segment/v4"), request);
        }

        public static HttpResponseMessage AutoInsertSegment(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string air4Pnr, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, out string reqAutoInsertSegmentAir4)
        {
            reqAutoInsertSegmentAir4 = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\AutoInsert.xml"), appChannelName, applicationId, sessionId, air4Pnr, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(reqAutoInsertSegmentAir4);

            xmlDocument.SignSoapXml(x509Certificate);

            reqAutoInsertSegmentAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/segment/v4"), reqAutoInsertSegmentAir4);
        }

        public static HttpResponseMessage PnrListRetrieveElitePassengers(X509Certificate2 x509Certificate, string applicationName, string applicationId, string flightLegArrivalAirportCd, string flightLegDepartureAirportCd, string flightLegDepartureDate, string flightNumber, out string requestRetrieveEilteAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PnrList\\RetrieveElitePassengers.xml"), applicationName, applicationId, flightLegArrivalAirportCd, flightLegDepartureAirportCd, flightLegDepartureDate, flightNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestRetrieveEilteAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(ConfigReader.Get("delta_acceptance_retriveElite"), requestRetrieveEilteAir4);
        }

        public static HttpResponseMessage SpecialServiceRequestAdd(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string requestCode, string numberParty, out string requestAdd)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SpecialServiceRequest\\Add.xml"), appChannelName, applicationId, sessionId, requestCode, numberParty);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAdd = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/specialservicerequest/v4"), requestAdd);
        }

        public static HttpResponseMessage AnyDemandCode(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string demandCode, out string requestDemandCode, string sessionId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\ACSGenericRequest\\AnyDemandCode.xml"), appChannelName, applicationId, demandCode, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestDemandCode = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/acsgenericrequest"), requestDemandCode);
        }

        public static HttpResponseMessage SpecialServiceRequestAutoAdd(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string pnr, string requestCode, out string requestAutoAdd)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SpecialServiceRequest\\AutoAdd.xml"), appChannelName, applicationId, pnr, requestCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAutoAdd = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/specialservicerequest/v4"), requestAutoAdd);
        }
        public static HttpResponseMessage BookingProcessAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string arrivalPoint, string classOfService, string departureDate, string departurePoint, string flightNumber, string returnDepartureDate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\BookingprocessV3\\CreateGroup.xml"), appChannelName, applicationId, arrivalPoint, classOfService, departureDate, departurePoint, flightNumber, returnDepartureDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/bookingprocess/v3"), request);
        }

        public static HttpResponseMessage BookingProcessPerformRevenue(X509Certificate2 x509Certificate, string airlineCode, string arrivalPoint, string arrivalTime, string classOfService, string depatureDate, string departurePoint, string departureTime, string flightNumber, string appChannelName, string applicationId, string returnDepartureDate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\BookingprocessV3\\PerformRevenue.xml"), airlineCode, arrivalPoint, arrivalTime, classOfService, depatureDate, departurePoint, departureTime, flightNumber, appChannelName, applicationId, returnDepartureDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/bookingprocess/v3"), request);
        }

        public static HttpResponseMessage TickedPnrAIR4(X509Certificate2 x509Certificate, string sessionId)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\TicketedPnr.xml"), sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/bookingprocess/v3"), request);
        }

        public static HttpResponseMessage TicketingProcessRetrieve(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string electronicDocumentNumber, out string retrieveRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\Retrieve.xml"), appChannelName, applicationId, electronicDocumentNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            retrieveRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), retrieveRequest);
        }

        public static HttpResponseMessage SpecialServiceRequestAutoDelete(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string pnr, string airlineCode, string recordNumber, out string requestAutoDelete)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SpecialServiceRequest\\AutoDelete.xml"), appChannelName, applicationId, pnr, airlineCode, recordNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAutoDelete = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/specialservicerequest/v4"), requestAutoDelete);
        }

        public static HttpResponseMessage PassengerAddPhone(X509Certificate2 x509Certificate, string appChannelName, string appChannelId, string sessionId, string phoneNumber, out string addPhoneRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\AddPhone.xml"), appChannelName, appChannelId, sessionId, phoneNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            addPhoneRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), addPhoneRequest);
        }

        public static HttpResponseMessage SpecialServiceRequestDelete(X509Certificate2 x509Certificate, string appChannelName, string appChannelId, string sessionId, string requestCode, out string requestDelete)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SpecialServiceRequest\\Delete.xml"), appChannelName, appChannelId, sessionId, requestCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestDelete = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/specialservicerequest/v4"), requestDelete);
        }

        public static HttpResponseMessage AddPassengerAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string addRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\Add.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            addRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), addRequest);
        }

        public static HttpResponseMessage PassengerDelete(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string deleteRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\Delete.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            deleteRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), deleteRequest);
        }

        public static HttpResponseMessage PassengerAddPTCCodes(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string addPTCCode, out string addPTCCodesRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\AddPTCCodes.xml"), appChannelName, applicationId, sessionId, addPTCCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            addPTCCodesRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), addPTCCodesRequest);
        }

        public static HttpResponseMessage PassengerModify(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string air4CreateRloc, out string modifyRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\Modify.xml"), appChannelName, applicationId, sessionId, air4CreateRloc);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            modifyRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), modifyRequest);
        }

        public static HttpResponseMessage PassengerDeletePTCCodes(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string deletePTCCodesRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\DeletePTCCodes.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            deletePTCCodesRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), deletePTCCodesRequest);
        }

        public static HttpResponseMessage Retrievemap(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string airlineCode, string departure, string arrival, string flightNumber, string date)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SeatV7\\SeatV7.xml"), appChannelName, applicationId, airlineCode, departure, arrival, flightNumber, date);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/seat/v7"), request);
        }

        public static HttpResponseMessage LoyaltyProfileSyncAdd(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string LoyaltyNumber, out string requestLoyaltyAdd)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\LoyaltyProfileSync\\Add.xml"), appChannelName, applicationId, LoyaltyNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestLoyaltyAdd = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/loyaltyprofilesync"), requestLoyaltyAdd);
        }

        public static HttpResponseMessage LoyaltyProfileSyncUpdate(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string LoyaltyNumber, string addressLineOne, string addressLineTwo, string phoneNumber, string emailAddress, out string requestLoyaltyUpdate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\LoyaltyProfileSync\\Update.xml"), appChannelName, applicationId, LoyaltyNumber, addressLineOne, addressLineTwo, phoneNumber, emailAddress);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestLoyaltyUpdate = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/loyaltyprofilesync"), requestLoyaltyUpdate);
        }

        public static HttpResponseMessage RetrieveFlightCloseOutWeightBalance(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string flightBoardCity, string deptDate, string flightNumber, out string requestFlightCloseOut)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\FlightOperationsV2\\RetrieveFlightCloseOutWeightBalance.xml"), appChannelName, applicationId, flightBoardCity, deptDate, flightNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestFlightCloseOut = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("33527/flightoperations/v2"), requestFlightCloseOut);
        }

        public static HttpResponseMessage TicketingProcessPerformRevenue(X509Certificate2 x509Certificate, string applicationId, string sessionId, string cvccIdNumber, string creditCardExpiration, string creditCardNumber, string creditCardType, string currencyCode, string customerFirstName, string customerLastName, string paymentAmount, out string performRevenueRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\PerformRevenue.xml"), applicationId, sessionId, cvccIdNumber, creditCardExpiration, creditCardNumber, creditCardType, currencyCode, customerFirstName, customerLastName, paymentAmount);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            performRevenueRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), performRevenueRequest);
        }

        public static HttpResponseMessage TicketingProcessPerformRevenueItravel(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string formOfPayment, out string performRevenueRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\PerformRevenueItravel.xml"), appChannelName, applicationId, sessionId, formOfPayment);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            performRevenueRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), performRevenueRequest);
        }

        public static HttpResponseMessage PassengerManageMultipleInfantV2(X509Certificate2 x509Certificate, string appChannelName, string appChannelId, string sessionId, string passengerGivenName, string passengerSurname, string birthDay, string birthMonth, string birthYear, out string multipleInfantRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\ManageMultipleInfantV2.xml"), appChannelName, appChannelId, sessionId, passengerGivenName, passengerSurname, birthDay, birthMonth, birthYear);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            multipleInfantRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), multipleInfantRequest);
        }

        public static HttpResponseMessage AddSplRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string remarkFreeformText, out string requestAdd)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AddSpecialRemark.xml"), appChannelName, applicationId, sessionId, remarkFreeformText);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAdd = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestAdd);
        }

        public static HttpResponseMessage AddGenericRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string genericremarkFreeformText, out string requestAdd)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AddGenericRemark.xml"), appChannelName, applicationId, sessionId, genericremarkFreeformText);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAdd = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestAdd);
        }

        public static HttpResponseMessage TicketingProcessRetrieveArchived(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string ticketNumber, out string retrieveArchivedRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\RetrieveArchived.xml"), appChannelName, applicationId, ticketNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            retrieveArchivedRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), retrieveArchivedRequest);
        }

        public static HttpResponseMessage PassengerManageTravelDocsAndSecureFlight(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string passengerGivenName, string passengerSurName, out string manageTravelDocRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\ManageTravelDocsAndSecureFlight.xml"), appChannelName, applicationId, sessionId, passengerGivenName, passengerSurName);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            manageTravelDocRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), manageTravelDocRequest);
        }

        public static HttpResponseMessage AutoAddRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string remarkText, string typeOfRemark, string airlineCode, out string requestAutoAddRemark)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AutoAddRemark.xml"), appChannelName, applicationId, sessionId, remarkText, typeOfRemark, airlineCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAutoAddRemark = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestAutoAddRemark);
        }

        public static HttpResponseMessage ManageMultipleTravelDocsAndSecureFlight(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string passengerGivenName, string passengerSurName, string birthDay, string birthMonth, string birthYear, string gender, out string manageMultipleTravelDocRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\ManageMultipleTravelDocsAndSecureFlight.xml"), appChannelName, applicationId, sessionId, passengerGivenName, passengerSurName, birthDay, birthMonth, birthYear, gender, passengerGivenName, passengerSurName);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            manageMultipleTravelDocRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), manageMultipleTravelDocRequest);
        }

        public static HttpResponseMessage AddOsiRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string remarkText, string airlineCode, out string requestAddOsiRemark)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AddOSIRemark.xml"), appChannelName, applicationId, sessionId, remarkText, airlineCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAddOsiRemark = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestAddOsiRemark);
        }

        public static HttpResponseMessage PassengerUpdateGroupSFPDInfo(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string air4CreateRloc, out string updateGroupSFPDInfoRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\UpdateGroupSFPDInfo.xml"), appChannelName, applicationId, air4CreateRloc);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            updateGroupSFPDInfoRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), updateGroupSFPDInfoRequest);
        }

        public static HttpResponseMessage PassengerUpdateSFPD(X509Certificate2 x509Certificate, string appChannelName, string applicationVersion, string transactionId, string loyaltyAccountNumber, out string updateSFPDRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\UpdateSFPD.xml"), appChannelName, applicationVersion, transactionId, loyaltyAccountNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            updateSFPDRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), updateSFPDRequest);
        }

        public static HttpResponseMessage TicketingProcessReprice(X509Certificate2 x509Certificate, string applicationId, string sessionId, out string repriceRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketingProcessV3\\Reprice.xml"), applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            repriceRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/ticketingprocess/v3"), repriceRequest);
        }

        public static HttpResponseMessage SetupContextForNDC(X509Certificate2 x509Certificate, string recordLocator, out string setupContextRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\SetupContextForNDC.xml"), recordLocator);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            setupContextRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), setupContextRequest);
        }

        public static HttpResponseMessage ConvertMilesToCurrencyAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string CountryCode, string CurrencyCode, string TotalFareAmount, out string requestloyalty)
        {
            var requestxml = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\LoyaltyMiles\\ConvertMilesToCurrency.xml"), appChannelName, applicationId, CountryCode, CurrencyCode, TotalFareAmount);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(requestxml);

            xmlDocument.SignSoapXml(x509Certificate);

            requestloyalty = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.BaggageForZuluEnvt.GetUrl("32029/loyaltymiles"), requestloyalty);
        }

        public static HttpResponseMessage CurrencyConvertExecuteAIR4(X509Certificate2 x509Certificate, string applicationId, string CountryCode, string CurrencyCode, string TotalFareAmount, out string requestCurrency)
        {
            var requestxml = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\LoyaltyMiles\\CurrencyConvertExecute.xml"), applicationId, CountryCode, CurrencyCode, TotalFareAmount);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "Execute");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(requestxml);

            xmlDocument.SignSoapXml(x509Certificate);

            requestCurrency = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("32048/loyalty-miles-currencyconvert"), requestCurrency);
        }

        public static HttpResponseMessage DeleteOsiRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string recordNumber, string airlineCode, out string requestDeleteOsiRemark)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\DeleteOSIRemark.xml"), appChannelName, applicationId, sessionId, recordNumber, airlineCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestDeleteOsiRemark = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestDeleteOsiRemark);
        }

        public static HttpResponseMessage AutoModifyRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string modifyRemarkText, string typeOfRemark, string remarkInfoRecNo, out string requestAutoModifyRemark)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\AutoModifyRemark.xml"), appChannelName, applicationId, sessionId, modifyRemarkText, typeOfRemark, remarkInfoRecNo);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestAutoModifyRemark = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestAutoModifyRemark);
        }

        public static HttpResponseMessage RetrievePassengerMap(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string departureAirportCd, string arrivalAirportCd, string operatingFlightNumber, string marketingFlightNumber, string departureDate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SeatV8\\RetrievePassengerMaps.xml"), appChannelName, applicationId, departureAirportCd, arrivalAirportCd, operatingFlightNumber, marketingFlightNumber, departureDate);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "RetrievePassengerMap");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            request = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SeatForZuluEnvt.GetUrl("/seat/v8"), request, headers);
        }

        public static HttpResponseMessage ModifyStatusSegment(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string segmentNumber, string status, out string requestModifyStatusAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\ModifyStatus.xml"), appChannelName, applicationId, sessionId, segmentNumber, status);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestModifyStatusAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/segment/v4"), requestModifyStatusAir4);
        }

        public static HttpResponseMessage SeatV8RetrievePassengerMap(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string recordLocator, out string retrievePassengerMapRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SeatV8\\RetrievePassengerMap.xml"), appChannelName, applicationId, recordLocator);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "RetrievePassengerMap");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            retrievePassengerMapRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SeatForZuluEnvt.GetUrl("/seat/v8"), retrievePassengerMapRequest, headers);
        }

        public static HttpResponseMessage SeatV8Add(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string offerId, string offerItemNumber, string seatNumber, string chargeAmount, out string addRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SeatV8\\Add.xml"), appChannelName, applicationId, offerId, offerItemNumber, seatNumber, chargeAmount);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "Add");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            addRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SeatForZuluEnvt.GetUrl("/seat/v8"), addRequest, headers);
        }

        public static HttpResponseMessage DeleteGenericRemarkAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string remarkInfoRecNo, out string requestDeleteGenericRemark)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RemarkV3\\DeleteGenericRemark.xml"), appChannelName, applicationId, sessionId, remarkInfoRecNo);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestDeleteGenericRemark = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/remark/v3"), requestDeleteGenericRemark);
        }

        public static HttpResponseMessage AddPnrQueue(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string queueNumber, out string addPnrRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\QueueV2\\AddPnrQueue.xml"), appChannelName, applicationId, sessionId, queueNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            addPnrRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/queue/v2"), addPnrRequest);
        }

        public static HttpResponseMessage FaresV5BookPriceStoreFare(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string requestType, out string bookPriceStoreFareRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\FaresV5\\BookPriceStoreFare.xml"), appChannelName, applicationId, sessionId, requestType);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            bookPriceStoreFareRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("37025/fares/v5"), bookPriceStoreFareRequest);
        }

        public static HttpResponseMessage CancelItineraryAIR4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string requestCancelItineraryAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\ItineraryV5\\CancelItinerary.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestCancelItineraryAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/itinerary/v5"), requestCancelItineraryAir4);
        }

        public static HttpResponseMessage AddPnrPprPassenger(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string employeePprID, out string addPnrPprPassengerRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PrimaryPassRiderV3\\AddPnrPprPassenger.xml"), appChannelName, applicationId, sessionId, employeePprID);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            addPnrPprPassengerRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("36013/primarypassrider/v3"), addPnrPprPassengerRequest);
        }

        public static HttpResponseMessage RetrievePnrQueue(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string queueNumber, out string retrievePnrRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\QueueV2\\RetrievePnrQueue.xml"), appChannelName, applicationId, sessionId, queueNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            retrievePnrRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/queue/v2"), retrievePnrRequest);
        }

        public static HttpResponseMessage CustomerNotificationCleanPnr(X509Certificate2 x509Certificate, string applicationId, string recordLocator, out string cleanPnrRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\CustomerNotifications\\CleanPnr.xml"), applicationId, recordLocator);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            cleanPnrRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.BaggageForZuluEnvt.GetUrl("32014/customernotification"), cleanPnrRequest);
        }

        public static HttpResponseMessage LookUpArrayOfTokensAir4(X509Certificate2 x509Certificate, string appChannelName, string NetworkAddress, string SysCode, out string reqLookUpArrayOfTokensAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PCITokenManager\\LookUpArrayOfTokens.xml"), appChannelName, NetworkAddress, SysCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            reqLookUpArrayOfTokensAir4 = xmlDocument.OuterXml;

            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Content-Type", "text/xml" },
                { "SOAPAction", "LookupArrayOfTokens" }
            };

            return WebRequest.Xml.Post("https://webservices.vs01.vs.air4.com:36518/pcitokenmgr", reqLookUpArrayOfTokensAir4, headers);
        }

        public static HttpResponseMessage PassengerRetrieveTimaticInfo(X509Certificate2 x509Certificate, string applicationId, string recordLocator, string customerId, string pnrCreationDate, string firstName, string lastName, out string retrieveTimaticInfoRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PassengerV4\\RetrieveTimaticInfo.xml"), applicationId, recordLocator, customerId, pnrCreationDate, firstName, lastName);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            retrieveTimaticInfoRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/passenger/v4"), retrieveTimaticInfoRequest);
        }

        public static HttpResponseMessage RetrieveFlightAvailabilityAir4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string flightNumber, string flightOriginDate, out string requestRetrieveFlightAvailabilityAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Inventory\\RetrieveFlightAvailability.xml"), appChannelName, applicationId, flightNumber, flightOriginDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestRetrieveFlightAvailabilityAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("37520/inventory/v2"), requestRetrieveFlightAvailabilityAir4);
        }

        public static HttpResponseMessage ResDemandCodeAir4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string resDemandCode, out string requestResDemandCode)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\RESGenericRequest\\AnyDemandCode.xml"), appChannelName, applicationId, sessionId, resDemandCode);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestResDemandCode = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("38022/resgenericrequest/v2"), requestResDemandCode);
        }

        public static HttpResponseMessage CreditCardRetrieveAir4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string requestCreditCardRetrieve)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\CreditCardV2\\Retrieve.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestCreditCardRetrieve = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("33013/creditcard/v2"), requestCreditCardRetrieve);
        }

        public static HttpResponseMessage ToolsExecuteSoldSeatRetrieveAir4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string air4sessionId, string flightNumber, string segDepartureDate, out string requestExecuteSoldSeatAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Tools\\SeatsSoldCountbyClassRetrieve.xml"), appChannelName, applicationId, air4sessionId, flightNumber, segDepartureDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestExecuteSoldSeatAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("31084/tools-booking-seatsoldcountbyclassretrieve"), requestExecuteSoldSeatAir4);
        }

        public static HttpResponseMessage ToolsExecuteGroupPaxCountRetrieveAir4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string air4sessionId, string flightNumber, string segDepartureDate, out string requestExecuteGroupPaxCountAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\Tools\\GroupPassengersCountbyClassRetrieve.xml"), appChannelName, applicationId, air4sessionId, flightNumber, segDepartureDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestExecuteGroupPaxCountAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.Post(Config.SoaForZuluEnvt.GetUrl("31085/tools-booking-grouppassengerscountbyclassretrieve"), requestExecuteGroupPaxCountAir4);
        }

        public static HttpResponseMessage PaymentOrderCreate(X509Certificate2 x509Certificate, string applicationId, string paymentReferenceId, string amount, string currencyCode, string transactionSequenceNumber, out string createRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PaymentOrder\\Create.xml"), applicationId, paymentReferenceId, amount, currencyCode, transactionSequenceNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            createRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/paymentorder"), createRequest);
        }

        public static HttpResponseMessage RemovePnrQueue(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string removePnrRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\QueueV2\\RemovePnrQueue.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            removePnrRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/queue/v2"), removePnrRequest);
        }

        public static HttpResponseMessage SegmentCancelAndRebook(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, string returnDate, string cancelTripDate, string cancelReturnDate, out string cancelAndRebookRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\CancelAndRebook.xml"), appChannelName, applicationId, sessionId, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber, returnDate, cancelTripDate, cancelReturnDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            cancelAndRebookRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/segment/v4"), cancelAndRebookRequest);
        }

        public static HttpResponseMessage SegmentAutoModifyStatus(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string recordLocator, string airlineCode, string arrivalAirportCode, string classOfService, string tripDate, string departureAirportCode, string flightNumber, string status, out string autoModifyStatusRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SegmentV4\\AutoModifyStatus.xml"), appChannelName, applicationId, recordLocator, airlineCode, arrivalAirportCode, classOfService, tripDate, departureAirportCode, flightNumber, status);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            autoModifyStatusRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/segment/v4"), autoModifyStatusRequest);
        }

        public static HttpResponseMessage SeatV8Delete(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string recordLocator, string orderId, string arrivalAirportCd, string classOfService, string DepartureAirportCd, string departureDate, out string deleteRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SeatV8\\Delete.xml"), appChannelName, applicationId, recordLocator, orderId, arrivalAirportCd, classOfService, DepartureAirportCd, departureDate);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "Delete");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            deleteRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SeatForZuluEnvt.GetUrl("/seat/v8"), deleteRequest, headers);
        }

        public static HttpResponseMessage SeatV8Modify(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string offerId, string offerItemNumber, string seatNumber, string chargeAmount, string amountDecimalPlacesout, out string modifyRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SeatV8\\Modify.xml"), appChannelName, applicationId, offerId, offerItemNumber, seatNumber, chargeAmount, amountDecimalPlacesout);

            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("SOAPAction", "Modify");

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            modifyRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SeatForZuluEnvt.GetUrl("/seat/v8"), modifyRequest, headers);
        }

        public static HttpResponseMessage IgnoreAir4(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, out string ignoreRequestAir4)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\Ignore.xml"), appChannelName, applicationId, sessionId);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            ignoreRequestAir4 = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), ignoreRequestAir4);
        }

        public static HttpResponseMessage CreatePnrForNDC(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string arrivalPoint, string arrivalTime, string classOfService, string departureDate, string departurePoint, string departureTime, string flightNumber, out string requestCreatePnrNdc)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\CreatePnrForNDC.xml"), appChannelName, applicationId, arrivalPoint, arrivalTime, classOfService, departureDate, departurePoint, departureTime, flightNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            requestCreatePnrNdc = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), requestCreatePnrNdc);
        }

        public static HttpResponseMessage SFPDUpdate(X509Certificate2 dlCertificate, string airlineCode, string dateOfBirth, string firstName, string lastName, string gender, string fcNumber, out string requestSFPDUpdate)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SFPD\\SFPDUpdate.xml"), airlineCode, dateOfBirth, firstName, lastName, gender, fcNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(dlCertificate);

            requestSFPDUpdate = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForLoopsAIR4.GetUrl("/passenger/v4"), requestSFPDUpdate);
        }

        public static HttpResponseMessage SFPDRetrieve(X509Certificate2 dlCertificate, string airlineCode, string transactionId, string fcNumber, out string requestSFPDRetrieve)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\SFPD\\SFPDRetrieve.xml"), airlineCode, transactionId, fcNumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(dlCertificate);

            requestSFPDRetrieve = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForLoopsAIR4.GetUrl("/passenger/v4"), requestSFPDRetrieve);
        }

        public static HttpResponseMessage Decode(X509Certificate2 x509Certificate, string applicationId, string recordLocator, out string decodeRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\PNRCommonV6\\Decode.xml"), applicationId, recordLocator);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            decodeRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("36013/pnrcommon/v6"), decodeRequest);
        }

        public static HttpResponseMessage Refund(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string ticketnumber, out string refundRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketV3\\Refund.xml"), appChannelName, applicationId, sessionId, ticketnumber);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            refundRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("37526/ticket/v3"), refundRequest);
        }

        public static HttpResponseMessage Arefund(X509Certificate2 x509Certificate, string appChannelName, string applicationId, string sessionId, string ticketnumber, string pnrcreationDate, out string refundRequest)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\AIR4\\TicketV3\\ActualRefund.xml"), appChannelName, applicationId, sessionId, ticketnumber, pnrcreationDate);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(x509Certificate);

            refundRequest = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForZuluEnvt.GetUrl("37526/ticket/v3"), refundRequest);
        }

        public static HttpResponseMessage LoyaltyProfileSyncAdd(X509Certificate2 dlCertificate, string memberNumber, string dateOfBirth, string firstName, string lastName, string gender, string email, out string requestLoyaltyProfileSyncAdd)
        {
            var request = string.Format(File.ReadAllText("Requests\\Body\\SOA\\LoyaltyProfileSyncAdd.xml"), memberNumber, dateOfBirth, firstName, lastName, gender, email);

            XmlDocument xmlDocument = new();

            xmlDocument.LoadXml(request);

            xmlDocument.SignSoapXml(dlCertificate);

            requestLoyaltyProfileSyncAdd = xmlDocument.OuterXml;

            return WebRequest.Xml.PostSoap(Config.SoaForLoyaltyProfileSyncAIR4.GetUrl("/loyaltyprofilesync"), requestLoyaltyProfileSyncAdd);
        }
    }
}
