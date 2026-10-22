using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.CRM.Domain.CRMSchema
{
    public class PaymentEntity
    {
        [JsonProperty("vaa_paymentnumber")]
        public string VaaPaymentnumber { get; set; }

        [JsonProperty("vaa_paymentid")]
        public string VaaPaymentid { get; set; }

        [JsonProperty("vaa_beneficiaryaddress")]
        public string VaaBeneficiaryaddress { get; set; }

        [JsonProperty("_vaa_countrycode_value")]
        public string VaaCountrycodeValue { get; set; }

        [JsonProperty("vaa_iban")]
        public string VaaIban { get; set; }

        [JsonProperty("vaa_accountname")]
        public string VaaAccountname { get; set; }

        [JsonProperty("vaa_accountnumber")]
        public string VaaAccountnumber { get; set; }

        [JsonProperty("vaa_ifsc")]
        public string VaaIfsc { get; set; }

        [JsonProperty("vaa_bankcode")]
        public string VaaBankcode { get; set; }

        [JsonProperty("vaa_routingcode")]
        public string VaaRoutingcode { get; set; }

        [JsonProperty("vaa_swift")]
        public string VaaSwift { get; set; }

        [JsonProperty("vaa_clabenumber")]
        public string VaaClabenumber { get; set; }

        [JsonProperty("vaa_bankname")]
        public string VaaBankname { get; set; }

        [JsonProperty("vaa_bankcodenumber")]
        public string VaaBankcodenumber { get; set; }

        [JsonProperty("vaa_banksuffixnumber")]
        public string VaaBanksuffixnumber { get; set; }

        [JsonProperty("vaa_streetname")]
        public string VaaStreetname { get; set; }

        [JsonProperty("vaa_towncityname")]
        public string VaaTowncityname { get; set; }

        [JsonProperty("vaa_stateprovincecounty")]
        public string VaaStateprovincecounty { get; set; }

        [JsonProperty("vaa_postcode")]
        public string VaaPostcode { get; set; }

        /// <summary>
        /// CRM Lookup field 
        /// </summary>
        [JsonProperty("vaa_CountryCode@odata.bind")]
        public string VaaCountryCode { get; set; }


        [JsonProperty("vaa_CustomerCountry@odata.bind")]
        public string VaaCustomerCountry { get; set; }

        [JsonProperty("vaa_bankaccountdetailsentered")]
        public bool VaaBankaccountdetailsentered { get; set; }

        [JsonProperty("vaa_beneficiaryname")]
        public string VaaBeneficiaryName { get; set; }

        [JsonProperty("vaa_paymentstage")]
        public int VaaPaymentStage { get; set; }
    }
}
