using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Factory
{
    public static class LoopsBackendAPIFactory
    {
        public static class Customer
        {

            public static string CreateMemberTier(string updatedOn, string tierStartDate,
                        string tierEndDate, int revision, string tierName, string appliedRuleType)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\CustomerAPI\\CreateMemberTier.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["updated"] = updatedOn;
                payloadObject["tierStartDate"] = tierStartDate;
                payloadObject["tierEndDate"] = tierEndDate;
                payloadObject["revision"] = revision;
                payloadObject["tierName"] = tierName;
                payloadObject["appliedRuleType"] = appliedRuleType;

                return JsonConvert.SerializeObject(payloadObject);
            }


            public static string UpdateMemberTier(string docId, string updatedOn, string tierStartDate,
                        string tierEndDate, int revision)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\CustomerAPI\\UpdateMemberTier.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);


                payloadObject["DocId"] = docId;

                JToken tierDetails = payloadObject["TierDetails"];

                tierDetails["UpdatedOn"] = updatedOn;
                tierDetails["TierStartDate"] = tierStartDate;
                tierDetails["TierEndDate"] = tierEndDate;
                tierDetails["Revision"] = revision;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string UpdateMemberAddress(string addressLine1, string addressLine2, string addressLine3,
                       string company, string city, string startDate, string postalCode, string region, string country, string lastUpdatedBy)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\CustomerAPI\\MemberAddressUpdate.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["addressLine1"] = addressLine1;
                payloadObject["addressLine2"] = addressLine2;
                payloadObject["addressLine3"] = addressLine3;
                payloadObject["companyName"] = company;
                payloadObject["city"] = city;
                payloadObject["startDate"] = startDate;
                payloadObject["created"] = startDate;
                payloadObject["postalCode"] = postalCode;
                payloadObject["region"] = region;
                payloadObject["country"] = country;
                payloadObject["lastUpdatedBy"] = lastUpdatedBy;

                return JsonConvert.SerializeObject(payloadObject);
            }


            public static string UpdateMemberPreference(string fc1Value, string doNotCallValue = "", string doNotSMSValue = "", string doNotEmailValue = "", string preferredPartnerFFP = "", string updatedBy = "")
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\CustomerAPI\\MemberPreferenceUpdate.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["preferences"][0]["preferenceValue"] = fc1Value;

                payloadObject["preferences"][1]["preferenceValue"] = doNotCallValue;

                payloadObject["preferences"][2]["preferenceValue"] = doNotSMSValue;

                payloadObject["preferences"][3]["preferenceValue"] = doNotEmailValue;

                payloadObject["preferences"].FirstOrDefault(x => (string)x["preferenceType"] == "PreferredPartnerFFP")["preferenceValue"] = preferredPartnerFFP;

                if (!string.IsNullOrEmpty(updatedBy))
                    foreach (JToken key in payloadObject["preferences"])
                        key["lastUpdatedBy"] = updatedBy;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string UpdateMemberEmail(string emailId, string lastUpdatedBy = "")
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\CustomerAPI\\MemberEmailUpdate.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["emailAddress"] = emailId;
                payloadObject["lastUpdatedSource"] = lastUpdatedBy;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string UpdateMemberPhone(string phoneNumber, string startDate, string lastUpdatedBy = "")
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\CustomerAPI\\MemberPhoneUpdate.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["phoneNumber"] = phoneNumber;
                payloadObject["startDate"] = startDate;
                payloadObject["lastUpdatedBy"] = lastUpdatedBy;

                return JsonConvert.SerializeObject(payloadObject);
            }

        }

        public static class Transaction
        {

            public static string CreateAccrualTransaction(string bookingReference, string activityDate, string ticketIssueDate)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\TransactionAPI\\CreateAccrualTransaction.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["activityDate"] = activityDate;
                payloadObject["flightDetail"]["bookingReference"] = bookingReference;
                payloadObject["flightDetail"]["ticketIssueDate"] = ticketIssueDate;
                payloadObject["flightDetail"]["departureDate"] = activityDate;
                payloadObject["passengers"][0]["flightSegments"][0]["departureDate"] = activityDate;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string CreateManualAdjustmentTransaction(string activityDate)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\TransactionAPI\\CreateManualAdjustmentTransaction.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["activityDate"] = activityDate;

                return JsonConvert.SerializeObject(payloadObject);
            }


            public static string CreateRedemptionTransaction(string bookingReference, string activityDate, string ticketIssueDate)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\TransactionAPI\\CreateRedemptionTransaction.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["activityDate"] = activityDate;
                payloadObject["flightDetail"]["bookingReference"] = bookingReference;
                payloadObject["flightDetail"]["ticketIssueDate"] = ticketIssueDate;
                payloadObject["flightDetail"]["departureDate"] = activityDate;
                payloadObject["passengers"][0]["flightSegments"][0]["departureDate"] = activityDate;

                return JsonConvert.SerializeObject(payloadObject);
            }

        }

        public static class Award
        {

            public static string CreateAward(string voucherName, string startDate, string endDate, string createdBy = "")
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\AwardAPI\\CreateAward.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["voucherDetails"]["voucherName"] = voucherName;
                payloadObject["voucherDetails"]["startDate"] = startDate;
                payloadObject["voucherDetails"]["expiryDate"] = endDate;
                payloadObject["createdByEmailId"] = createdBy;


                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string GiftAward(string recipientMemberNumber, string recipientCustomerId, string giftedBy = "")
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\AwardAPI\\GiftAward.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["recipientAccountNo"] = recipientMemberNumber;
                payloadObject["recipientCustomerID"] = recipientCustomerId;
                payloadObject["createdByEmailId"] = giftedBy;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string UpdateAward(string status, string startDate, string endDate, string usedDate, string updatedBy = "")
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\AwardAPI\\UpdateAward.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["status"] = status;
                payloadObject["startDate"] = startDate;
                payloadObject["expiryDate"] = endDate;
                payloadObject["usedDate"] = usedDate;
                payloadObject["createdByEmailId"] = updatedBy;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string RedeemAward()
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\AwardAPI\\RedeemAward.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                return JsonConvert.SerializeObject(payloadObject);
            }

        }
        public static class FinancialPartnerProcessing
        {
            public static string VoucherAward(string memberNumber, string voucherType, string cardLinkPartnerUserId, string voucherActivityDate, string cardOpenDate)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\FinancialPartnerProcessingAPI\\VoucherAward.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["manifest"]["partnerUserId"] = memberNumber;
                payloadObject["manifest"]["cardLinkPartnerUserId"] = cardLinkPartnerUserId;
                payloadObject["manifest"]["cardLinkPartnerUserIdDomain"] = voucherType;
                payloadObject["manifest"]["voucherActivityDate"] = voucherActivityDate;
                payloadObject["manifest"]["cardOpenDate"] = cardOpenDate;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string VoucherAwardVMCC(string memberNumber, string cardLinkPartnerUserId, string cardLinkPartnerUserIdDomain, string voucherActivityDate, string cardOpenDate, string activityCode)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\FinancialPartnerProcessingAPI\\VoucherAwardVMCC.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["manifest"]["partnerUserId"] = memberNumber;
                payloadObject["manifest"]["cardLinkPartnerUserId"] = cardLinkPartnerUserId;
                payloadObject["manifest"]["cardLinkPartnerUserIdDomain"] = cardLinkPartnerUserIdDomain;
                payloadObject["manifest"]["voucherActivityDate"] = voucherActivityDate;
                payloadObject["manifest"]["cardOpenDate"] = cardOpenDate;
                payloadObject["manifest"]["activityCode"] = activityCode;

                return JsonConvert.SerializeObject(payloadObject);
            }
            public static string UsCoBrandVoucherTierpointsTxn(string transactionId, string memberNumber, string transactionActivityDate, string cardLinkPartnerUserIdDomain, string cardLinkPartnerUserId, string monthlySpendAmount)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\FinancialPartnerProcessingAPI\\USCoBrandVoucherTierpoints.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["manifest"]["transactionId"] = transactionId;
                payloadObject["manifest"]["partnerUserId"] = memberNumber;
                payloadObject["manifest"]["transactionActivityDate"] = transactionActivityDate;
                payloadObject["manifest"]["cardLinkPartnerUserIdDomain"] = cardLinkPartnerUserIdDomain;
                payloadObject["manifest"]["cardLinkPartnerUserId"] = cardLinkPartnerUserId;
                payloadObject["manifest"]["monthlySpendAmount"] = monthlySpendAmount;

                return JsonConvert.SerializeObject(payloadObject);
            }

            public static string VCCUVoucherTierpointsTxn(string transactionId, string memberNumber, string transactionActivityDate, string cardLinkPartnerUserIdDomain, string cardLinkPartnerUserId, string monthlySpendAmount, string creditCardType)
            {
                string payload = File.ReadAllText("Requests\\Body\\LoopsBackendAPI\\FinancialPartnerProcessingAPI\\VCCUVoucherTierpoints.json");

                JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

                payloadObject["manifest"]["transactionId"] = transactionId;
                payloadObject["manifest"]["partnerUserId"] = memberNumber;
                payloadObject["manifest"]["transactionActivityDate"] = transactionActivityDate;
                payloadObject["manifest"]["cardLinkPartnerUserIdDomain"] = cardLinkPartnerUserIdDomain;
                payloadObject["manifest"]["cardLinkPartnerUserId"] = cardLinkPartnerUserId;
                payloadObject["manifest"]["monthlySpendAmount"] = monthlySpendAmount;
                payloadObject["manifest"]["creditCardType"] = creditCardType;

                return JsonConvert.SerializeObject(payloadObject);
            }
        }
    }
}
