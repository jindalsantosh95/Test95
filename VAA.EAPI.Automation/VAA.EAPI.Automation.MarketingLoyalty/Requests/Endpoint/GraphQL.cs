﻿using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint
{
    public static class GraphQL
    {

        private readonly static string _env = Config.Environment;

        #region CampaignManagementEndpoints
        public static HttpResponseMessage CampaignPromotionLink(string bearer, string campaignId, string promotionId, string createdBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CampaignPromotionLink(campaignId, promotionId, createdBy), headers);
        }


        public static HttpResponseMessage AssignCampaign(string bearer, string customerId, string memberNumber, string campaignCode, string campaignId, string createdBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.AssignCampaign(customerId, memberNumber, campaignCode, campaignId, createdBy), headers);
        }

        public static HttpResponseMessage SkyPartnerRetroTransactionCreate(string bearer, string customerId, string documentFirstName, string documentSurname, string memberNumber, string boardingNumber, string gdsRecordLocator, string seatNumber, string ticketIssueDate, string ticketNumber, string marketedCabin, string flightActivityDate, string destinationAirportCode, string originAirportCode, string marketingAirlineCode, string marketingBookingClass, string marketingFlightNumber, string operatedAirlineCode, string operatedFlightNumber, string operatedbookingClass, string codeshareFlag, string vsComments, string productName, string createdBy, string transactionChannel)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.SkyPartnerRetroCreate(customerId, documentFirstName, documentSurname, memberNumber, boardingNumber, gdsRecordLocator, seatNumber, ticketIssueDate, ticketNumber, marketedCabin, flightActivityDate, destinationAirportCode, originAirportCode, marketingAirlineCode, marketingBookingClass, marketingFlightNumber, operatedAirlineCode, operatedFlightNumber, operatedbookingClass, codeshareFlag, vsComments, productName, createdBy, transactionChannel), headers);
        }

        public static HttpResponseMessage CampaignUpdate(string bearer, string campaignId, string campaignCode, string campaignName, string startDate, string endDate, string status, string type, string desc, bool regReqFlag, string linkToTemplate)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CampaignUpdate(campaignId, campaignCode, campaignName, startDate, endDate, status, type, desc, regReqFlag, linkToTemplate), headers);
        }


        public static HttpResponseMessage RegisterCampaign(string bearer, string customerId, string memberNumber, string campaignCode, string campaignId, string documentId, string createdBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RegisterCampaign(customerId, memberNumber, campaignCode, campaignId, documentId, createdBy), headers);
        }

        public static HttpResponseMessage RetrieveMemberCampaign(string bearer, string customerId, string memberNumber, string order, string where)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveMemberCampaign(customerId, memberNumber, order, where), headers);
        }


        public static HttpResponseMessage RetrieveCampaignType(string bearer)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", bearer);

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveCampaignType(), headers);
        }

        public static HttpResponseMessage CampaignCreate(string bearer, string campaignCode, string campaignName, string startDate, string endDate, string status, string type, string desc, bool regReqFlag, string linkToTemplate)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CampaignCreate(campaignCode, campaignName, startDate, endDate, status, type, desc, regReqFlag, linkToTemplate), headers);
        }


        public static HttpResponseMessage UnassignCampaign(string bearer, string customerId, string documentId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.UnassignCampaign(customerId, documentId), headers);
        }


        public static HttpResponseMessage DelinkCampaignPromotion(string bearer, string campaignId, string promotionId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.DelinkCampaignPromotion(campaignId, promotionId), headers);
        }


        public static HttpResponseMessage PromotionSearchCampaign(string bearer, string promotionId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.PromotionSearchCampaign(promotionId), headers);
        }

        public static HttpResponseMessage CampaignSearch(string bearer, string campaignId, string campaignName, string campaignCode, string startDateFrom, string endDateFrom, string startDateTo, string endDateTo, string status, string type, string ocpSubKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CampaignSearch(campaignId, campaignName, campaignCode, startDateFrom, endDateFrom, startDateTo, endDateTo, status, type), headers);

        }

        public static HttpResponseMessage CampaignRetrieve(string bearer, string campaignId, string campaignCode)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CampaignRetrieve(campaignId, campaignCode), headers);
        }

        public static HttpResponseMessage DownloadCampaignsList(string bearer)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.DownloadCampaignList(), headers);

        }

        public static HttpResponseMessage AssociatedMembersToACampaign(string bearer, string campaignCode)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.AssociatedMembersToACampaign(campaignCode), headers);

        }

        #endregion

        #region PromotionManagementEndpoints
        public static HttpResponseMessage PromotionCreate(string bearer, string promotionName, string partnerName, string startDate, string endDate, string activeFlag)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.PromotionCreate(promotionName, partnerName, startDate, endDate, activeFlag), headers);
        }


        public static HttpResponseMessage PromotionUpdate(string bearer, string promotionId, string promotionName, string partnerName, string startDate, string endDate, string activeFlag)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"),
                Factory.GraphQL.PromotionUpdate(promotionId, promotionName, partnerName, startDate, endDate, activeFlag), headers);
        }


        public static HttpResponseMessage PromotionSearchAndRetrieve(string bearer, string promotionId, string promotionName, string startDateF, string endDateF, string startDateT, string endDateT, string partnerName, string active, string ocpSubKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.PromotionSearchAndRetrieve(promotionId, promotionName, startDateF, endDateF, startDateT, endDateT, partnerName, active), headers);
        }

        #endregion

        #region LoyaltyContactNotesEndpoints
        public static HttpResponseMessage LoyaltyCreateNote(string bearer, string customerId, string memberNumber, string notesGroup, string noteType, string text, string createdBy, string lastUpdatedBy, string createdOnDate, string lastUpdatedOnDate, string docId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.LoyaltyCreateNote(customerId, memberNumber, notesGroup, noteType, text, createdBy, lastUpdatedBy, createdOnDate, lastUpdatedOnDate, docId), headers);
        }


        public static HttpResponseMessage LoyaltyUpdateNote(string bearer, string customerId, string memberNumber, string notesGroup, string noteType, string text, string createdBy, string lastUpdatedBy, string createdOnDate, string lastUpdatedOnDate, string docId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.LoyaltyUpdateNote(customerId, memberNumber, notesGroup, noteType, text, createdBy, lastUpdatedBy, createdOnDate, lastUpdatedOnDate, docId), headers);
        }


        public static HttpResponseMessage LoyaltyDeleteNote(string bearer, string customerId, string memberNumber, string notesGroup, string noteType, string text, string createdBy, string lastUpdatedBy, string createdOnDate, string lastUpdatedOnDate, string docId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.LoyaltyDeleteNote(customerId, memberNumber, notesGroup, noteType, text, createdBy, lastUpdatedBy, createdOnDate, lastUpdatedOnDate, docId), headers);
        }


        public static HttpResponseMessage LoyaltyGetNote(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.LoyaltyGetNote(customerId, memberNumber), headers);
        }

        #endregion

        #region LoyaltyMemberManagementEndpoints
        public static HttpResponseMessage PartnerRetrieval(string bearer)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.PartnerRetrieval(), headers);
        }


        public static HttpResponseMessage RetrieveMemberAccountDetails(string bearer, string customerId, string where, string ocpSubKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveMemberAccountDetails(customerId, where), headers);
        }


        public static HttpResponseMessage RetrieveHouseHoldDetails(string bearer, string customerId, string memberNumber, string ocpSubKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveHouseHoldDetails(customerId, memberNumber), headers);

        }

        public static HttpResponseMessage HouseHoldLinking(string bearer, string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.HouseHoldLinking(childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, initiatedBy), headers);
        }


        public static HttpResponseMessage HouseHoldDelinking(string bearer, string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.HouseHoldDelinking(childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, initiatedBy), headers);
        }

        #endregion

        #region CustomerEndpoints

        public static HttpResponseMessage CustomerSearch(string bearer, string customerId, string memberNumber, string ocpSubKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CustomerSearch(customerId, memberNumber), headers);
        }

        #endregion

        #region SupplementarySilverAccountEndpoints

        public static HttpResponseMessage SupplementarySilverSearch(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.SupplementarySilverSearch(customerId, memberNumber), headers);

        }


        public static HttpResponseMessage SupplementarySilverDelink(string bearer, string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.SupplementarySilverDelink(childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, initiatedBy), headers);

        }


        public static HttpResponseMessage SupplementarySilverLink(string bearer, string childCustomerId, string childMemberNumber, string parentCustomerId, string parentMemberNumber, string initiatedBy)
        {

            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.SupplementarySilverLink(childCustomerId, childMemberNumber, parentCustomerId, parentMemberNumber, initiatedBy), headers);
        }

        #endregion

        #region Audit Endpoints

        public static HttpResponseMessage RetrieveAddressAudit(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveAddressAudit(customerId, memberNumber), headers);

        }

        public static HttpResponseMessage RetrieveContactAudit(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveContactAudit(customerId, memberNumber), headers);
        }

        public static HttpResponseMessage RetrieveAwardAudit(string bearer, string customerId, string memberNumber, string ocpSubKey = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{(_env.Is("Prod") ? string.Empty : $"{_env}/")}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveAwardAudit(customerId, memberNumber), headers);
        }

        public static HttpResponseMessage RetrievePreferenceAudit(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrievePreferenceAudit(customerId, memberNumber), headers);
        }

        public static HttpResponseMessage RetrieveCustomerProfileAudit(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveCustomerProfileAudit(customerId, memberNumber), headers);
        }

        public static HttpResponseMessage RetrieveSupplementarySilverAudit(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveSupplementarySilverAudit(customerId, memberNumber), headers);
        }

        public static HttpResponseMessage RetrieveAmexCenturionAuditHistory(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveAmexCenturionAuditHistory(customerId, memberNumber), headers);
        }

        #endregion

        #region Retail Therapy Transactions endpoints
        public static HttpResponseMessage CreateEarnTransactionForRetailTherapy(string bearer, string token, string activityDate, string channel, string comment, string memberNumber, string orderNumber, int points)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"{bearer},{token}" }
            };



            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlMya")}"), Factory.GraphQL.CreateEarnTransactionForRetailTherapy(activityDate, channel, comment, memberNumber, orderNumber, points), headers);
        }


        public static HttpResponseMessage CreateBurnTransactionForRetailTherapy(string bearer, string token, string activityDate, string channel, string comment, string memberNumber, string orderNumber, int points)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"{bearer},{token}" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlMya")}"), Factory.GraphQL.CreateBurnTransactionForRetailTherapy(activityDate, channel, comment, memberNumber, orderNumber, points), headers);
        }

        public static HttpResponseMessage RetrieveMemberBalanceForRetailTherapy(string token)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection();

            if (token != null)
            {
                headers.Add("Authorization", token);
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.RetrieveMemberBalanceForRetailTherapy(), headers);
        }

        public static HttpResponseMessage CancelTransactionForRetailTherapy(string bearer, string token, string activityDate, string cancellationReason, string channel, string memberNumber, string transactionId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", $"{bearer},{token}" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlMya")}"), Factory.GraphQL.CancelTransactionForRetailTherapy(activityDate, cancellationReason, channel, memberNumber, transactionId), headers);
        }

        #endregion

        #region LoyaltyMemberCardLink including member Partnership Document endpoints

        public static HttpResponseMessage RetrieveLoyaltyMemberCardLink(string bearer, string customerId, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveLoyaltyMemberCardLink(customerId, memberNumber), headers);
        }

        public static HttpResponseMessage CreatePartnerMembership(string bearer, string cardType, string customerId, string memberNumber, string partnerCode, string partnerId, string startDate)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CreatePartnerMembership(cardType, customerId, memberNumber, partnerCode, partnerId, startDate), headers);
        }

        public static HttpResponseMessage UpdatePartnerMembership(string bearer, string cardType, string customerId, string memberNumber, string partnerCode, string partnerId, string startDate, string endDate, string docId, string updatedBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.UpdatePartnerMembership(cardType, customerId, memberNumber, partnerCode, partnerId, startDate, endDate, docId, updatedBy), headers);
        }

        #endregion

        #region Create voucher accruals endpoints

        public static HttpResponseMessage CreateVoucherAccrualTransaction(string bearer, string customerId, string memberNumber, string memberStatus, string activityDate, string comments, string createdBy, string bookingClass, string bookingReference, string originAirportCode, string destinationAirportCode, string ticketNumber, int noOfPassengers, int noOfSegments, string partnerCode, string productName, int points, int tierPoints, string transactionSource, string voucherNumber, string voucherUsage, string voucherId = "", string voucherName = "")
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CreateVoucherAccrualTransaction(customerId, memberNumber, memberStatus, activityDate, comments, createdBy, bookingClass, bookingReference, originAirportCode, destinationAirportCode, ticketNumber, noOfPassengers, noOfSegments, partnerCode, productName, points, tierPoints, transactionSource, voucherNumber, voucherUsage, voucherId, voucherName), headers);
        }

        #endregion

        #region LoyaltySync Endpoints
        public static HttpResponseMessage loyaltySyncSFPD(string bearer, string customerId, string dateOfBirth, string enrollmentChannel, string fcNumber, string firstName, string gender, string lastName)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.loyaltySyncSFPD(customerId, dateOfBirth, enrollmentChannel, fcNumber, firstName, gender, lastName), headers);

        }

        #endregion

        public static HttpResponseMessage VACProfileUpdate(string bearer, string customerId, string memberNumber, string firstName, string lastName, string emailId, string dateOfBirth, string phoneCountryCode, string phoneNumber, string preferenceValue)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.VACProfileUpdate(customerId, memberNumber, firstName, lastName, emailId, dateOfBirth, phoneCountryCode, phoneNumber, preferenceValue), headers);
        }

        public static HttpResponseMessage MemberSearchByPhone(string bearer, string phoneNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.MemberSearchByPhoneNumber(phoneNumber), headers);
        }

        public static HttpResponseMessage RetrieveRelatedTransactions(string bearer, string customerId, string memberNumber, string transactionId)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.RetrieveRelatedTransactions(customerId, memberNumber, transactionId), headers);
        }

        public static HttpResponseMessage CancelRelatedTransactions(string bearer, string customerId, string memberNumber, string initiatedTransactionId, string cancellationReason, string initiatedBy, List<Canceltransactiondetail> cancelTransactionDetails)
        {
            System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("graphQlLoyalty")}"), Factory.GraphQL.CancelRelatedTransactions(customerId, memberNumber, initiatedTransactionId, cancellationReason, initiatedBy, cancelTransactionDetails), headers);
        }

    }

}
