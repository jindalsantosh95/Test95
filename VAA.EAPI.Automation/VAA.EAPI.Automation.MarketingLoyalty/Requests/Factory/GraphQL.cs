using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Factory
{
    public static class GraphQL
    {

        #region CampaignManagementRequestPayloads
        public static string AssignCampaign(string customerId, string memberNumber, string campaignCode, string campaignId, string createdBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\AssignCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken campaign = variables["campaignAssignRequest"];

            campaign["customerId"] = customerId;
            campaign["memberNumber"] = memberNumber;
            campaign["campaignCode"] = campaignCode;
            campaign["campaignId"] = campaignId;
            campaign["createdBy"] = createdBy;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CampaignUpdate(string campaignId, string campaignCode, string campaignName, string startDate, string endDate, string status, string type, string desc, bool regReqFlag, string linkToTemplate)
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\UpdateCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken campaign = variables["campaign"];

            campaign["campaignId"] = campaignId;
            campaign["campaignCode"] = campaignCode;
            campaign["campaignName"] = campaignName;
            campaign["startDate"] = startDate;
            campaign["endDate"] = endDate;
            campaign["status"] = status;
            campaign["type"] = type;
            campaign["description"] = desc;
            campaign["registrationRequiredFlag"] = regReqFlag;
            campaign["linkToTemplate"] = linkToTemplate;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RegisterCampaign(string customerId, string memberNumber, string campaignCode, string campaignId, string documentId, string createdBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\RegisterCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken campaign = variables["campaignRegisterRequest"];

            campaign["customerId"] = customerId;
            campaign["memberNumber"] = memberNumber;
            campaign["campaignCode"] = campaignCode;
            campaign["campaignId"] = campaignId;
            campaign["documentId"] = documentId;
            campaign["createdBy"] = createdBy;

            return JsonConvert.SerializeObject(payloadObject);
        }
        public static string RetrieveMemberCampaign(string customerId, string memberNumber, string order, string where)
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\RetrieveMemberCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            // JToken campaign = variables["campaignRetrieveRequest"];

            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;
            variables["order"] = order;
            variables["where"] = where;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveCampaignType()
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\RetrieveCampaignTypes.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);
        }
        public static string CampaignCreate(string campaignCode, string campaignName, string startDate, string endDate, string status, string type, string desc, bool regReqFlag, string linkToTemplate)
        {

            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\CreateCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);


            JToken variables = payloadObject["variables"];
            JToken campaign = variables["campaign"];

            campaign["campaignCode"] = campaignCode;
            campaign["campaignName"] = campaignName;
            campaign["startDate"] = startDate;
            campaign["endDate"] = endDate;
            campaign["status"] = status;
            campaign["type"] = type;
            campaign["description"] = desc;
            campaign["registrationRequiredFlag"] = regReqFlag;
            campaign["linkToTemplate"] = linkToTemplate;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string PromotionUpdate(string promotionId, string promotionName, string partnerName, string startDate, string endDate, string activeFlag)
        {
            string payload = File.ReadAllText("Requests\\Body\\PromotionManagement\\UpdatePromotion.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken promotion = variables["promotion"];

            promotion["promotionId"] = promotionId;
            promotion["promotionName"] = promotionName;
            promotion["partnerName"] = partnerName;
            promotion["startDate"] = startDate;
            promotion["endDate"] = endDate;
            promotion["active"] = activeFlag;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UnassignCampaign(string customerId, string documentId)
        {

            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\UnassignCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);


            JToken variables = payloadObject["variables"];
            JToken campaign = variables["campaignUnassignRequest"];

            campaign["customerId"] = customerId;
            campaign["documentId"] = documentId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CampaignPromotionLink(string campaignId, string promotionId, string createdBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\PromotionManagement\\LinkPromotion.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken campaignPromotionLink = variables["linkCampaignPromotionRequest"];

            campaignPromotionLink["campaignId"] = campaignId;
            campaignPromotionLink["promotionId"] = promotionId;
            campaignPromotionLink["createdBy"] = createdBy;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string DelinkCampaignPromotion(string campaignId, string promotionId)
        {

            string payload = File.ReadAllText("Requests\\Body\\PromotionManagement\\DelinkCampaignPromotion.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);


            JToken variables = payloadObject["variables"];
            JToken campaign = variables["delinkCampaignPromotionRequest"];

            campaign["campaignId"] = campaignId;
            campaign["promotionId"] = promotionId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CampaignSearch(string campaignId, string campaignName, string campaignCode, string startDateFrom, string endDateFrom, string startDateTo, string endDateTo, string status, string type)
        {

            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\SearchCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken campaign = variables["param"];

            campaign["campaignId"] = campaignId;
            campaign["campaignName"] = campaignName;
            campaign["campaignCode"] = campaignCode;
            campaign["startDateF"] = startDateFrom;
            campaign["endDateF"] = endDateFrom;
            campaign["startDateT"] = startDateTo;
            campaign["endDateT"] = endDateTo;
            campaign["status"] = status;
            campaign["type"] = type;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CampaignRetrieve(string campaignId, string campaignCode)
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\RetrieveCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["campaignId"] = campaignId;
            variables["campaignCode"] = campaignCode;

            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion

        #region PromotionManagementRequestPayloads
        public static string PromotionCreate(string promotionName, string partnerName, string startDate, string endDate, string activeFlag)
        {
            string payload = File.ReadAllText("Requests\\Body\\PromotionManagement\\CreatePromotion.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken promotion = variables["promotion"];

            promotion["promotionName"] = promotionName;
            promotion["partnerName"] = partnerName;
            promotion["startDate"] = startDate;
            promotion["endDate"] = endDate;
            promotion["active"] = activeFlag;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string PromotionSearchCampaign(string promotionId)
        {

            string payload = File.ReadAllText("Requests\\Body\\PromotionManagement\\PromotionSearchCampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["promotionId"] = promotionId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string PromotionSearchAndRetrieve(string promotionId, string promotionName, string startDateF, string endDateF, string startDateT, string endDateT, string partnerName, string active)
        {
            string payload = File.ReadAllText("Requests\\Body\\PromotionManagement\\PromotionSearchAndRetrieve.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);


            JToken variables = payloadObject["variables"];
            variables["promotionId"] = promotionId;
            variables["promotionName"] = promotionName;
            variables["startDateF"] = startDateF;
            variables["endDateF"] = endDateF;
            variables["startDateT"] = startDateT;
            variables["endDateT"] = endDateT;
            variables["partnerName"] = partnerName;
            variables["active"] = active;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string DownloadCampaignList()
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\DownloadCampaignsList.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);

        }

        public static string AssociatedMembersToACampaign(string campaignCode)
        {
            string payload = File.ReadAllText("Requests\\Body\\CampaignManagement\\AssociatedMembersToACampaign.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            variables["campaignCode"] = campaignCode;

            return JsonConvert.SerializeObject(payloadObject);

        }

        #endregion

        #region LoyaltyCreateNotesRequestPayloads
        public static string LoyaltyCreateNote(string customerId, string memberNumber, string notesGroup, string noteType, string text, string createdBy, string lastUpdatedBy, string createdOnDate, string lastUpdatedOnDate, string docId)
        {
            string payload = File.ReadAllText("Requests\\Body\\ContactNotes\\LoyaltyCreateNote.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberId"] = memberNumber;
            variables = variables["contactNote"];
            variables["group"] = notesGroup;
            variables["type"] = noteType;
            variables["text"] = text;
            variables["createdBy"] = createdBy;
            variables["lastUpdatedBy"] = lastUpdatedBy;
            variables["createdOn"] = createdOnDate;
            variables["lastUpdatedOn"] = lastUpdatedOnDate;
            variables["id"] = docId;

            return JsonConvert.SerializeObject(payloadObject);

        }
        public static string LoyaltyUpdateNote(string customerId, string memberNumber, string notesGroup, string noteType, string text, string createdBy, string lastUpdatedBy, string createdOnDate, string lastUpdatedOnDate, string docId)
        {
            string payload = File.ReadAllText("Requests\\Body\\ContactNotes\\LoyaltyUpdateNote.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberId"] = memberNumber;
            variables = variables["contactNote"];
            variables["group"] = notesGroup;
            variables["type"] = noteType;
            variables["text"] = text;
            variables["createdBy"] = createdBy;
            variables["lastUpdatedBy"] = lastUpdatedBy;
            variables["createdOn"] = createdOnDate;
            variables["lastUpdatedOn"] = lastUpdatedOnDate;
            variables["id"] = docId;

            return JsonConvert.SerializeObject(payloadObject);

        }

        public static string LoyaltyDeleteNote(string customerId, string memberNumber, string notesGroup, string noteType, string text, string createdBy, string lastUpdatedBy, string createdOnDate, string lastUpdatedOnDate, string docId)
        {
            string payload = File.ReadAllText("Requests\\Body\\ContactNotes\\LoyaltyDeleteNote.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberId"] = memberNumber;
            variables = variables["contactNote"];
            variables["group"] = notesGroup;
            variables["type"] = noteType;
            variables["text"] = text;
            variables["createdBy"] = createdBy;
            variables["lastUpdatedBy"] = lastUpdatedBy;
            variables["createdOn"] = createdOnDate;
            variables["lastUpdatedOn"] = lastUpdatedOnDate;
            variables["id"] = docId;

            return JsonConvert.SerializeObject(payloadObject);

        }
        public static string LoyaltyGetNote(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\ContactNotes\\LoyaltyGetNote.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberId"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);

        }
        #endregion

        #region LoyaltyMemberManagementRequestPayloads

        public static string PartnerRetrieval()
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\RetrieveLoyaltyPartnersDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveMemberAccountDetails(string customerId, string where)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\RetrieveMemberAccountDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["where"] = where;

            return JsonConvert.SerializeObject(payloadObject);
        }
        public static string RetrieveHouseHoldDetails(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\RetrieveHouseHoldDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberId"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string HouseHoldLinking(string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\HouseHoldLinking.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables = variables["houseHoldLinkingRequest"];
            variables["childCustomerId"] = childCustomerId;
            variables["childMemberNumber"] = childMemberNumber;
            variables["headCustomerId"] = parentCustomerId;
            variables["headMemberNumber"] = parentMemberNumber;
            variables["initiatedBy"] = initiatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string HouseHoldDelinking(string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\HouseHoldDelinking.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables = variables["houseHoldDelinkingRequest"];
            variables["childCustomerId"] = childCustomerId;
            variables["childMemberNumber"] = childMemberNumber;
            variables["headCustomerId"] = parentCustomerId;
            variables["headMemberNumber"] = parentMemberNumber;
            variables["initiatedBy"] = initiatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }
        #endregion

        #region CustomerRequestPayloads

        public static string CustomerSearch(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\CustomerSearch.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }
        #endregion

        #region SupplementarySilverPayloads

        public static string SupplementarySilverSearch(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\SupplementarySilverAccountDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberId"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SupplementarySilverDelink(string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\SupplementarySilverDelinking.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables = variables["supplementarySilverDelinkingRequest"];
            variables["childCustomerId"] = childCustomerId;
            variables["childMemberNumber"] = childMemberNumber;
            variables["headCustomerId"] = parentCustomerId;
            variables["headMemberNumber"] = parentMemberNumber;
            variables["initiatedBy"] = initiatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }


        public static string SupplementarySilverLink(string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberManagement\\SupplementarySilverLinking.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables = variables["supplementarySilverLinkingRequest"];
            variables["childCustomerId"] = childCustomerId;
            variables["childMemberNumber"] = childMemberNumber;
            variables["headCustomerId"] = parentCustomerId;
            variables["headMemberNumber"] = parentMemberNumber;
            variables["initiatedBy"] = initiatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion

        #region AuditManagementPayloads

        public static string RetrieveAddressAudit(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrieveAddressAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveContactAudit(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrieveContactAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrievePreferenceAudit(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrievePreferenceAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveAwardAudit(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrieveAwardAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveCustomerProfileAudit(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrieveCustomerProfileAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveSupplementarySilverAudit(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrieveSupplementarySilverAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveAmexCenturionAuditHistory(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\AuditManagement\\RetrieveAmexCenturionAudit.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }
        #endregion

        #region Retail Therapy Request Payloads

        public static string CreateEarnTransactionForRetailTherapy(string activityDate, string channel, string comment, string memberNumber, string orderId, int points)
        {
            string payload = File.ReadAllText("Requests\\Body\\RetailTherapy\\CreateEarnTransaction.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken transactionRequest = variables["transactionRequest"];

            transactionRequest["activityDate"] = activityDate;
            transactionRequest["channel"] = channel;
            transactionRequest["comment"] = comment;
            transactionRequest["orderId"] = orderId;
            transactionRequest["points"] = points;
            transactionRequest["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreateBurnTransactionForRetailTherapy(string activityDate, string channel, string comment, string memberNumber, string orderId, int points)
        {
            string payload = File.ReadAllText("Requests\\Body\\RetailTherapy\\CreateBurnTransaction.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken transactionRequest = variables["transactionRequest"];

            transactionRequest["activityDate"] = activityDate;
            transactionRequest["channel"] = channel;
            transactionRequest["comment"] = comment;
            transactionRequest["orderId"] = orderId;
            transactionRequest["points"] = points;
            transactionRequest["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveMemberBalanceForRetailTherapy()
        {
            string payload = File.ReadAllText("Requests\\Body\\RetailTherapy\\AccountMemberDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            return JsonConvert.SerializeObject(payloadObject);

        }


        public static string CancelTransactionForRetailTherapy(string activityDate, string cancellationReason, string channel, string memberNumber, string transactionId)
        {
            string payload = File.ReadAllText("Requests\\Body\\RetailTherapy\\CancelTransaction.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken cancelRequest = variables["cancelRequest"];

            cancelRequest["activityDate"] = activityDate;
            cancelRequest["cancellationReason"] = cancellationReason;
            cancelRequest["channel"] = channel;
            cancelRequest["memberNumber"] = memberNumber;
            cancelRequest["transactionId"] = transactionId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion

        #region Partner Retro Request Payloads
        public static string SkyPartnerRetroCreate(string customerId, string documentFirstName, string documentSurname, string memberNumber, string boardingNumber, string gdsRecordLocator, string seatNumber, string ticketIssueDate, string ticketNumber, string marketedCabin, string flightActivityDate, string destinationAirportCode, string originAirportCode, string marketingAirlineCode, string marketingBookingClass, string marketingFlightNumber, string operatedAirlineCode, string operatedFlightNumber, string operatedbookingClass, string codeshareFlag, string vsComments, string productName, string createdBy, string transactionChannel)
        {
            string payload = File.ReadAllText("Requests\\Body\\PartnerRetro\\SkyPartnerRetroCreate.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken partnerRetroRequest = variables["partnerRetroRequest"];
            partnerRetroRequest["transactionChannel"] = transactionChannel;
            partnerRetroRequest["createdBy"] = createdBy;

            JToken loyTransaction = variables["partnerRetroRequest"]["loyTransaction"];
            loyTransaction["productName"] = productName;
            loyTransaction["vsComments"] = vsComments;
            loyTransaction["codeshareFlag"] = codeshareFlag;

            JToken bookingDetails = variables["partnerRetroRequest"]["loyTransaction"]["bookingDetails"];
            bookingDetails["boardingNumber"] = boardingNumber;
            bookingDetails["gdsRecordLocator"] = gdsRecordLocator;
            bookingDetails["seatNumber"] = seatNumber;
            bookingDetails["ticketIssueDate"] = ticketIssueDate;
            bookingDetails["ticketNumber"] = ticketNumber;

            JToken flightDetails = variables["partnerRetroRequest"]["loyTransaction"]["bookingDetails"]["flightDetails"];
            flightDetails["marketedCabin"] = marketedCabin;
            flightDetails["flightActivityDate"] = flightActivityDate;
            flightDetails["destinationAirportCode"] = destinationAirportCode;
            flightDetails["marketingAirlineCode"] = marketingAirlineCode;
            flightDetails["marketingBookingClass"] = marketingBookingClass;
            flightDetails["marketingFlightNumber"] = marketingFlightNumber;
            flightDetails["operatedAirlineCode"] = operatedAirlineCode;
            flightDetails["operatedFlightNumber"] = operatedFlightNumber;
            flightDetails["operatedbookingClass"] = operatedbookingClass;
            flightDetails["originAirportCode"] = originAirportCode;

            JToken customerDetails = variables["partnerRetroRequest"]["loyTransaction"]["customerDetails"];
            customerDetails["customerId"] = customerId;
            customerDetails["documentFirstName"] = documentFirstName;
            customerDetails["documentSurname"] = documentSurname;
            customerDetails["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }
        #endregion

        #region LoyaltyMemberCard payloads
        public static string RetrieveLoyaltyMemberCardLink(string customerId, string memberNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberCards\\LoyaltyMemberCardLink.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreatePartnerMembership(string cardType, string customerId, string memberNumber, string partnerCode, string partnerId, string startDate)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberCards\\CreatePartnerMembershipGraphQl.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken partnerMembershipRequest = variables["partnerMembershipRequest"];
            partnerMembershipRequest["cardType"] = cardType;
            partnerMembershipRequest["memberNumber"] = memberNumber;
            partnerMembershipRequest["customerId"] = customerId;
            partnerMembershipRequest["partnerCode"] = partnerCode;
            partnerMembershipRequest["partnerId"] = partnerId;
            partnerMembershipRequest["startDate"] = startDate;

            return JsonConvert.SerializeObject(payloadObject);
        }


        public static string UpdatePartnerMembership(string cardType, string customerId, string memberNumber, string partnerCode, string partnerId, string startDate, string endDate, string docId, string updatedBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberCards\\UpdatePartnerMembershipGraphQl.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];
            JToken partnerMembershipRequest = variables["partnerMembershipRequest"];
            partnerMembershipRequest["cardType"] = cardType;
            partnerMembershipRequest["memberNumber"] = memberNumber;
            partnerMembershipRequest["customerId"] = customerId;
            partnerMembershipRequest["partnerCode"] = partnerCode;
            partnerMembershipRequest["partnerId"] = partnerId;
            partnerMembershipRequest["startDate"] = startDate;
            partnerMembershipRequest["endDate"] = endDate;
            partnerMembershipRequest["id"] = docId;
            partnerMembershipRequest["createdBy"] = updatedBy;
            partnerMembershipRequest["lastUpdatedBy"] = updatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion

        #region Create Voucher Accruals transaction payloads

        public static string CreateVoucherAccrualTransaction(string customerId, string memberNumber, string memberStatus, string activityDate, string comments, string createdBy, string bookingClass, string bookingReference, string originAirportCode, string destinationAirportCode, string ticketNumber, int noOfPassengers, int noOfSegments, string partnerCode, string productName, int points, int tierPoints, string transactionSource, string voucherNumber, string voucherUsage, string voucherId = "", string voucherName = "")
        {
            string payload = File.ReadAllText("Requests\\Body\\CreateAccrual\\LoyaltyCreateAccrual.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"]["transactionRequest"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;
            variables["activityDate"] = activityDate;
            variables["comments"] = comments;
            variables["createdBy"] = createdBy;
            variables["memberStatus"] = memberStatus;
            variables["noOfPassengers"] = noOfPassengers;
            variables["noOfSegments"] = noOfSegments;
            variables["partnerCode"] = partnerCode;
            variables["productName"] = productName;
            variables["points"] = points;
            variables["tierPoints"] = tierPoints;
            variables["transactionSource"] = transactionSource;

            JToken flightDetails = payloadObject["variables"]["transactionRequest"]["flightDetails"];
            flightDetails["bookingClass"] = bookingClass;
            flightDetails["bookingReference"] = bookingReference;
            flightDetails["destinationAirport"] = destinationAirportCode;
            flightDetails["originationAirport"] = originAirportCode;
            flightDetails["ticketNumber"] = ticketNumber;

            JToken voucherDetails = payloadObject["variables"]["transactionRequest"]["voucherDetails"];
            voucherDetails["voucherNumber"] = voucherNumber;
            voucherDetails["voucherUsage"] = voucherUsage;
            voucherDetails["voucherId"] = voucherId;
            voucherDetails["voucherName"] = voucherName;

            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion


        #region Loyalty Sync Payloads

        public static string loyaltySyncSFPD(string customerId, string dateOfBirth, string enrollmentChannel, string fcNumber, string firstName, string gender, string lastName)
        {

            string payload = File.ReadAllText("Requests\\Body\\LoyaltySync\\LoyaltySyncSFPD.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"]["passengerDetailRequest"];
            variables["customerId"] = customerId;
            variables["dateOfBirth"] = dateOfBirth;
            variables["enrollmentChannel"] = enrollmentChannel;
            variables["fcNumber"] = fcNumber;
            variables["firstName"] = firstName;
            variables["gender"] = gender;
            variables["lastName"] = lastName;

            return JsonConvert.SerializeObject(payloadObject);

        }

        #endregion

        public static string VACProfileUpdate(string customerId, string memberNumber, string firstName, string lastName, string emailId, string dateOfBirth, string phoneCountryCode, string phoneNumber, string preferenceValue)
        {
            string payload = File.ReadAllText("Requests\\Body\\VAAccount\\VACGraphQLProfileUpdate.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"]["updateMemberProfileRequest"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;
            variables["firstName"] = firstName;
            variables["lastName"] = lastName;
            variables["dateOfBirth"] = dateOfBirth;
            variables["emailId"] = emailId;
            variables["countryCode"] = phoneCountryCode;
            variables["phoneNumber"] = phoneNumber;
            variables["preferences"][0]["preferenceValue"] = preferenceValue;
            variables["lastUpdatedBy"] = Application.AutomationScriptCreatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string MemberSearchByPhoneNumber(string phoneNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\MemberSearchByPhone\\MemberSearchByPhone.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            variables["phoneNumber"] = phoneNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveRelatedTransactions(string customerId, string memberNumber, string transactionId)
        {
            string query = File.ReadAllText("Requests\\Body\\AutoCancellation\\RetrieveRelatedTransactions.txt");

            GraphQLRequest graphRequest = new()
            {
                query = query,
                variables = new RetrieveRelatedTransactionsVariables()
                {
                    relatedTransactionRequest = new Relatedtransactionrequest
                    {
                        customerId = customerId,
                        memberNumber = memberNumber,
                        transactionId = transactionId
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string CancelRelatedTransactions(string customerId, string memberNumber, string initiatedTransactionId, string cancellationReason, string initiatedBy, List<Canceltransactiondetail> cancelTransactionDetails)
        {
            string query = File.ReadAllText("Requests\\Body\\AutoCancellation\\CancelRelatedTransactions.txt");

            GraphQLRequest graphRequest = new()
            {
                query = query,
                variables = new CancelRelatedTransactionsVariables()
                {
                    cancelTransactionRequest = new Canceltransactionrequest
                    {
                        customerId = customerId,
                        memberNumber = memberNumber,
                        initiatedTransactionId = initiatedTransactionId,
                        cancellationReason = cancellationReason,
                        initiatedBy = initiatedBy,
                        transactionSource = "PAUSER",
                        cancelTransactionDetails = cancelTransactionDetails
                    }
                }
            };


            return JsonConvert.SerializeObject(graphRequest);
        }

    }
}
