using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.NDC.Helpers;

namespace VAA.EAPI.Automation.NDC.Requests.Endpoints
{
    public static class NDCTransformationEndpoints
    {
        private const string CommonTypesNamespace = "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes";
        public static readonly string env = Config.Environment;
        public static readonly string version = Config.Version;

        public static HttpResponseMessage OrderRetrieve(string bookingId, string subscriptionKey = "")
        {
            string requestPayload = string.Format(File.ReadAllText("requests\\Body\\OrderRetrieve.xml"), bookingId);

            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty())
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            string versionSegment = GetVersionSegment();
            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.IsAny("sandbox", "prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderRetrieve"), requestPayload, headers);
        }

        public static HttpResponseMessage GDSOrderRetrieves(string bookingId, string orderId, string subscriptionKey = "")
        {
            string requestPayload = string.Format(File.ReadAllText("requests\\Body\\OrderRetrieves.xml"), bookingId, orderId);

            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty())
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            string versionSegment = GetVersionSegment();
            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderRetrieve"), requestPayload, headers);
        }

        public static HttpResponseMessage OrderReshop(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(GetOfferManagementUrl("OrderReshop", versionSegment), requestPayload, headers);
        }

        public static HttpResponseMessage OrderHistory(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderHistory"), requestPayload, headers);
        }

        public static HttpResponseMessage AirShoppingRequest(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(GetOfferManagementUrl("AirShopping", versionSegment), requestPayload, headers);
        }

        public static HttpResponseMessage OfferPrice(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            string versionSegment = GetVersionSegment();
            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return WebRequest.Xml.PostSoap(GetOfferManagementUrl("OfferPrice", versionSegment),
                requestPayload, headers);
        }

        public static HttpResponseMessage OrderCreate(string requestPayload, string subscriptionKey = "", string bookingType = "Unknown", string paymentType = "Unknown", string seatType = "Unknown")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            HttpResponseMessage response = WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderCreate"), requestPayload, headers);

            if (response.IsSuccessStatusCode)
            {
                string orderCreateResult = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                string pnr = string.Empty;

                try
                {
                    pnr = FindAndReturnDataHelper.ReturnBookingId(orderCreateResult);
                }
                catch (XmlException xmlException)
                {
                    Console.Error.WriteLine($"NDC PNR storage skipped because OrderCreate returned invalid XML: {xmlException.Message}");
                }

                string resolvedBookingType = bookingType;

                if (resolvedBookingType.Is("Unknown"))
                {
                    resolvedBookingType = requestPayload.Contains("PaymentFunctions", StringComparison.OrdinalIgnoreCase) ? "InstantPurchase" : "HoldBooking";
                }

                string resolvedPaymentType = ResolvePaymentType(requestPayload, paymentType);
                string resolvedSeatType = ResolveSeatType(requestPayload, seatType);

                if (!pnr.IsNullOrWhiteSpace())
                {
                    try
                    {
                        StorageAccountHelper.SavePNRToStorageTable(pnr, Config.Environment, resolvedBookingType, resolvedPaymentType, resolvedSeatType);
                    }
                    catch (Exception storageException)
                    {
                        Console.Error.WriteLine($"NDC PNR storage write was skipped after OrderCreate succeeded: {storageException.GetType().Name}: {storageException.Message}");
                    }
                }
            }

            return response;
        }

        private static string ResolvePaymentType(string requestPayload, string paymentType)
        {
            if (!paymentType.Is("Unknown"))
            {
                return paymentType;
            }

            if (requestPayload.Contains("IATA_EasyPay", StringComparison.OrdinalIgnoreCase) || requestPayload.Contains("EasyPay", StringComparison.OrdinalIgnoreCase))
            {
                return "EasyPay";
            }

            XmlDocument requestDocument = new XmlDocument();
            try
            {
                requestDocument.LoadXml(requestPayload);
            }
            catch (XmlException)
            {
                return "Unknown";
            }

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(requestDocument.NameTable);
            namespaceManager.AddNamespace("ct", CommonTypesNamespace);
            XmlNode paymentTypeNode = requestDocument.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethodCriteria//ct:PaymentTypeCode", namespaceManager);
            string requestPaymentType = paymentTypeNode?.InnerText;

            if (requestPaymentType.Is("CA"))
            {
                return "Cash";
            }

            if (requestPaymentType.Is("EP"))
            {
                return "EasyPay";
            }

            return requestPaymentType.IsNullOrWhiteSpace() ? "Unknown" : requestPaymentType;
        }

        private static string ResolveSeatType(string requestPayload, string seatType)
        {
            if (!seatType.Is("Unknown"))
            {
                return seatType;
            }

            return requestPayload.Contains("SelectedSeat", StringComparison.OrdinalIgnoreCase) ? "Selected" : "None";
        }

        public static HttpResponseMessage OrderCancel(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("SIT", "DEV", "UAT"))
            {
                subscriptionKey = Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-Cancel-OcpSubID");
            }
            else
            {
                subscriptionKey = Vault.Default.GetSecretValue($"AVATEUNOFFER-Config-Agent-CancelPreview-OcpSubID");
            }

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("preview") ? "sb" : $"{env}/")}NdcOrderManagement/ordercancel"), requestPayload, headers);
        }

        public static HttpResponseMessage OrderChange(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderChange"), requestPayload, headers);

        }

        public static HttpResponseMessage OrderList(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderList"), requestPayload, headers);
        }

        public static HttpResponseMessage OrderQuote(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(GetOfferManagementUrl("OrderQuote", versionSegment), requestPayload, headers);
        }

        public static HttpResponseMessage SeatAvailability(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(GetOfferManagementUrl("SeatAvailability", versionSegment), requestPayload, headers);
        }

        public static HttpResponseMessage OrderRetrieveRequest(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}SalesAndRevenueManagement/OrderShopping/OrderManagement/{versionSegment}OrderRetrieve"), requestPayload, headers);
        }

        public static HttpResponseMessage ServiceListRequest(string requestPayload, string subscriptionKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (env.IsAny("sandbox", "preview", "prod"))
                requestPayload = GetUpdatedRequest(requestPayload);

            if (subscriptionKey.IsNullOrEmpty() || env.IsAny("sandbox", "preview", "prod"))
                subscriptionKey = GetAgentSubscriptionKey(requestPayload);

            headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            string versionSegment = GetVersionSegment();

            return WebRequest.Xml.PostSoap(GetOfferManagementUrl("ServiceList", versionSegment), requestPayload, headers);
        }

        private static string GetAgentSubscriptionKey(string requestPayload)
        {
            const string DefaultAgentId = "91281035";

            // Load XML and extract OrgID values
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(requestPayload);
            XmlNamespaceManager xmlNamespaceManager = doc.CreateXmlNamespaceManager();

            XmlNodeList orgIds = doc.SelectNodes("//ct:ParticipatingOrg/ct:OrgID", xmlNamespaceManager);

            string agentID = orgIds.Count >= 1
                 ? orgIds[0].InnerText
                 : DefaultAgentId;

            string agentDistributionID = orgIds.Count >= 2
                 ? orgIds[1].InnerText
                 : null;

            // Determine environment suffix
            // Sandbox intentionally uses Preview secrets
            string envSuffix =
                env.Is("prod") ? "Prod" :
                env.Is("preview") || env.Is("sandbox") ? "Preview" :
                null;

            // Prefer distribution ID when present
            //var id = agentDistributionID ?? agentID;
            string id = agentDistributionID.IsNullOrEmpty() || agentDistributionID.Is("VS")
                ? agentID
                : agentDistributionID ?? agentID;

            // Build secret name
            string secretName = envSuffix != null
                ? $"AVATEUNOFFER-Config-Agent-{envSuffix}-{id}-OcpSubID"
                : $"AVATEUNOFFER-Config-Agent-{id}-OcpSubID";

            // Fetch secret from Key Vault
            return Vault.Default.GetSecretValue(secretName);
        }

        private static string GetVersionSegment() => env.Is("prod") ? string.Empty : $"{version}/";

        private static string GetOfferManagementUrl(string endpoint, string versionSegment)
        {
            if (Config.UseDigitalNdcEndpoints)
                return $"{Config.DigitalNdcOfferManagementBaseUrl.TrimEnd('/')}/{endpoint}";

            bool usesDigitalOfferManagementRoute = env.IsAny("dev", "sit", "uat");
            string digitalSegment = usesDigitalOfferManagementRoute ? "Digital/" : string.Empty;
            string offerManagementVersionSegment = usesDigitalOfferManagementRoute ? string.Empty : versionSegment;

            return Config.TestResourcesVirginAtlantic.GetUrl($"/{(env.Is("sandbox") || env.Is("prod") ? string.Empty : $"{env}/")}{digitalSegment}SalesAndRevenueManagement/OrderShopping/OfferManagement/{offerManagementVersionSegment}{endpoint}");
        }

        private static string GetUpdatedRequest(string requestPayload)
        {
            XmlDocument updatedRQ = new XmlDocument();
            XmlNamespaceManager requestXmlnsManager = new XmlNamespaceManager(updatedRQ.NameTable);
            requestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            updatedRQ.LoadXml(requestPayload);

            updatedRQ.SelectSingleNode("//def:ParticipatingOrg//def:OrgID", requestXmlnsManager).InnerText = "91269021";

            return updatedRQ.OuterXml;

        }
    }
}
