using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Factory
{
    public static class LoopsFrontEndAPIFactory
    {

        #region  manual redemptions

        public static string CreateManualRedemption(string partnerCode, string productName, string activityDate, string points, string comments, string customerId, string pnr = "", string ticketNumber = "", string originAirportCode = "", string destinationAirportCode = "", string bookingClass = "", int noOfPassengers = 0, int noOfSegments = 0, string voucherNumber = "", string voucherUsage = "")
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\CreateManualRedemption.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["partnerCode"] = partnerCode;
            payloadObject["productName"] = productName;
            payloadObject["activityDate"] = activityDate;
            payloadObject["points"] = points;
            payloadObject["comments"] = comments;
            payloadObject["customerId"] = customerId;
            payloadObject["pnr"] = pnr;
            payloadObject["ticketNo"] = ticketNumber;
            payloadObject["originationAirport"] = originAirportCode;
            payloadObject["destinationAirport"] = destinationAirportCode;
            payloadObject["bookingClass"] = bookingClass;
            payloadObject["noPassengers"] = noOfPassengers;
            payloadObject["noSegments"] = noOfSegments;
            payloadObject["voucherNumber"] = voucherNumber;
            payloadObject["voucherUsage"] = voucherUsage;


            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion


        #region Customer API 

        public static string AwardVoucher(string voucherName, string startDate, string endDate, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\AwardVoucher.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["voucherName"] = voucherName;
            payloadObject["startDate"] = startDate;
            payloadObject["endDate"] = endDate;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["createdByUserName"] = createdByUserName;


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RedeemVoucher(string voucherId, string voucherNumber, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\RedeemVoucher.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["voucherID"] = voucherId;
            payloadObject["voucherNumber"] = voucherNumber;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["createdByUserName"] = createdByUserName;


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateVoucher(string voucherNumber, string voucherId, string startDate, string expiryDate, string status, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\UpdateVoucher.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["voucherNumber"] = voucherNumber;
            payloadObject["voucherID"] = voucherId;
            payloadObject["startDate"] = startDate;
            payloadObject["expiryDate"] = expiryDate;
            payloadObject["status"] = status;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["createdByUserName"] = createdByUserName;


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreateFlightAccrual(string bookingReference, string bookingClass, string activityDate, string airlineCode, string ticketIssueDate, string originAirport, string flightNumber, string destinationAirport, string memberNumber, string customerId, string createdOn, string comments, string ticketNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\CreateFlightAccrual.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["bookingReference"] = bookingReference;
            payloadObject["bookingClass"] = bookingClass;
            payloadObject["activityDate"] = activityDate;
            payloadObject["airlineCode"] = airlineCode;
            payloadObject["ticketIssueDate"] = ticketIssueDate;
            payloadObject["originAirport"] = originAirport;
            payloadObject["flightNumber"] = flightNumber;
            payloadObject["destinationAirport"] = destinationAirport;
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["customerId"] = customerId;
            payloadObject["createdOn"] = createdOn;
            payloadObject["comments"] = comments;
            payloadObject["ticketNumber"] = ticketNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string TransferVoucher(string senderAccountNumber, string recipientAccountNumber, string recipientCustomerId, string voucherNumber, string voucherId, string voucherName, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\TransferVoucher.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["senderAccountNo"] = senderAccountNumber;
            payloadObject["recipientAccountNo"] = recipientAccountNumber;
            payloadObject["recipientCustomerID"] = recipientCustomerId;
            payloadObject["transactionID"] = "";
            payloadObject["voucherNumber"] = voucherNumber;
            payloadObject["voucherID"] = voucherId;
            payloadObject["voucherName"] = voucherName;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["createdByUserName"] = createdByUserName;


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string PointsTransfer(string senderAccountNumber, string recipientAccountNumber, string recipientCustomerId, int points, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\PointsTransfer.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["senderAccount"] = senderAccountNumber;
            payloadObject["recipientAccount"] = recipientAccountNumber;
            payloadObject["recipientCustomerID"] = recipientCustomerId;
            payloadObject["points"] = points;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["createdByUserName"] = createdByUserName;


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string ManualCreditAdjustment(string partnerCode, string productName, string activityDate, string comments, int points, int tierPoints, string transactionSource, string createdByEmailId, string createdByUserName, string customerId, string bookingReference = "")
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\ManualCreditAdjustment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["partnerCode"] = partnerCode;
            payloadObject["productName"] = productName;
            payloadObject["activityDate"] = activityDate;
            payloadObject["comments"] = comments;
            payloadObject["points"] = points;
            payloadObject["tierPoints"] = tierPoints;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByUserName"] = createdByUserName;
            payloadObject["customerId"] = customerId;
            payloadObject["pnr"] = bookingReference;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreateNewMemberTier(string tierName, string tierStartDate,
                   string tierEndDate, string revision, string createdBy, string issueReason)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\CreateNewTier.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["tier"] = tierName;
            payloadObject["startDate"] = tierStartDate;
            payloadObject["endDate"] = tierEndDate;
            payloadObject["revision"] = revision;
            payloadObject["userCreated"] = createdBy;
            payloadObject["issueReason"] = issueReason;


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateNewMemberTier(string docId, string tierName, string tierApprovalStatus,
                    string revision, string updatedBy)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\CreateNewTier.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["revision"] = revision;
            payloadObject["tierDocId"] = docId;
            payloadObject["userUpdated"] = updatedBy;
            payloadObject["manualTierChangeFlag"] = "Y";
            payloadObject["tierApprovalStatus"] = tierApprovalStatus;
            payloadObject["tierName"] = tierName;


            return JsonConvert.SerializeObject(payloadObject);
        }


        public static string UpdateMemberPhone(string operationIdentifier, string customerId, string memberNumber, string phoneId, string phoneNumber, string updatedBy, string status)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\ProfileUpdate.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["LastUpdatedBy"] = updatedBy;
            payloadObject["OperationIdentifier"] = operationIdentifier;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["MemberNumber"] = memberNumber;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["CustomerID"] = customerId;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["Status"] = status;

            JToken phoneUpdatedPayload = payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfAlternatePhone"]["AlternatePhone"][0];

            phoneUpdatedPayload["PhoneId"] = phoneId;
            phoneUpdatedPayload["PhoneNumber"] = phoneNumber;
            phoneUpdatedPayload["Primary"] = "Y";
            phoneUpdatedPayload["PhoneType"] = "Mobile";
            phoneUpdatedPayload["Description"] = "Updated from Script";
            phoneUpdatedPayload["VAPhoneCountryCode"] = "44";
            phoneUpdatedPayload["Valid"] = "Y";
            phoneUpdatedPayload["Usage"] = "Personal";


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateMemberEmail(string operationIdentifier, string customerId, string memberNumber, string emailId, string emailAddress, string updatedBy, string updatedOn)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\ProfileUpdate.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["LastUpdatedBy"] = updatedBy;
            payloadObject["OperationIdentifier"] = operationIdentifier;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["MemberNumber"] = memberNumber;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["CustomerID"] = customerId;

            JToken emailUpdatedPayload = payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfCommunicationAddress"]["CommunicationAddress"];

            emailUpdatedPayload["EmailId"] = emailId;
            emailUpdatedPayload["Primary"] = "Y";
            emailUpdatedPayload["UseType"] = "Business";
            emailUpdatedPayload["EmailAddress"] = emailAddress;
            emailUpdatedPayload["LastUpdated"] = updatedOn;
            emailUpdatedPayload["Description"] = "Updated from Script";
            emailUpdatedPayload["LastUpdatedSource"] = "Automation Script";
            emailUpdatedPayload["Valid"] = "Y";

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateMemberAddress(string operationIdentifier, string customerId, string memberNumber, string updatedBy, string addressId, string addressLine1, string addressLine2, string city, string country)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\ProfileUpdate.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["LastUpdatedBy"] = updatedBy;
            payloadObject["OperationIdentifier"] = operationIdentifier;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["MemberNumber"] = memberNumber;
            payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["CustomerID"] = customerId;

            JToken addressUpdatedPayload = payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"];

            addressUpdatedPayload["AddressId"] = addressId;
            addressUpdatedPayload["AddressLine1"] = addressLine1;
            addressUpdatedPayload["AddressLine2"] = addressLine2;
            addressUpdatedPayload["City"] = city;
            addressUpdatedPayload["Country"] = country;
            addressUpdatedPayload["PostCode"] = "RH10 7DU";
            addressUpdatedPayload["Primary"] = "Y";
            addressUpdatedPayload["AddressType"] = "Home";
            addressUpdatedPayload["Region"] = "West Sussex";
            addressUpdatedPayload["Territory"] = "GB";
            addressUpdatedPayload["ValidFlag"] = "Y";


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string AddOrUpdateAwardToTheMember(string operationIdentifier, string customerId, string memberNumber, string updatedBy, string status,
            string vaPrefCardName, string dateOfBirth, string firstName, string lastName, string title, string formalSalutation, string awardProductName, string startDate)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\ProfileUpdate.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["LastUpdatedBy"] = updatedBy;
            payloadObject["OperationIdentifier"] = operationIdentifier;

            JToken profileUpdatePayload = payloadObject["ListOfVAMemberUpdate"]["LOYMember"];

            profileUpdatePayload["MemberNumber"] = memberNumber;
            profileUpdatePayload["CustomerID"] = customerId;

            profileUpdatePayload["VATermsandConditions"] = "Y";
            profileUpdatePayload["Status"] = status;
            profileUpdatePayload["VAPreferedCardName"] = vaPrefCardName;
            profileUpdatePayload["VATestFlag"] = "False";
            profileUpdatePayload["ListOfContact"]["Contact"]["DateOfBirth"] = dateOfBirth;
            profileUpdatePayload["ListOfContact"]["Contact"]["FirstName"] = firstName;
            profileUpdatePayload["ListOfContact"]["Contact"]["LastName"] = lastName;
            profileUpdatePayload["ListOfContact"]["Contact"]["Tile"] = title;
            profileUpdatePayload["ListOfContact"]["Contact"]["FormalSalutation"] = formalSalutation;
            profileUpdatePayload["ListOfContact"]["Contact"]["InformalSalutation"] = formalSalutation;
            profileUpdatePayload["ListOfContact"]["Contact"]["EnvelopeSalutation"] = formalSalutation;

            profileUpdatePayload["ListOfAwards"][0]["ProductName"] = awardProductName;
            profileUpdatePayload["ListOfAwards"][0]["StartDate"] = startDate;


            return JsonConvert.SerializeObject(payloadObject);

        }

        public static string CancellationTransaction(string customerId, string transactionId, int transactionID_AWS, string memberNumber, string activityDate, string transactionProcessedDate, string totalMiles, string points)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\TransactionCancellation.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["customerId"] = customerId;
            payloadObject["transactionID"] = transactionId;
            payloadObject["activityType"] = "Accrual";
            payloadObject["cancellationReason"] = "Canx Agent Error";
            payloadObject["cancellationComments"] = "";
            payloadObject["transactionID_AWS"] = transactionID_AWS;
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["owner"] = "EAIUSER";
            payloadObject["transactionStatus"] = "Processed";
            payloadObject["activityDate"] = activityDate;
            payloadObject["transactionProcessedDate"] = transactionProcessedDate;
            payloadObject["partnerName"] = "Virgin Atlantic Airways";
            payloadObject["partnerCode"] = "VS";
            payloadObject["ptnrNumber"] = null;
            payloadObject["operatingFlightstring"] = "";
            payloadObject["bookingClass"] = "";
            payloadObject["originationAirport"] = "";
            payloadObject["destinationAirport"] = "";
            payloadObject["bookingReference"] = "";
            payloadObject["baseMiles"] = "";
            payloadObject["totalMiles"] = totalMiles;
            payloadObject["points"] = points;
            payloadObject["tierPoints"] = "0";
            payloadObject["statementDescription"] = "Virgin Flying Club Manual Adjustment";
            payloadObject["cabinClass"] = "";
            payloadObject["productName"] = "1-1ZWY5ZT";
            payloadObject["marketingCarrierCode"] = null;
            payloadObject["marketingFlightstring"] = null;
            payloadObject["marketingBookingClass"] = "";
            payloadObject["comments"] = null;
            payloadObject["ticketstring"] = "";
            payloadObject["standardMiles"] = null;
            payloadObject["programmeBonusMiles"] = null;
            payloadObject["retailerBonusMiles"] = "";
            payloadObject["serviceRecoveryMiles"] = null;
            payloadObject["voucherstring"] = null;
            payloadObject["voucherType"] = null;
            payloadObject["transactionChannel"] = "PAUSER";
            payloadObject["operatingAuthstring"] = null;
            payloadObject["canxBy"] = "";
            payloadObject["retroOverrideFlag"] = "N";
            payloadObject["PDMFileType"] = "";
            payloadObject["retroOverrideReason"] = null;
            payloadObject["ticketIssueDate"] = "";
            payloadObject["transactionSubType"] = "Product";
            payloadObject["processingLog"] = null;
            payloadObject["transactionSource"] = "scrTransactionDetail";
            payloadObject["createdByEmailId"] = "automation@test.com";
            payloadObject["createdByUserName"] = "Automation Script";


            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string MemberEnrolment(string title, string gender, string firstName, string lastName, string emailId, string addressType, string addressLine1, string city, string postCode, string country, string dateOfBirth, string phoneCountryCode, string phoneNumber, string guardianName, string guardianDateOfBirth, string guardianPhoneCountryCode, string guardianPhoneNumber, string guardianEmailId)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoopsFrontendAPI\\MemberEnrolment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["customerDetails"]["firstName"] = firstName;
            payloadObject["customerDetails"]["lastName"] = lastName;
            payloadObject["customerDetails"]["gender"] = gender;
            payloadObject["customerDetails"]["title"] = title;
            payloadObject["customerDetails"]["dateOfBirth"] = dateOfBirth;
            payloadObject["customerDetails"]["addressDetails"][0]["addressType"] = addressType;
            payloadObject["customerDetails"]["addressDetails"][0]["addressLine1"] = addressLine1;
            payloadObject["customerDetails"]["addressDetails"][0]["city"] = city;
            payloadObject["customerDetails"]["addressDetails"][0]["postalCode"] = postCode;
            payloadObject["customerDetails"]["addressDetails"][0]["country"] = country;
            payloadObject["customerDetails"]["contactDetails"]["emailDetails"][0]["emailId"] = emailId;
            payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0]["countryCode"] = phoneCountryCode;
            payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0]["phoneNumber"] = phoneNumber;
            payloadObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["name"] = guardianName;
            payloadObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["dateOfBirth"] = guardianDateOfBirth;
            payloadObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["phoneDetails"]["countryCode"] = guardianPhoneCountryCode;
            payloadObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["phoneDetails"]["phoneNumber"] = guardianPhoneNumber;
            payloadObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["emailDetails"]["emailId"] = guardianEmailId;

            return JsonConvert.SerializeObject(payloadObject);
        }
        #endregion
    }
}
