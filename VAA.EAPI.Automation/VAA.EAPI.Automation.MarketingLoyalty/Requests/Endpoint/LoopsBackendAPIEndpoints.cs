﻿using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Factory;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint
{
    public static class LoopsBackendAPIEndpoints
    {

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        // Project-Dev -> MarketingLoyaltyAPI->Cosmos Acc -> SIT KeyVault
        // BAU-SIT -> LOOPS BAU SIT->Cosmos Acc  -> SIT KeyVault
        // BAU-UAT -> LOOPS BAU UAT ->Cosmos UAT -> UAT KeyVault

        private static readonly string _customerApiEndpoint = backendAPIEnv.Is("Project-DEV") ? Vault.Default.GetSecretValue("MarketingLoyalty-BECustomerApiEndpoint") : backendAPIEnv.Is("BAU-SIT") ? Config.TestResourcesVirginAtlantic.GetUrl("/UAT/LoyaltyManagement/Enterprise/CustomerManagement") : Config.TestResourcesVirginAtlantic.GetUrl("/UAT/V2/LoyaltyManagement/Enterprise/CustomerManagement");

        private static readonly string _referenceApiEndpoint = backendAPIEnv.Is("Project-DEV") ? Vault.Default.GetSecretValue("MarketingLoyalty-BEReferenceApiEndpoint") : backendAPIEnv.Is("BAU-SIT") ? Config.TestResourcesVirginAtlantic.GetUrl("/UAT/LoyaltyManagement/Enterprise/ReferenceManagement") : Config.TestResourcesVirginAtlantic.GetUrl("/UAT/V2/LoyaltyManagement/Enterprise/ReferenceManagement");

        private static readonly string _transactionApiEndpoint = backendAPIEnv.Is("Project-DEV") ? Vault.Default.GetSecretValue("MarketingLoyalty-BETransactionApiEndpoint") : backendAPIEnv.Is("BAU-SIT") ? Config.TestResourcesVirginAtlantic.GetUrl("/UAT/LoyaltyManagement/Enterprise/TransactionManagement") : Config.TestResourcesVirginAtlantic.GetUrl("/UAT/V2/LoyaltyManagement/Enterprise/TransactionManagement");

        private static readonly string _voucherApiEndpoint = backendAPIEnv.Is("Project-DEV") ? Vault.Default.GetSecretValue("MarketingLoyalty-BEVoucherApiEndpoint") : backendAPIEnv.Is("BAU-SIT") ? Config.TestResourcesVirginAtlantic.GetUrl("/UAT/LoyaltyManagement/Enterprise/VoucherManagement") : Config.TestResourcesVirginAtlantic.GetUrl("/UAT/V2/LoyaltyManagement/Enterprise/VoucherManagement");

        private static readonly string _financialPartnerProcessingApiEndpoint = backendAPIEnv.Is("Project-DEV") ? Vault.Default.GetSecretValue("MarketingLoyalty-BEFinancialPartnerProcessingApiEndpoint") : backendAPIEnv.Is("BAU-SIT") ? Config.TestResourcesVirginAtlantic.GetUrl("/fpmigrationnotification-staging/vred-accounts-notification") : Config.TestResourcesVirginAtlantic.GetUrl("/fpmigrationnotification-preprod/vred-accounts-notification");

        private static readonly string ocpSubKey = Vault.Default.GetSecretValue("MarketingLoyalty-BackendCustomerSubscriptionKey-UAT");

        private static readonly string financialPartnerApiocpSubKey = Vault.Default.GetSecretValue("MarketingLoyalty-FinancialPartnerProcessingAPISubscriptionKey");


        public static class Customer
        {

            public static HttpResponseMessage CustomerSearch(string bearer, string memberNumber, string emailId)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return string.IsNullOrEmpty(emailId) ? WebRequest.Get($"{_customerApiEndpoint}/customers?cardNumber={memberNumber}", headers) :
                    WebRequest.Get($"{_customerApiEndpoint}/customers?emailAddress={emailId}", headers);
            }

            public static HttpResponseMessage CustomerRetrieve(string bearer, string customerId)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_customerApiEndpoint}/customers/{customerId}", headers);
            }

            public static HttpResponseMessage CreateMemberTier(string customerId, string bearer, string updatedOn, string tierStartDate,
                        string tierEndDate, int revision, string tierName, string appliedRuleType)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Post($"{_customerApiEndpoint}/customers/{customerId}/tier",
                    LoopsBackendAPIFactory.Customer.CreateMemberTier(updatedOn, tierStartDate, tierEndDate,
                    revision, tierName, appliedRuleType), headers);

            }

            public static HttpResponseMessage UpdateMemberTier(string customerId, string bearer, string docId, string updatedOn, string tierStartDate,
                        string tierEndDate, int revision)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Patch($"{_customerApiEndpoint}/customers/{customerId}/tier",
                    LoopsBackendAPIFactory.Customer.UpdateMemberTier(docId, updatedOn, tierStartDate, tierEndDate,
                    revision), headers);
            }

            public static HttpResponseMessage UpdateMemberAddress(string customerId, string addressId, string bearer, string addressLine1, string addressLine2, string addressLine3,
                       string company, string city, string startDate, string postalCode, string region, string country, string lastUpdatedBy)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Patch($"{_customerApiEndpoint}/customers/{customerId}/address/{addressId}",
                    LoopsBackendAPIFactory.Customer.UpdateMemberAddress(addressLine1, addressLine2, addressLine3,
                        company, city, startDate, postalCode, region, country, lastUpdatedBy), headers);
            }

            public static HttpResponseMessage UpdateMemberPreference(string bearer, string customerId, string fc1PrefValue,
                string doNotCallPrefValue = "", string doNotEmailPrefValue = "", string doNotSMSPrefValue = "", string preferredPartnerFFP = "", string updatedBy = "")
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Patch($"{_customerApiEndpoint}/customers/{customerId}/preferences",
                    LoopsBackendAPIFactory.Customer.UpdateMemberPreference(fc1PrefValue, doNotCallPrefValue, doNotSMSPrefValue, doNotEmailPrefValue, preferredPartnerFFP, updatedBy), headers);
            }

            public static HttpResponseMessage UpdateMemberPhone(string bearer, string customerId, string phoneNumber,
                string startDate, string phoneId, string lastUpdatedBy = "")
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Patch($"{_customerApiEndpoint}/customers/{customerId}/phone/{phoneId}",
                    LoopsBackendAPIFactory.Customer.UpdateMemberPhone(phoneNumber, startDate, lastUpdatedBy), headers);
            }

            public static HttpResponseMessage UpdateMemberEmail(string bearer, string customerId, string emailAddress,
                string emailContactId, string lastUpdatedBy = "")
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Patch($"{_customerApiEndpoint}/customers/{customerId}/email/{emailContactId}",
                    LoopsBackendAPIFactory.Customer.UpdateMemberEmail(emailAddress, lastUpdatedBy), headers);
            }

        }

        public static class Reference
        {
            public static HttpResponseMessage GetPartners(string bearer)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key",ocpSubKey }
            };

                return WebRequest.Get($"{_referenceApiEndpoint}/reference/partners", headers);
            }

            public static HttpResponseMessage GetCountries(string bearer)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key",ocpSubKey }
            };

                return WebRequest.Get($"{_referenceApiEndpoint}/reference/countries", headers);
            }

            public static HttpResponseMessage GetListOfMediaCodes(string bearer)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_referenceApiEndpoint}/reference/media/codes", headers);
            }

            public static HttpResponseMessage GetListOfStatus(string bearer)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_referenceApiEndpoint}/reference/member/status", headers);
            }

            public static HttpResponseMessage GetListOfProducts(string bearer)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_referenceApiEndpoint}/reference/products", headers);
            }

        }

        public static class Transaction
        {

            public static HttpResponseMessage RetrieveRedemptionTransaction(string bearer, string customerId, string transactionId)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_transactionApiEndpoint}/customer/{customerId}/transactions/{transactionId}/debit", headers);
            }

            public static HttpResponseMessage RetrieveAccrualTransaction(string bearer, string customerId, string transactionId)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_transactionApiEndpoint}/customer/{customerId}/transactions/{transactionId}/credit", headers);
            }
            public static HttpResponseMessage CreateAccrualTransaction(string bearer, string customerId, string bookingReference, string activityDate, string ticketIssueDate)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key",ocpSubKey }
            };

                return WebRequest.Json.Post($"{_transactionApiEndpoint}/customer/{customerId}/transaction/credit",
                    LoopsBackendAPIFactory.Transaction.CreateAccrualTransaction(bookingReference, activityDate, ticketIssueDate), headers);
            }

            public static HttpResponseMessage CreateManualAdjustmentTransaction(string bearer, string customerId, string activityDate)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Post($"{_transactionApiEndpoint}/customer/{customerId}/transaction/credit",
                    LoopsBackendAPIFactory.Transaction.CreateManualAdjustmentTransaction(activityDate), headers);
            }

            public static HttpResponseMessage CreateRedemptionTransaction(string bearer, string customerId, string bookingReference, string activityDate, string ticketIssueDate)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key",ocpSubKey }
            };

                return WebRequest.Json.Post($"{_transactionApiEndpoint}/customer/{customerId}/transaction/debit",
                    LoopsBackendAPIFactory.Transaction.CreateRedemptionTransaction(bookingReference, activityDate, ticketIssueDate), headers);
            }

        }

        public static class Award
        {

            public static HttpResponseMessage RetrieveAward(string bearer, string customerId)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Get($"{_voucherApiEndpoint}/customers/{customerId}/voucher", headers);
            }

            public static HttpResponseMessage CreateAward(string bearer, string customerId, string voucherName, string startDate, string endDate, string createdBy = "")
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Post($"{_voucherApiEndpoint}/customers/{customerId}/voucher",
                    LoopsBackendAPIFactory.Award.CreateAward(voucherName, startDate, endDate, createdBy), headers);
            }

            public static HttpResponseMessage UpdateAward(string bearer, string customerId, string voucherId, string status, string startDate, string endDate, string usedDate, string updatedBy = "")
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Patch($"{_voucherApiEndpoint}/customers/{customerId}/voucher/{voucherId}",
                    LoopsBackendAPIFactory.Award.UpdateAward(status, startDate, endDate, usedDate, updatedBy), headers);
            }

            public static HttpResponseMessage GiftAward(string bearer, string customerId, string voucherId, string recipientMemberNumber, string recipientCustomerId, string giftedBy = "")
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

                return WebRequest.Json.Post($"{_voucherApiEndpoint}/customers/{customerId}/voucher/{voucherId}/transfers",
                    LoopsBackendAPIFactory.Award.GiftAward(recipientMemberNumber, recipientCustomerId, giftedBy), headers);
            }

            public static HttpResponseMessage RedeemAward(string bearer, string customerId, string voucherId)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

                return WebRequest.Json.Put($"{_voucherApiEndpoint}/customers/{customerId}/voucher/{voucherId}", LoopsBackendAPIFactory.Award.RedeemAward(), headers);
            }
        }

        public static class FinancialPartnerMigration
        {
            public static HttpResponseMessage VoucherAward(string bearer, string memberNumber, string voucherType, string cardLinkPartnerUserId, string voucherActivityDate, string cardOpenDate)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Authorization", bearer },

                { "Ocp-Apim-Subscription-Key", financialPartnerApiocpSubKey }
            };

                return WebRequest.Json.Post(_financialPartnerProcessingApiEndpoint, LoopsBackendAPIFactory.FinancialPartnerProcessing.VoucherAward(memberNumber, voucherType, cardLinkPartnerUserId, voucherActivityDate, cardOpenDate), headers);
            }

            public static HttpResponseMessage VoucherAwardVMCC(string bearer, string memberNumber, string cardLinkPartnerUserId, string cardLinkPartnerUserIdDomain, string voucherActivityDate, string cardOpenDate, string activityCode)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", financialPartnerApiocpSubKey }
                };

                return WebRequest.Json.Post(_financialPartnerProcessingApiEndpoint, LoopsBackendAPIFactory.FinancialPartnerProcessing.VoucherAwardVMCC(memberNumber, cardLinkPartnerUserId, cardLinkPartnerUserIdDomain, voucherActivityDate, cardOpenDate, activityCode), headers);
            }

            public static HttpResponseMessage UsCoBrandVoucherTierpointsTxn(string bearer, string transactionId, string memberNumber, string transactionActivityDate, string cardLinkPartnerUserIdDomain, string cardLinkPartnerUserId, string monthlySpendAmount)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", financialPartnerApiocpSubKey }
                };

                return WebRequest.Json.Post($"{_financialPartnerProcessingApiEndpoint}", LoopsBackendAPIFactory.FinancialPartnerProcessing.UsCoBrandVoucherTierpointsTxn(transactionId, memberNumber, transactionActivityDate, cardLinkPartnerUserIdDomain, cardLinkPartnerUserId, monthlySpendAmount), headers);
            }

            public static HttpResponseMessage VCCUVoucherTierpointsTxn(string bearer, string transactionId, string memberNumber, string transactionActivityDate, string cardLinkPartnerUserIdDomain, string cardLinkPartnerUserId, string monthlySpendAmount, string creditCardType)
            {
                System.Collections.Specialized.NameValueCollection headers = new System.Collections.Specialized.NameValueCollection
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", financialPartnerApiocpSubKey }
                };

                return WebRequest.Json.Post($"{_financialPartnerProcessingApiEndpoint}", LoopsBackendAPIFactory.FinancialPartnerProcessing.VCCUVoucherTierpointsTxn(transactionId, memberNumber, transactionActivityDate, cardLinkPartnerUserIdDomain, cardLinkPartnerUserId, monthlySpendAmount, creditCardType), headers);
            }
        }
    }
}
