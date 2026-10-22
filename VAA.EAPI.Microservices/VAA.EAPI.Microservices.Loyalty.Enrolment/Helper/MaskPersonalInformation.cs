using Newtonsoft.Json.Linq;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class MaskPersonalInformation
    {
        public static string MASKPIData(string JSOnRequest)
        {
            string maskedJSON = JSOnRequest;
            try
            {
                var jObject = JObject.Parse(JSOnRequest);
                if (!string.IsNullOrEmpty(jObject["MemberEnrolment_Input"]?["ListOfVAContactIO"]?["Contact"].ToString()))
                {

                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["Password"] = "XXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["DateOfBirth"] = "XXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["Gender"] = "XXXXX";
                }
                if (!string.IsNullOrEmpty(jObject["MemberEnrolment_Input"]?["ListOfVAContactIO"]?["Contact"]?.ToString())) // MemberProfileUpdate and EnrolMember - Password DOB and Sec QnA mask
                {
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["Password"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["DateOfBirth"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["Answer1"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["Answer2"] = "XXXXXX";
                }

                if (!string.IsNullOrEmpty(jObject["MemberEnrolment_Input"]?["ListOfVAContactIO"]?["Contact"]?["ListOfPersonalAddress"]?["PersonalAddress"]?.ToString())) // MemberProfileUpdate and EnrolMember - Address Mask
                {
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"]["PostCode"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"]["AddressLine1"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"]["Country"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"]["City"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"]["Region"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfPersonalAddress"]["PersonalAddress"]["AddressLine2"] = "XXXXXX";

                }
                if (!string.IsNullOrEmpty(jObject["MemberEnrolment_Input"]?["ListOfVAContactIO"]?["Contact"]?["ListOfAlternatePhone"]?.ToString())) // MemberProfileUpdate and EnrolMember - Phone number Mask
                {
                    var JSONAlternatePhoneist = (JArray)jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]?["Contact"]?["ListOfAlternatePhone"];
                    foreach (var phoneData in JSONAlternatePhoneist)
                    {
                        phoneData["PhoneNumber"] = "XXXXXX";
                        phoneData["VAPhoneCountryCode"] = "XXXXXX";
                    }
                }

                if (!string.IsNullOrEmpty(jObject["MemberEnrolment_Input"]?["ListOfVAContactIO"]?["Contact"]?["ListOfCommunicationAddress"]?["CommunicationAddress"]?.ToString())) // MemberProfileUpdate and EnrolMember - Communicate Email Address Mask
                {
                    jObject["MemberEnrolment_Input"]["ListOfVAContactIO"]["Contact"]["ListOfCommunicationAddress"]["CommunicationAddress"]["EmailAddress"] = "XXXXXX";
                }

                if (!string.IsNullOrEmpty(jObject["MemberEnrolment_Input"]?["ListOfVAMemberUpdate"]?["LOYMember"]?["ListOfContact"]?["Contact"]?["ListOfLoyTravelProfile"]?["LOYTravelProfile"]?.ToString()))  //MemberProfileUpdate - Passport data mask
                {
                    jObject["MemberEnrolment_Input"]["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfLoyTravelProfile"]["LOYTravelProfile"]["PassportExpirationDate"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfLoyTravelProfile"]["LOYTravelProfile"]["PassportIssueCountry"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfLoyTravelProfile"]["LOYTravelProfile"]["PassportNumber"] = "XXXXXX";
                    jObject["MemberEnrolment_Input"]["ListOfVAMemberUpdate"]["LOYMember"]["ListOfContact"]["Contact"]["ListOfLoyTravelProfile"]["LOYTravelProfile"]["Citizenship"] = "XXXXXX";
                }
                maskedJSON = jObject.ToString();
            }
            catch (Exception)
            {
                maskedJSON = JSOnRequest;
            }

            return maskedJSON;

        }
        public static string MASKEAPIPIData(string JSOnRequest)
        {
            string maskedJSON = JSOnRequest;
            try
            {
                JObject jObject = JObject.Parse(JSOnRequest);
                if (!string.IsNullOrEmpty(jObject["customerDetails"].ToString()))
                {
                    jObject["customerDetails"]["dateOfBirth"] = "XXXXX";
                }
                if (!string.IsNullOrEmpty(jObject["customerDetails"]?["addressDetails"]?.ToString()))
                {
                    JArray addressDetails = (JArray)jObject["customerDetails"]["addressDetails"];
                    foreach (JToken addressDetail in addressDetails)
                    {
                        addressDetail["postalCode"] = "XXXXXX";
                        addressDetail["country"] = "XXXXXX";
                        addressDetail["addressLine1"] = "XXXXXX";
                        addressDetail["addressLine2"] = "XXXXXX";
                        addressDetail["region"] = "XXXXXX";
                        addressDetail["city"] = "XXXXXX";
                    }
                }
                if (!string.IsNullOrEmpty(jObject["customerDetails"]?["contactDetails"]?["emailDetails"]?.ToString()))
                {
                    JArray emailDetails = (JArray)jObject["customerDetails"]["contactDetails"]["emailDetails"];
                    foreach (JToken emailDetail in emailDetails)
                    {
                        emailDetail["emailId"] = "XXXXXX";
                    }
                }
                if (!string.IsNullOrEmpty(jObject["customerDetails"]?["contactDetails"]?["phoneDetails"]?.ToString()))
                {
                    JArray phoneDetails = (JArray)jObject["customerDetails"]["contactDetails"]["phoneDetails"];
                    foreach (JToken phoneDetail in phoneDetails)
                    {
                        phoneDetail["phoneNumber"] = "XXXXXX";
                        phoneDetail["countryCode"] = "XXXXXX";
                    }
                }

                if (!string.IsNullOrEmpty(jObject["customerDetails"]?["loyaltyMember"]?["guardianDetails"]?.ToString()))
                {

                    jObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["dateOfBirth"] = "XXXXX";
                    if (!string.IsNullOrEmpty(jObject["customerDetails"]?["loyaltyMember"]?["guardianDetails"]?["phoneDetails"]?.ToString()))
                    {
                        jObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["phoneDetails"]["phoneNumber"] = "XXXXXX";
                        jObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["phoneDetails"]["countryCode"] = "XXXXXX";
                    }

                    if (!string.IsNullOrEmpty(jObject["customerDetails"]?["loyaltyMember"]?["guardianDetails"]?["emailDetails"]?.ToString()))
                    {
                        jObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["emailDetails"]["emailId"] = "XXXXXX";
                    }
                }
                maskedJSON = jObject.ToString();
            }
            catch (Exception)
            {
                maskedJSON = JSOnRequest;
            }
            return maskedJSON;
        }

    }
}
