using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Factory
{
    public static class Microservice
    {
        public static string CreateTransaction(string customerId, string memberNumber, string partnerCode, string productName, string activityDate, string comments, int tierPoints, int points, string productDescription, string partnerName, string partnerCategory, string productCategory, string transactionSource, string createdBy, string transactionChannel, string statementText, string activityType)
        {

            string payload = File.ReadAllText("Requests\\Body\\VRed\\CreateTransaction.json");

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

        public static string CancelTransaction(string memberNumber, string transactionId, string activityDate, string transactionChannel, string cancellationReason, string updatedBy)
        {

            string payload = File.ReadAllText("Requests\\Body\\VRed\\CancelTransaction.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["transactionId"] = transactionId;
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["transactionchannel"] = transactionChannel;
            payloadObject["cancellationReason"] = cancellationReason;
            payloadObject["activityDate"] = activityDate;
            payloadObject["updatedBy"] = updatedBy;

            return JsonConvert.SerializeObject(payloadObject);
        }


        #region PartnerMembership Document payloads

        public static string CreatePartnerMembership(string customerId, string memberNumber, string startDate, string partnerStatusCode, string partnerId)
        {
            string payload = File.ReadAllText("Requests\\Body\\LoyaltyMemberCards\\CreatePartnerMembership.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["PartnerMembershipDocument"];
            variables["customerId"] = customerId;
            variables["memberNumber"] = memberNumber;
            variables["startDate"] = startDate;
            variables["partnerStatusCode"] = partnerStatusCode;
            variables["partnerId"] = partnerId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string CreateAward(string customerId, string createdBy, string awardNumber, string productId, string startDate, string expiryDate, string status)
        {
            string payload = File.ReadAllText("Requests\\Body\\CustomerTransactions\\CreateAward.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["award"];
            variables["customerId"] = customerId;
            variables["createdBy"] = createdBy;
            variables["awardNumber"] = awardNumber;
            variables["productId"] = productId;
            variables["startDate"] = startDate;
            variables["expiryDate"] = expiryDate;
            variables["status"] = status;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateAward(string customerId, string id, string lastUpdatedBy, string status, string productId)
        {
            string payload = File.ReadAllText("Requests\\Body\\CustomerTransactions\\UpdateAward.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["productId"] = productId;

            JToken variables = payloadObject["award"];
            variables["customerId"] = customerId;
            variables["id"] = id;
            variables["lastUpdatedBy"] = lastUpdatedBy;
            variables["status"] = status;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateMemberAddress(string customerId, string currentAddressType, string newAddressLine1, string currentAddressLine2, string currentAddressLine3, string currentAddressVerified, string currentLandlordOrCompanyName, string currentCity, string currentPostalCode, string currentRegion, string currentCountry, string currentPreferredFlag)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\UpdateMemberAddress.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["newAddressDetails"];
            variables["AddressType"] = currentAddressType;
            variables["AddressLine1"] = newAddressLine1;
            variables["AddressLine2"] = currentAddressLine2;
            variables["AddressLine3"] = currentAddressLine3;
            variables["AddressVerified"] = currentAddressVerified == "Y" ? true : false;
            variables["LandlordOrCompanyName"] = currentLandlordOrCompanyName;
            variables["City"] = currentCity;
            variables["PostalCode"] = currentPostalCode;
            variables["Region"] = currentRegion;
            variables["Country"] = currentCountry;
            variables["PreferredFlag"] = currentPreferredFlag;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdateMemberEmail(string newEmailAddress)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\UpdateMemberEmail.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["NewEmailAddress"] = newEmailAddress;

            return JsonConvert.SerializeObject(payloadObject);
        }


        public static string UpdateMemberPhone(string customerId, string fcNumber, string phoneNumber, string countryCode)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\UpdateMemberPhone.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["customerId"] = customerId;
            payloadObject["fcNumber"] = fcNumber;
            payloadObject["phoneNumber"] = phoneNumber;
            payloadObject["countryCode"] = countryCode;

            return JsonConvert.SerializeObject(payloadObject);
        }

        #endregion

        public static string CreateVAEnrolment(string firstName, string lastName, string middleName, string phoneNumber, string customerType, string doB, string emailId)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\VAEnrolment.json");

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

        public static string UpdateVACMemberProfile(string customerId, string memberNumber, string firstName, string lastName, string doB, string emailAddress, string phoneNumber)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\UpdateVACMemberProfile.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["CustomerId"] = customerId;
            payloadObject["LoyaltyNumber"] = memberNumber;
            payloadObject["FirstName"] = firstName;
            payloadObject["LastName"] = lastName;
            payloadObject["DateOfBirth"] = doB;
            payloadObject["EmailId"] = emailAddress;
            payloadObject["PhoneNumber"] = phoneNumber;

            return JsonConvert.SerializeObject(payloadObject);

        }

        public static string UpgradeToFlyingClub(string firstName, string lastName, string doB, string phoneNumber, string emailAddress, string customerId, string memberNumber, string enrollementChannel, string channelUserId)
        {
            string payload = File.ReadAllText("Requests\\Body\\Customer\\VACUpgradeToFlyingClub.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["customerDetails"];

            variables["channelUserId"] = channelUserId;
            variables["enrollmentChannel"] = enrollementChannel;
            variables["firstName"] = firstName;
            variables["lastName"] = lastName;
            variables["dateOfBirth"] = doB;
            variables["customerId"] = customerId;
            variables["fcNumber"] = memberNumber;
            variables["memberNumber"] = memberNumber;

            JToken addressDetails = payloadObject["customerDetails"]["addressDetails"][0];

            addressDetails["addressLine1"] = "104 Test address2";
            addressDetails["addressLine2"] = "Home Locality";
            addressDetails["addressLine3"] = "";
            addressDetails["companyName"] = "VAA";
            addressDetails["city"] = "Colchester";
            addressDetails["postalCode"] = "CO12 4ZZZ";
            addressDetails["region"] = "Essex";
            addressDetails["countryCode"] = "GB";
            addressDetails["preferredFlag"] = "Y";
            addressDetails["addressType"] = "Work";

            variables["password"] = Application.LogInPassword;


            JToken phoneNum = payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0];
            phoneNum["phoneNumber"] = phoneNumber;

            JToken emailNum = payloadObject["customerDetails"]["contactDetails"]["emailDetails"][0];
            emailNum["emailId"] = emailAddress;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RedeemVoucher(string customerId, string memberNumber, string redemptionType, string activityDate, string bookingClass, string originAirport, string destinationAirport, string bookingReference, string voucherNumber, string voucherName, int points)
        {

            string payload = File.ReadAllText("Requests\\Body\\VoucherSelfService\\RedeemVoucher.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["customerId"] = customerId;
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["redemptionType"] = redemptionType;
            payloadObject["flightDetails"]["activityDate"] = activityDate;
            payloadObject["flightDetails"]["bookingClass"] = bookingClass;
            payloadObject["flightDetails"]["originAirport"] = originAirport;
            payloadObject["flightDetails"]["destinationAirport"] = destinationAirport;
            payloadObject["flightDetails"]["bookingReference"] = bookingReference;
            payloadObject["voucherDetails"]["voucherNumber"] = voucherNumber;
            payloadObject["voucherDetails"]["voucherName"] = voucherName;
            payloadObject["voucherDetails"]["pointsToAward"] = points;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string VoucherRollback(string customerId, string memberNumber, string voucherNumber)
        {

            string payload = File.ReadAllText(Path.Combine("Requests", "Body", "VoucherSelfService", "VoucherRollback.json"));

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["customerId"] = customerId;
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["voucherNumber"] = voucherNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdatePnr(string customerId, string memberNumber, string voucherNumber, string pNR)
        {

            string payload = File.ReadAllText(Path.Combine("Requests", "Body", "VoucherSelfService", "UpdatePNR.json"));

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["customerId"] = customerId;
            payloadObject["memberNumber"] = memberNumber;
            payloadObject["voucherNumbers"] = new JArray(voucherNumber);
            payloadObject["pNR"] = pNR;

            return JsonConvert.SerializeObject(payloadObject);
        }

    }

}
