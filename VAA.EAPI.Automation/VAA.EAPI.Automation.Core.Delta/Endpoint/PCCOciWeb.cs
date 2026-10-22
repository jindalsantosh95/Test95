namespace VAA.EAPI.Automation.Core.Delta.Endpoint
{
    public static class PCCOciWeb
    {
        public static HttpResponseMessage TravelParties(string awsBearerToken, string requestPayload, string clientId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Json.Post(Config.DeltaPCCOciWebEndPoint.GetUrl("/travelparties"), requestPayload, headers);
        }

        public static HttpResponseMessage SelectPassengers(string awsBearerToken, string requestPayload, string travelPartyId, string clientId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Json.Patch(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}"), requestPayload, headers);
        }

        public static HttpResponseMessage BoardingDocuments(string awsBearerToken, string travelPartyId, string clientId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Json.Post(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}/boardingdocuments"), "", headers);
        }

        public static HttpResponseMessage GetBaggage(string awsBearerToken, string travelPartyId, string clientId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Get(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}/addBags"), headers);
        }

        public static HttpResponseMessage AddBags(string awsBearerToken, string travelPartyId, string requestPayload, string clientId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Json.Post(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}/addBags"), requestPayload, headers);
        }

        public static HttpResponseMessage UpdateBags(string awsBearerToken, string travelPartyId, string requestPayload, string clientId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Json.Patch(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}/addBags"), requestPayload, headers);
        }

        public static HttpResponseMessage ReviewAndPayDetails(string awsBearerToken, string travelPartyId, string clientId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Get(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}/reviewAndPayDetail"), headers);
        }

        public static HttpResponseMessage GeneratePaymentReferenceId(string awsBearerToken, string requestPayload)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("channelId", "DFMOBILE");
            headers.Add("appID", "D2");
            headers.Add("purchasePath", "DFMOBILEOCI");

            return WebRequest.Json.Post(Config.DeltaPaymentManagementEndPoint.GetUrl("/paymentReferenceId"), requestPayload, headers);
        }

        public static HttpResponseMessage GetPaymentDetails(string awsBearerToken, string paymentReferenceId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("channelId", "DFMOBILE");
            headers.Add("appID", "D2");
            headers.Add("purchasePath", "DFMOBILEOCI");

            return WebRequest.Get(Config.DeltaPaymentManagementEndPoint.GetUrl($"/paymentDetails/{paymentReferenceId}"), headers);
        }

        public static HttpResponseMessage SubmitReviewAndPayDetails(string awsBearerToken, string travelPartyId, string requestPayload, string clientId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("ClientId", clientId);

            return WebRequest.Json.Post(Config.DeltaPCCOciWebEndPoint.GetUrl($"/travelparties/{travelPartyId}/reviewAndPayDetail"), requestPayload, headers);
        }

        public static HttpResponseMessage TrmApiPatchRequest(string awsBearerToken, string clientId, string trmVSDigitalTestId, string requestPayload, string requirementsResourceId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("ClientId", clientId);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("transactionId", Guid.NewGuid().ToString());
            headers.Add("Content-Type", "application/json");
            headers.Add("X-TRRM_VSDigital_Test", trmVSDigitalTestId);

            return WebRequest.Json.Patch(Config.DeltaTRMAPIEndPoint.GetUrl($"/int/requirements/{requirementsResourceId}"), requestPayload, headers);
        }

        public static HttpResponseMessage RetrieveRequirements(string awsBearerToken, string clientId, string trmVSDigitalTestId, string requestPayload)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("ClientId", clientId);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("transactionId", Guid.NewGuid().ToString());
            headers.Add("Content-Type", "application/json");
            headers.Add("X-TRRM_VSDigital_Test", trmVSDigitalTestId);

            return WebRequest.Json.Post(Config.DeltaTRMAPIEndPoint.GetUrl("/int/requirements"), requestPayload, headers);
        }

        public static HttpResponseMessage GetRequirementsByResourceId(string awsBearerToken, string clientId, string trmVSDigitalTestId, string requirementsResourceId)
        {
            var headers = new System.Collections.Specialized.NameValueCollection();

            headers.Add("Authorization", awsBearerToken);
            headers.Add("ClientId", clientId);
            headers.Add("AppChannelName", "DFMOBILE");
            headers.Add("AppId", "D2");
            headers.Add("transactionId", Guid.NewGuid().ToString());
            headers.Add("Content-Type", "application/json");
            headers.Add("X-TRRM_VSDigital_Test", trmVSDigitalTestId);

            return WebRequest.Get(Config.DeltaTRMAPIEndPoint.GetUrl($"/int/requirements/{requirementsResourceId}"), headers);
        }
    }
}
