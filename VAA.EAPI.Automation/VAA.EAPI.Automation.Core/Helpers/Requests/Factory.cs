using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Core.Helpers.Requests
{
    public static class Factory
    {
        public static string CreateMemberTier(string updatedOn, string tierStartDate, string tierEndDate, int revision, string tierName, string appliedRuleType)
        {
            string filePath = @".\Requests\Body\LoopsCustomerAPI\CreateMemberTier.json";

            string payload = File.ReadAllText(filePath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["updated"] = updatedOn;
            payloadObject["tierStartDate"] = tierStartDate;
            payloadObject["tierEndDate"] = tierEndDate;
            payloadObject["revision"] = revision;
            payloadObject["tierName"] = tierName;
            payloadObject["appliedRuleType"] = appliedRuleType;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateMemberTier(string docId, string updatedOn, string tierStartDate, string tierEndDate, int revision)
        {
            string filePath = @".\Requests\Body\LoopsCustomerAPI\UpdateMemberTier.json";

            string payload = File.ReadAllText(filePath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["DocId"] = docId;

            JToken tierDetails = payloadObject["TierDetails"];

            tierDetails["UpdatedOn"] = updatedOn;
            tierDetails["TierStartDate"] = tierStartDate;
            tierDetails["TierEndDate"] = tierEndDate;
            tierDetails["Revision"] = revision;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreateTransaction(string customerId, string memberNumber, string partnerCode, string productName, string activityDate, string comments, int tierPoints, int points, string productDescription, string partnerName, string partnerCategory, string productCategory, string transactionSource, string createdBy, string transactionChannel, string statementText, string activityType)
        {

            string payloadPath = Path.Combine("Requests", "Body", "Vred", "CreateTransaction.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            DateTime baseDate = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            payloadObject["customerId"] = customerId;
            payloadObject["transactionId"] = $"AZ{(long)(DateTime.UtcNow.ToUniversalTime() - baseDate).TotalMilliseconds}";
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["partnerCode"] = partnerCode;
            payloadObject["productName"] = productName;
            payloadObject["activityDate"] = activityDate;
            payloadObject["comments"] = comments;
            payloadObject["tierPoints"] = tierPoints;
            payloadObject["points"] = points;
            payloadObject["productDescription"] = productDescription;
            payloadObject["partnerName"] = partnerName;
            payloadObject["partnerCategory"] = partnerCategory;
            payloadObject["productCategory"] = productCategory;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdBy"] = createdBy;
            payloadObject["transactionChannel"] = transactionChannel;
            payloadObject["activityType"] = activityType;
            payloadObject["statementText"] = statementText;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreateVAEnrolment(string firstName, string lastName, string middleName, string phoneNumber, string customerType, string doB, string emailId)
        {
            string payload = File.ReadAllText("Requests\\Body\\Vred\\VAEnrolment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["customerDetails"];

            variables["firstName"] = firstName;
            variables["lastName"] = lastName;
            variables["middleName"] = middleName;
            variables["customerType"] = customerType;
            variables["dateOfBirth"] = doB;

            JToken phoneUpdate = payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0];
            phoneUpdate["phoneNumber"] = phoneNumber;

            JToken emailUpdate = payloadObject["customerDetails"]["contactDetails"]["emailDetails"][0];
            emailUpdate["emailId"] = emailId;

            return JsonConvert.SerializeObject(payloadObject);
        }


        public static string UpdateFCMemberStatus(string memberNumber, string customerId, string firstName, string lastName, string dob, string status, string title)
        {
            string filePath = @".\Requests\Body\LoopsFrontendAPI\StatusUpdate.json";

            string payload = File.ReadAllText(filePath);

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken memberDetails = payloadObject["ListOfVAMemberUpdate"]["LOYMember"];

            memberDetails["MemberNumber"] = memberNumber;
            memberDetails["CustomerID"] = customerId;
            memberDetails["Status"] = status;
            memberDetails["VAPreferedCardName"] = $"{firstName} {lastName}";

            JToken contactDetails = payloadObject["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"];

            contactDetails["DateOfBirth"] = dob;
            contactDetails["FirstName"] = firstName;
            contactDetails["LastName"] = lastName;
            contactDetails["Title"] = title;
            contactDetails["FormalSalutation"] = $"Dear {title} {lastName}";
            contactDetails["InformalSalutation"] = $"Dear {firstName}";
            contactDetails["EnvelopeSalutation"] = $"{title} {firstName} {lastName}";
            contactDetails["PnrName"] = $"{lastName.ToUpper()}/{firstName.ToUpper()}{title.ToUpper()}";

            return JsonConvert.SerializeObject(payloadObject);

        }

        public static string CreateFlightAccrual(string bookingReference, string bookingClass, string activityDate, string airlineCode, string ticketIssueDate, string originAirport, string flightNumber, string destinationAirport, string memberNumber, string customerId, string createdOn, string comments, string ticketNumber)
        {
            string payload = File.ReadAllText(@".\Requests\Body\LoopsFrontendAPI\CreateFlightAccrual.json");

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

        public static string AddOrUpdateAwardToTheMember(string operationIdentifier, string customerId, string memberNumber, string updatedBy, string status,
         string vaPrefCardName, string dateOfBirth, string firstName, string lastName, string title, string formalSalutation, string awardProductName, string startDate)
        {
            string payload = File.ReadAllText(@".\Requests\Body\LoopsFrontendAPI\ProfileUpdate.json");

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
            profileUpdatePayload["ListOfContact"]["Contact"]["Title"] = title;
            profileUpdatePayload["ListOfContact"]["Contact"]["FormalSalutation"] = formalSalutation;
            profileUpdatePayload["ListOfContact"]["Contact"]["InformalSalutation"] = formalSalutation;
            profileUpdatePayload["ListOfContact"]["Contact"]["EnvelopeSalutation"] = formalSalutation;

            profileUpdatePayload["ListOfAwards"][0]["ProductName"] = awardProductName;
            profileUpdatePayload["ListOfAwards"][0]["StartDate"] = startDate;


            return JsonConvert.SerializeObject(payloadObject);

        }

        public static string AwardVoucher(string voucherName, string startDate, string expiryDate, string transactionSource, string createdByEmailId, string createdByUserName)
        {
            string payloadPath = Path.Combine("Requests", "Body", "LoopsFrontendAPI", "AwardVoucher.json");
            string payload = File.ReadAllText(payloadPath);

            JObject payloadObject = JObject.Parse(payload);

            payloadObject["voucherName"] = voucherName;
            payloadObject["startDate"] = startDate;
            payloadObject["expiryDate"] = expiryDate;
            payloadObject["transactionSource"] = transactionSource;
            payloadObject["createdByEmailId"] = createdByEmailId;
            payloadObject["createdByUserName"] = createdByUserName;


            return JsonConvert.SerializeObject(payloadObject);
        }


    }
}
