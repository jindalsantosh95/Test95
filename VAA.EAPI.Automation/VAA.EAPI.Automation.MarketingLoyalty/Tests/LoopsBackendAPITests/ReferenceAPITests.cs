using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsBackendAPITests
{

    [Collection("ReferenceAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class ReferenceAPITests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public ReferenceAPITests(TokenFixture tokenFixture)
        {
            this.tokenFixture = tokenFixture;

            if (!backendAPIEnv.Is("Project-Dev"))
                this.tokenFixture.AddTokenForReferenceApiFromKeyVault("reference");
        }

        [Trait("Category", Traits.LOOPSReferenceAPIRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_RetrieveListOfPartners()
        {

            IEnumerable<GetPartners> partnersList = AzureReferenceDBHelper.GetPartnerDetails();

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Reference.GetPartners(tokenFixture.GetToken("reference")).EnsureIsSuccessStatusCode();

            IEnumerable<GetPartners> partnerDetails = JsonConvert.DeserializeObject<IEnumerable<GetPartners>>(await response.Content.ReadAsStringAsync());

            Assert.True(partnerDetails.Count().Equals(partnersList.Count()),
                $"Expected the Partners Count to be {partnersList.Count()} but was {partnerDetails.Count()}");

            int i = 0;

            foreach (GetPartners getPartners in partnersList)
            {

                Assert.True(getPartners.PartnerId.Equals(partnerDetails.ElementAt(i).PartnerId),
                    $"Expected the Partner Id to be  {getPartners.PartnerId} but was {partnerDetails.ElementAt(i).PartnerId}");

                Assert.True(getPartners.PartnerName.Equals(partnerDetails.ElementAt(i).PartnerName),
                    $"Expected the Partner Name to be {getPartners.PartnerName} but was {partnerDetails.ElementAt(i).PartnerName}");

                Assert.True(getPartners.PartnerCode.Equals(partnerDetails.ElementAt(i).PartnerCode),
                    $"Expected the Partner Code to be {getPartners.PartnerCode} but was {partnerDetails.ElementAt(i).PartnerCode}");

                Assert.True(getPartners.PartnerType.Equals(partnerDetails.ElementAt(i).PartnerType),
                    $"Expected the Partner Type to be {getPartners.PartnerType} but was {partnerDetails.ElementAt(i).PartnerType}");

                Assert.True(getPartners.PartnerSubType.Equals(partnerDetails.ElementAt(i).PartnerSubType),
                    $"Expected the Partner SubType to be {getPartners.PartnerSubType} but was {partnerDetails.ElementAt(i).PartnerSubType}");

                Assert.True(getPartners.CodeShareFlag.Equals(partnerDetails.ElementAt(i).CodeShareFlag),
                    $"Expected the Partner CodeShareFlag to be {getPartners.CodeShareFlag} but was {partnerDetails.ElementAt(i).CodeShareFlag}");

                Assert.True(getPartners.PartnerCategory.Equals(partnerDetails.ElementAt(i).PartnerCategory),
                    $"Expected the Partner Category to be {getPartners.PartnerCategory} but was {partnerDetails.ElementAt(i).PartnerCategory}");

                i++;

            }

        }

        [Trait("Category", Traits.LOOPSReferenceAPIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_RetrieveListOfCountriesWithCountryCode()
        {

            IEnumerable<GetCountries> countryList = AzureReferenceDBHelper.GetCountryDetails();

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Reference.GetCountries(tokenFixture.GetToken("reference")).EnsureIsSuccessStatusCode();

            IEnumerable<GetCountries> countryDetails = JsonConvert.DeserializeObject<IEnumerable<GetCountries>>(await response.Content.ReadAsStringAsync());

            Assert.True(countryDetails.Count().Equals(countryList.Count()),
                $"Expected the Total Countries Count to be {countryList.Count()} but was {countryDetails.Count()}");

            int i = 0;

            foreach (GetCountries getCountries in countryList)
            {

                Assert.True(getCountries.Name.Equals(countryDetails.ElementAt(i).Name),
                    $"Expected the Country Name to be  {getCountries.Name} but was {countryDetails.ElementAt(i).Name}");

                Assert.True(getCountries.Code.Equals(countryDetails.ElementAt(i).Code),
                    $"Expected the Country Code to be {getCountries.Code} for the Country {getCountries.Name} but was {countryDetails.ElementAt(i).Code}");

                i++;

            }

        }

        [Trait("Category", Traits.LOOPSReferenceAPIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_RetrieveListOfMediaCodes()
        {

            IEnumerable<GetListOfMediaCodes> mediaCodeList = AzureReferenceDBHelper.GetMediaCodeDetails();

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Reference.GetListOfMediaCodes(tokenFixture.GetToken("reference")).EnsureIsSuccessStatusCode();

            IEnumerable<GetListOfMediaCodes> mediaCodeDetails = JsonConvert.DeserializeObject<IEnumerable<GetListOfMediaCodes>>(await response.Content.ReadAsStringAsync());

            Assert.True(mediaCodeList.Count().Equals(mediaCodeList.Count()),
                $"Expected the Total MediaCodes Count to be {mediaCodeList.Count()} but was {mediaCodeDetails.Count()}");

            int i = 0;

            foreach (GetListOfMediaCodes mediaCodes in mediaCodeList)
            {

                Assert.True(mediaCodes.MediaCode.Equals(mediaCodeDetails.ElementAt(i).MediaCode),
                    $"Expected the Media Code to be  {mediaCodes.MediaCode} but was {mediaCodeDetails.ElementAt(i).MediaCode}");

                Assert.True(mediaCodes.MediaCodeCategory.Equals(mediaCodeDetails.ElementAt(i).MediaCodeCategory, StringComparison.OrdinalIgnoreCase),
                   $"Expected the Media Code Category to be  {mediaCodes.MediaCodeCategory} but was {mediaCodeDetails.ElementAt(i).MediaCodeCategory}");

                Assert.True(mediaCodes.MediaCodeDescription.Equals(mediaCodeDetails.ElementAt(i).MediaCodeDescription, StringComparison.OrdinalIgnoreCase),
                  $"Expected the Media Code Description to be  {mediaCodes.MediaCodeDescription} but was {mediaCodeDetails.ElementAt(i).MediaCodeDescription}");


                i++;

            }

        }

        [Trait("Category", Traits.LOOPSReferenceAPIRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_RetrieveListOfStatus()
        {

            IEnumerable<GetMemberStatus> memberStatusList = AzureReferenceDBHelper.GetMemberStatusDetails();

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Reference.GetListOfStatus(tokenFixture.GetToken("reference")).EnsureIsSuccessStatusCode();

            IEnumerable<GetMemberStatus> memberStatusDetails = JsonConvert.DeserializeObject<IEnumerable<GetMemberStatus>>(await response.Content.ReadAsStringAsync());

            Assert.True(memberStatusDetails.Count().Equals(memberStatusList.Count()),
                $"Expected the Total Status Count to be {memberStatusList.Count()} but was {memberStatusDetails.Count()}");

            int i = 0;

            foreach (GetMemberStatus memberStatus in memberStatusList)
            {

                Assert.True(memberStatus.ProgramId.Equals(memberStatusDetails.ElementAt(i).ProgramId),
                    $"Expected the ProgramId to be  {memberStatus.ProgramId} but was {memberStatusDetails.ElementAt(i).ProgramId}");

                Assert.True(memberStatus.StatusCode.Equals(memberStatusDetails.ElementAt(i).StatusCode, StringComparison.OrdinalIgnoreCase),
                   $"Expected the Status Code to be  {memberStatus.StatusCode} but was {memberStatusDetails.ElementAt(i).StatusCode}");


                i++;

            }

        }

        [Trait("Category", Traits.LOOPSReferenceAPIRegression)]
        [Fact, RunOrder(5)]
        public async Task TC05_RetrieveListOfProducts()
        {

            IEnumerable<GetListOfProducts> productList = AzureReferenceDBHelper.GetProductDetails();

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Reference.GetListOfProducts(tokenFixture.GetToken("reference")).EnsureIsSuccessStatusCode();


            IEnumerable<GetListOfProducts> productDetails = JsonConvert.DeserializeObject<IEnumerable<GetListOfProducts>>(await response.Content.ReadAsStringAsync());

            Assert.True(productList.Count().Equals(productDetails.Count()),
                $"Expected the Total Products Count to be {productList.Count()} but was {productDetails.Count()}");

            int i = 0;

            foreach (GetListOfProducts products in productList)
            {

                Assert.True(products.ProductId.Equals(productDetails.ElementAt(i).ProductId),
                    $"Expected the ProductId to be {products.ProductId} but was {productDetails.ElementAt(i).ProductId}");

                Assert.True(products.ProductName.Equals(productDetails.ElementAt(i).ProductName),
                    $"Expected the ProductName for the Product Id {products.ProductId} to be  {products.ProductName} but was {productDetails.ElementAt(i).ProductName}");

                Assert.True(products.ProductCategory.Equals(productDetails.ElementAt(i).ProductCategory),
                    $"Expected the ProductCategory for the Product Id {products.ProductId} to be  {products.ProductCategory} but was {productDetails.ElementAt(i).ProductCategory}");

                Assert.True(products.ProductType.Equals(productDetails.ElementAt(i).ProductType),
                   $"Expected the ProductType for the Product Id {products.ProductId} to be  {products.ProductType} but was {productDetails.ElementAt(i).ProductType}");

                Assert.True(products.ProductDescription.Equals(productDetails.ElementAt(i).ProductDescription),
                   $"Expected the Product Description for the Product Id {products.ProductId} to be  {products.ProductCategory} but was {productDetails.ElementAt(i).ProductDescription}");

                i++;

            }

        }


    }
}
