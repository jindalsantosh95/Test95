using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using WebRequest = VAA.EAPI.Automation.Core.WebRequest;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint
{
    public static class Microservice
    {

        private readonly static string _env = Config.Environment;

        private readonly static string orderDataOcpSubKey = Vault.Default.GetSecretValue("Testing-OrderData-Ocp-Apim-SubscriptionKey");

        public static HttpResponseMessage RetrieveAwardAuditHistory(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/LoyaltyAuditManagement/{customerId}/awardAuditHistory"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/LoyaltyAuditManagement/{customerId}/awardAuditHistory"), headers);
        }

        public static HttpResponseMessage RetrieveCustomerProfileAuditHistory(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/LoyaltyAuditManagement/{customerId}/customerProfileAuditHistory"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/LoyaltyAuditManagement/{customerId}/customerProfileAuditHistory"), headers);
        }

        public static HttpResponseMessage RetrievePreferenceAuditHistory(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/LoyaltyAuditManagement/{customerId}/preferenceHistory"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/LoyaltyAuditManagement/{customerId}/preferenceHistory"), headers);
        }

        public static HttpResponseMessage RetrieveSupplementarySilverAccountAuditHistory(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/LoyaltyAuditManagement/{customerId}/SupplementarySilverAuditHistory"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/LoyaltyAuditManagement/{customerId}/SupplementarySilverAuditHistory"), headers);
        }

        public static HttpResponseMessage AssociatedMembersListDownload(string bearer, string ocpSubKey, string campCode)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/campaignmanagement/AssociatedMembersDownload?CampaignCode={campCode}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/AssociatedMembersDownload?CampaignCode={campCode}"), headers);
        }

        public static HttpResponseMessage CampaignsListDownload(string bearer, string ocpSubKey)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/campaignmanagement/CampaignsDownload"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/CampaignsDownload"), headers);
        }

        public static HttpResponseMessage PromotionsListDownload(string bearer, string ocpSubKey)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/campaignmanagement/PromotionsDownload"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/PromotionsDownload"), headers);
        }

        public static HttpResponseMessage CampaignTypesRetrieval(string bearer, string ocpSubKey)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/campaignmanagement/RetrieveCampaignTypes"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/RetrieveCampaignTypes"), headers);
        }

        public static HttpResponseMessage MemberCampaignListRetrieval(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/campaignmanagement/MemberCampaignRetrieval/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/MemberCampaignRetrieval/{customerId}"), headers);
        }

        public static HttpResponseMessage PromotionSearch(string bearer, string ocpSubKey)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/campaignmanagement/PromotionSearch"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/PromotionSearch"), headers);
        }

        public static HttpResponseMessage CampaignPromotionSearch(string bearer, string ocpSubKey, string promotionId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/campaignmanagement/PromotionCampaignSearch?promotionId={promotionId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/PromotionCampaignSearch?promotionId={promotionId}"), headers);
        }

        public static HttpResponseMessage RetrieveCampaignDetails(string bearer, string ocpSubKey, string campaignCode)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/campaignmanagement/campaigndetails?CampaignCode={campaignCode}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/campaigndetails?CampaignCode={campaignCode}"), headers);
        }

        public static HttpResponseMessage CampaignSearch(string bearer, string ocpSubKey)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/campaignmanagement/campaigns/search"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/campaignmanagement/campaigns/search"), headers);
        }

        public static HttpResponseMessage RetrieveAddressHistory(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/Customermicroservice/{customerId}/addressHistory"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/microservices/{customerId}/addressHistory"), headers);
        }

        public static HttpResponseMessage RetrieveContactHistory(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/Customermicroservice/{customerId}/contactHistory"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/microservices/{customerId}/contactHistory"), headers);
        }

        public static HttpResponseMessage RetrieveFlyingClubMemberDoc(string bearer, string ocpSubKey, string customerId)
        {

            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/flyingClubMember/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/flyingClubMember/{customerId}"), headers);
        }

        public static HttpResponseMessage RetrieveMembershipChangeDoc(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/membershipschange"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/membershipschange"), headers);
        }

        public static HttpResponseMessage RetrieveTargetedMembershipDoc(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/targetedmemberships"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/targetedmemberships"), headers);
        }

        public static HttpResponseMessage RetrieveTransactionDoc(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/transaction"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/transaction"), headers);
        }

        public static HttpResponseMessage RetrievePartnerMembershipDoc(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/PartnerMembership"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/PartnerMembership"), headers);
        }

        public static HttpResponseMessage CreatePartnerMembershipDoc(string bearer, string ocpSubKey, string memberNumber, string customerId, string startDate, string partnerStatusCode, string partnerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/CreatePartnerMembership"), Factory.Microservice.CreatePartnerMembership(customerId, memberNumber, startDate, partnerStatusCode, partnerId), headers);
        }
        public static HttpResponseMessage GetCreatePartnerMembershipDoc(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage RetrieveGroupDocByCustomerId(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/group/customer/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/group/customer/{customerId}"), headers);
        }

        public static HttpResponseMessage RetrieveGroupDocByGroupId(string bearer, string ocpSubKey, string groupId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/group/{groupId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/group/{groupId}"), headers);
        }

        public static HttpResponseMessage RetrieveGroupHistoryByCustomerId(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/groupHistory/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/groupHistory/{customerId}"), headers);
        }

        public static HttpResponseMessage RetrieveContactNotesByCustomerId(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/contactnotes"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/contactnotes"), headers);
        }

        public static HttpResponseMessage RetrieveHouseholdMemberDetails(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/loyaltymembermanagement/HouseholdDetails/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltymembermanagement/HouseholdDetails/{customerId}"), headers);
        }

        public static HttpResponseMessage RetrievePartnerTypes(string bearer, string ocpSubKey)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/loyaltymembermanagement/Reference/Partners"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltymembermanagement/Reference/Partners"), headers);
        }

        public static HttpResponseMessage RetrieveMemberAccountDetails(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/loyaltymembermanagement/GetLoyaltyMember/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltymembermanagement/GetLoyaltyMember/{customerId}"), headers);
        }

        public static HttpResponseMessage RetrieveListOfPromotionEnrolledMembers(string bearer, string ocpSubKey, string promotionId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/loyaltymembermanagement/PromotionEnrolledMembers/{promotionId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltymembermanagement/PromotionEnrolledMembers/{promotionId}"), headers);
        }

        public static HttpResponseMessage RetrieveSupplementarySilverMemberDetails(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                     { "Authorization", bearer },
                     { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/loyaltymembermanagement/supplementarySilver/accountDetails/{customerId}"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/loyaltymembermanagement/supplementarySilver/accountDetails/{customerId}"), headers);
        }

        public static HttpResponseMessage CreateTransaction(string bearer, string ocpSubKey, string customerId, string memberNumber, string partnerCode, string productName, string activityDate, string comments, int tierPoints, int points, string productDescription, string partnerName, string partnerCategory, string productCategory, string transactionSource, string createdBy, string transactionChannel, string statementText, string activityType)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{_env}/digital/vred/VredTransaction/CreateTransaction"), Factory.Microservice.CreateTransaction(customerId, memberNumber, partnerCode, productName, activityDate, comments, tierPoints, points, productDescription, partnerName, partnerCategory, productCategory, transactionSource, createdBy, transactionChannel, statementText, activityType), headers);
        }

        public static HttpResponseMessage CancelTransaction(string bearer, string ocpSubKey, string memberNumber, string transactionId, string activityDate, string transactionChannel, string cancellationReason, string updatedBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{_env}/digital/vred/VredTransaction/Cancel"), Factory.Microservice.CancelTransaction(memberNumber, transactionId, activityDate, transactionChannel, cancellationReason, updatedBy), headers);
        }

        public static HttpResponseMessage GetTransactionResponse(string bearer, string ocpSubKey, string url)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Get(url, headers);
        }

        public static HttpResponseMessage GetMemberPointBalance(string bearer, string ocpSubKey, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/vredMicroservice/{memberNumber}/VirginPoints"), headers) : WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/{_env}/digital/vred/{memberNumber}/VirginPoints"), headers);
        }

        public static HttpResponseMessage GetMemberTransactionStatement(string bearer, string ocpSubKey, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/vredMicroservice/VredStmts/{memberNumber}"), headers) : WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/{_env}/digital/vred/VredStmts/{memberNumber}"), headers);
        }

        public static HttpResponseMessage RetrieveAwardDoc(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/awards"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/awards"), headers);
        }

        public static HttpResponseMessage CreateAwardDoc(string bearer, string ocpSubKey, string customerId, string createdBy, string awardNumber, string productId, string startDate, string expiryDate, string status)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/award"), Factory.Microservice.CreateAward(customerId, createdBy, awardNumber, productId, startDate, expiryDate, status), headers);
        }

        public static HttpResponseMessage GetCreateAwardDoc(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage UpdateAwardDoc(string bearer, string ocpSubKey, string customerId, string id, string lastUpdatedBy, string status, string productId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/award"), Factory.Microservice.UpdateAward(customerId, id, lastUpdatedBy, status, productId), headers);
        }

        public static HttpResponseMessage GetUpdateAwardDoc(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage UpdateMemberAddress(string bearer, string ocpSubKey, string customerId, string currentAddressType, string newAddressLine1, string currentAddressLine2, string currentAddressLine3, string currentAddressVerified, string currentLandlordOrCompanyName, string currentCity, string currentPostalCode, string currentRegion, string currentCountry, string currentPreferredFlag)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/microservices/{customerId}/UpdateMemberAddress"), Factory.Microservice.UpdateMemberAddress(customerId, currentAddressType, newAddressLine1, currentAddressLine2, currentAddressLine3, currentAddressVerified, currentLandlordOrCompanyName, currentCity, currentPostalCode, currentRegion, currentCountry, currentPreferredFlag), headers);
        }

        public static HttpResponseMessage GetUpdateMemberAddress(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage UpdateMemberEmail(string bearer, string ocpSubKey, string memberNumber, string newEmailAddress)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/microservices/{memberNumber}/UpdateEmailAddress"), Factory.Microservice.UpdateMemberEmail(newEmailAddress), headers);
        }


        public static HttpResponseMessage UpdateMemberPhone(string bearer, string ocpSubKey, string customerId, string memberNumber, string phoneNumber, string countryCode)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/microservices/UpdateMemberPhoneNumber"), Factory.Microservice.UpdateMemberPhone(customerId, memberNumber, phoneNumber, countryCode), headers);
        }

        public static HttpResponseMessage GetUpdateMemberPhone(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage CreateVAEnrolment(string bearer, string ocpSubKey, string firstName, string lastName, string middleName, string phoneNumber, string customerType, string doB, string emailId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/Microservices/VAEnrolment"), Factory.Microservice.CreateVAEnrolment(firstName, lastName, middleName, phoneNumber, customerType, doB, emailId), headers);
        }

        public static HttpResponseMessage GetVAEnrolment(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage UpdateVACMemberProfile(string bearer, string ocpSubKey, string customerId, string memberNumber, string firstName, string lastName, string doB, string emailAddress, string phoneNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/microservices/UpdateVirginAccountCustomer"), Factory.Microservice.UpdateVACMemberProfile(customerId, memberNumber, firstName, lastName, doB, emailAddress, phoneNumber), headers);
        }

        public static HttpResponseMessage UpgradeToFlyingClub(string bearer, string ocpSubKey, string firstName, string lastName, string doB, string phoneNumber, string emailAddress, string customerId, string memberNumber, string enrollmentChannel, string channelUserId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/Microservices/UpgradeToFlyingClub"), Factory.Microservice.UpgradeToFlyingClub(firstName, lastName, doB, phoneNumber, emailAddress, customerId, memberNumber, enrollmentChannel, channelUserId), headers);
        }

        public static HttpResponseMessage RetrieveFutureFlights(string emailAddress)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Ocp-Apim-Subscription-Key", orderDataOcpSubKey },
                    {"email", emailAddress },
                    {"FCNumber", "" },
                    {"phoneNo","" }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl("/orderdata/FutureBookings"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/orderdata/FutureBookings"), headers);
        }

        public static HttpResponseMessage RetrieveVouchers(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/customertransactions/{customerId}/Vouchers"), headers) : WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/Vouchers"), headers);
        }

        public static HttpResponseMessage RedeemVoucher(string bearer, string ocpSubKey, string customerId, string memberNumber, string redemptionType, string activityDate, string bookingClass, string originAirport, string destinationAirport, string bookingReference, string voucherNumber, string voucherName, int points)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/redeemvoucher"), Factory.Microservice.RedeemVoucher(customerId, memberNumber, redemptionType, activityDate, bookingClass, originAirport, destinationAirport, bookingReference, voucherNumber, voucherName, points), headers);
        }

        public static HttpResponseMessage CreateTransactionResponse(string bearer, string ocpSubKey, string headerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return _env.Is("prod") ? WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/digital/vred/CreateTransaction/response/{headerId}"), headers) : WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/{_env}/digital/vred/CreateTransaction/response/{headerId}"), headers);
        }

        public static HttpResponseMessage VoucherRollback(string bearer, string ocpSubKey, string customerId, string memberNumber, string voucherNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/rollbackVoucherRedemption"), Factory.Microservice.VoucherRollback(customerId, memberNumber, voucherNumber), headers);
        }

        public static HttpResponseMessage UpdatePnr(string bearer, string ocpSubKey, string customerId, string memberNumber, string voucherNumber, string pNR)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Json.Patch(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/voucherRelatedTxnsPnr"), Factory.Microservice.UpdatePnr(customerId, memberNumber, voucherNumber, pNR), headers);

        }
    }
}
