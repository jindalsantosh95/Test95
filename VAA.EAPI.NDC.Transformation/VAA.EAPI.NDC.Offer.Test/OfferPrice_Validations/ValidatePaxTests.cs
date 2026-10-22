using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OfferPriceRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Mapper.GenericDataMapper;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidatePaxTests
    {

        [Fact]
        public void ValidatePax_Error_ForCustomerCountNotMatch_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var genericDataMapper = new GenericDataMapper();

            Customer customer1 = new Customer() { CustId = "ADULT_1", Ptc = "ADT" };
            var offerDB = new OfferResponse() { DataList = new() { CustomerDetails = new List<Customer> { customer1 } } };

            var validationErrors = ValidateOfferPrice.ValidatePax(offerDB, request, genericDataMapper);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_3", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePax_NoError_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var genericDataMapper = new GenericDataMapper();

            Customer customer1 = new Customer() { CustId = "ADULT_1", Ptc = "ADT" };
            Customer customer2 = new Customer() { CustId = "ADULT_2", Ptc = "ADT" };
            Customer customer3 = new Customer() { CustId = "ADULT_3", Ptc = "ADT" };
            Customer customer4 = new Customer() { CustId = "YOUNG_1", Ptc = "GBE" };
            Customer customer5 = new Customer() { CustId = "YOUNG_2", Ptc = "GBE" };
            Customer customer6 = new Customer() { CustId = "CHILD_1", Ptc = "CNN" };

            var offerDB = new OfferResponse() { DataList = new() { CustomerDetails = new List<Customer> { customer1, customer2, customer3, customer4, customer5, customer6 } } };
            
            var validationErrors = ValidateOfferPrice.ValidatePax(offerDB, request, genericDataMapper);

            Assert.True(validationErrors.Count == 0);
        }

        [Fact]
        public void ValidatePax_Error_ForCustomerPtcNotMatch_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            var genericDataMapper = new GenericDataMapper();

            Customer customer1 = new Customer() { CustId = "ADULT_1", Ptc = "ADT" };
            Customer customer2 = new Customer() { CustId = "ADULT_2", Ptc = "ADT" };
            Customer customer3 = new Customer() { CustId = "ADULT_3", Ptc = "ADT" };
            Customer customer4 = new Customer() { CustId = "YOUNG_1", Ptc = "CNN" };
            Customer customer5 = new Customer() { CustId = "YOUNG_2", Ptc = "CNN" };
            Customer customer6 = new Customer() { CustId = "CHILD_1", Ptc = "CHD" };

            var offerDB = new OfferResponse() { DataList = new() { CustomerDetails = new List<Customer> { customer1, customer2, customer3, customer4, customer5, customer6 } } };
            var validationErrors = ValidateOfferPrice.ValidatePax(offerDB, request, genericDataMapper);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_3", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void ValidatePax_Error_ForWrongPtc_Request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OfferPriceRQ>("NDCOffer\\OfferPrice\\OfferPriceRQ");
            request.Request.DataLists.PaxList[4].PTC = "ABC";
            
            var genericDataMapper = new GenericDataMapper();

            Customer customer1 = new Customer() { CustId = "ADULT_1", Ptc = "ADT" };
            Customer customer2 = new Customer() { CustId = "ADULT_2", Ptc = "ADT" };
            Customer customer3 = new Customer() { CustId = "ADULT_3", Ptc = "ADT" };
            Customer customer4 = new Customer() { CustId = "YOUNG_1", Ptc = "GBE" };
            Customer customer5 = new Customer() { CustId = "YOUNG_2", Ptc = "ABC" };
            Customer customer6 = new Customer() { CustId = "CHILD_1", Ptc = "CHD" };

            var offerDB = new OfferResponse() { DataList = new() { CustomerDetails = new List<Customer> { customer1, customer2, customer3, customer4, customer5, customer6 } } };
            var validationErrors = ValidateOfferPrice.ValidatePax(offerDB, request, genericDataMapper);

            Assert.True(validationErrors.Any());
            Assert.Equal("911_3", validationErrors.First().ErrorCode);

        }
    }
}
