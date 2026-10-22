using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Queries;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class MemberCardLinksQueryTest
    {
        private readonly Mock<IHttpConnector> _mockConnector;
        private readonly Mock<IVredCardDetailsQueryProvider> _vredCardDetailsQueryProvider;
        private readonly Mock<IPartnersProfilesQueryProvider> _partnersProfilesQueryProvider;
        private readonly Mock<IPartnerMembershipDocQueryProvider> _partnerMembershipDocQueryProvider;
        private readonly MemberCardLinksQuery _memberCardLinksQuery;
        private readonly Mock<ILogger<MemberCardLinksQuery>> _logger;
        private readonly Mock<ICustomerProvider> _customerService;

        public MemberCardLinksQueryTest()
        {
            _customerService = new Mock<ICustomerProvider>();
            _mockConnector = new Mock<IHttpConnector>();
            _vredCardDetailsQueryProvider = new Mock<IVredCardDetailsQueryProvider>();
            _partnersProfilesQueryProvider = new Mock<IPartnersProfilesQueryProvider>();
            _partnerMembershipDocQueryProvider = new Mock<IPartnerMembershipDocQueryProvider>();
            _logger = new Mock<ILogger<MemberCardLinksQuery>>();
            _memberCardLinksQuery = new MemberCardLinksQuery(_logger.Object, _vredCardDetailsQueryProvider.Object, _partnersProfilesQueryProvider.Object, _partnerMembershipDocQueryProvider.Object, _customerService.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenCustomerServiceObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MemberCardLinksQuery(null, _vredCardDetailsQueryProvider.Object, _partnersProfilesQueryProvider.Object, _partnerMembershipDocQueryProvider.Object, _customerService.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberCardLinksQuery(_logger.Object, null, _partnersProfilesQueryProvider.Object, _partnerMembershipDocQueryProvider.Object, _customerService.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberCardLinksQuery(_logger.Object, _vredCardDetailsQueryProvider.Object, null, _partnerMembershipDocQueryProvider.Object, _customerService.Object));
            Assert.Throws<ArgumentNullException>(() => new MemberCardLinksQuery(_logger.Object, _vredCardDetailsQueryProvider.Object, _partnersProfilesQueryProvider.Object, null, _customerService.Object));
        }

        [Fact]
        public async Task MemberCardLinkRetrievalShouldSucceed()
        {
            var customerDTO = TestDataProvider.GetTestData<CustomerDTO>("Loyalty\\CustomerDTO");
            customerDTO.Customer.CustomerId = "D971F4D0-3608-84D1-E053-6941D90A4EF2";

            _customerService.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerDTO);

            var vredCardResponse = TestDataProvider.GetTestData<APIResponse<VredCardDetails>>("MemberCardLink\\VredCardDetails_APIResponse");
            var partnerProfilesResponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<PartnerProfileDetails>>>("MemberCardLink\\PartnersProfiles_APIResponse");
            var partnerMembershipResponse = TestDataProvider.GetTestData<APIResponse<PartnerMembershipResponse>>("MemberCardLink\\PartnerMembership_APIResponse");
            var membercardresponse = TestDataProvider.GetTestData<APIResponse<IEnumerable<MemberCardLinkResponse>>>("MemberCardLink\\MemberCardLink_APIResponse");

            var customerId = "D971F4D0-3608-84D1-E053-6941D90A4EF2";
            var MemberNumber = "9106913073";

            _vredCardDetailsQueryProvider.Setup(m => m.GetVredCardDetails(MemberNumber)).ReturnsAsync(vredCardResponse.Response);
            _partnersProfilesQueryProvider.Setup(m => m.GetPartnerProfileDetails()).ReturnsAsync(partnerProfilesResponse.Response);
            _partnerMembershipDocQueryProvider.Setup(m => m.GetPartnerMembershipDoc(customerId)).ReturnsAsync(partnerMembershipResponse.Response);

            _mockConnector.Setup(m => m.GetAsync<IEnumerable<MemberCardLinkResponse>>(It.IsAny<string>(),
                                                                     It.IsAny<Dictionary<string, string>>(),
                                                                     It.IsAny<string>())).ReturnsAsync(membercardresponse);

            var queryResponse = await _memberCardLinksQuery.GetMemberCardLink(customerId, MemberNumber, _customerService.Object);

            Assert.NotNull(queryResponse);
            Assert.True(queryResponse.Any());
            Assert.True(queryResponse.Last().PartnerCode == membercardresponse.Response.Last().PartnerCode);
        }
    }
}
