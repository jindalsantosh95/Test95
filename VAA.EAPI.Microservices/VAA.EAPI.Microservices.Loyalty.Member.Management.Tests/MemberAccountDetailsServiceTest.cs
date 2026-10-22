using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MembershipChange;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Award;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class MemberAccountDetailsServiceTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<MemberAccountDetailsService>> _mockLogger;
        private readonly MemberAccountDetailsService _memberAccountDetailsService;
        private readonly Mock<ICachedPartners> _mockCachedPartners;
        private readonly Mock<ICommonRefDBServices> _mockCommonRefDBServices;

        public MemberAccountDetailsServiceTest()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<MemberAccountDetailsService>>();
            _mockCachedPartners = new Mock<ICachedPartners>();
            _mockCommonRefDBServices = new Mock<ICommonRefDBServices>();
            _memberAccountDetailsService = new MemberAccountDetailsService(_mockMediator.Object, _mockLogger.Object, _mockCachedPartners.Object, _mockCommonRefDBServices.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountDetailsService(_mockMediator.Object, null, _mockCachedPartners.Object, _mockCommonRefDBServices.Object));
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new MemberAccountDetailsService(null, _mockLogger.Object, _mockCachedPartners.Object, _mockCommonRefDBServices.Object));
        }

        [Fact]
        public async Task GetMemberAccountDetailsShouldSucceed()
        {
            var request = TestDataProvider.GetTestData<MemberAccountRequestQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocument");
            var memberchangeDoc = TestDataProvider.GetTestData<IEnumerable<MembershipChange>>("MemberManagement\\MembershipChangeDocument");
            var transactionDoc = TestDataProvider.GetTestData<IEnumerable<TransactionDocument>>("MemberManagement\\TransactionDocument");
            var partnersResponse = new Response<PartnersResponse>
            {
                Status = Status.OK,
                Value = new PartnersResponse
                {
                    Partners = new List<PartnerProfile>
                    {
                        new PartnerProfile
                        {
                            PartnerId = "1-22XT",
                            PartnerCode = "VS",
                            Active = true
                        }
                    }
                }
            };
            var productResponse = new Product
            {
                ProductId = "1-288X",
                ProductName = "Flight"
            };
            var awardDoc = new List<Award>
            {
                new Award
                {
                    AwardNumber = "W001",
                    ProductId = request.ProductId,
                    StartDate = DateTime.UtcNow.AddMonths(-6),
                    ExpiryDate = DateTime.UtcNow.AddMonths(6),
                    Status = "Available",
                    TransactionId = "TX001"
                }
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IEnumerable<MembershipChange>> { Status = Status.OK, Value = memberchangeDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerTransactionDocumentsServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IEnumerable<TransactionDocument>> { Status = Status.OK, Value = transactionDoc });
            _mockCachedPartners.Setup(x => x.GetCachedPartnersReferenceData()).ReturnsAsync(CSharpFunctionalExtensions.Result.Success(partnersResponse));
            _mockCommonRefDBServices.Setup(x => x.GetProductReferenceDataByName(It.IsAny<string>())).ReturnsAsync(productResponse);
            _mockMediator.Setup(m => m.Send(It.IsAny<AwardDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IEnumerable<Award>> { Status = Status.OK, Value = awardDoc });

            var result = await _memberAccountDetailsService.GetMemberAccountDetails(request);
            var value = result.Value.Value;
            Assert.NotNull(value.loyaltyMemberAccount);
            Assert.NotNull(value.loyaltyMemberAccount.RollingCoreTierPoints);
            Assert.NotNull(value.loyaltyMemberAccount.RetainCoreTierPoints);
            Assert.NotNull(value.loyaltyMemberAccount.CoreTierPointsSinceBecomingWings);
        }

        [Fact]
        public async Task GetMemberAccountDetailsFlyingClubShouldThrowException()
        {
            var request = TestDataProvider.GetTestData<MemberAccountRequestQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocument");
            var memberchangeDoc = TestDataProvider.GetTestData<IEnumerable<MembershipChange>>("MemberManagement\\MembershipChangeDocument");
            var transactionDoc = TestDataProvider.GetTestData<IEnumerable<TransactionDocument>>("MemberManagement\\TransactionDocument");
            var partnersResponse = new Response<PartnersResponse>
            {
                Status = Status.OK,
                Value = new PartnersResponse
                {
                    Partners = new List<PartnerProfile>
                    {
                        new PartnerProfile
                        {
                            PartnerId = "1-22XT",
                            PartnerCode = "VS",
                            Active = true
                        }
                    }
                }
            };
            var productResponse = new Product
            {
                ProductId = "1-288X",
                ProductName = "Flight"
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IEnumerable<MembershipChange>> { Status = Status.OK, Value = memberchangeDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerTransactionDocumentsServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IEnumerable<TransactionDocument>> { Status = Status.OK, Value = transactionDoc });
            _mockCachedPartners.Setup(x => x.GetCachedPartnersReferenceData()).ReturnsAsync(CSharpFunctionalExtensions.Result.Success(partnersResponse));
            _mockCommonRefDBServices.Setup(x => x.GetProductReferenceDataByName(It.IsAny<string>())).ReturnsAsync(productResponse);

            var result = await _memberAccountDetailsService.GetMemberAccountDetails(request);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task GetMemberAccountDetailsMemberChangeShouldThrowException()
        {
            var request = TestDataProvider.GetTestData<MemberAccountRequestQuery>("MemberManagement\\RetrieveMemberAccountDetailsRequest");
            var flyingclubDoc = TestDataProvider.GetTestData<FlyingClubMember>("MemberManagement\\FlyingclubMemberDocument");
            var memberchangeDoc = TestDataProvider.GetTestData<IEnumerable<MembershipChange>>("MemberManagement\\MembershipChangeDocument");
            var transactionDoc = TestDataProvider.GetTestData<IEnumerable<TransactionDocument>>("MemberManagement\\TransactionDocument");
            var partnersResponse = new Response<PartnersResponse>
            {
                Status = Status.OK,
                Value = new PartnersResponse
                {
                    Partners = new List<PartnerProfile>
                    {
                        new PartnerProfile
                        {
                            PartnerId = "1-22XT",
                            PartnerCode = "VS",
                            Active = true
                        }
                    }
                }
            };
            var productResponse = new Product
            {
                ProductId = "1-288X",
                ProductName = "Flight"
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<FlyingClubMemberDocumentServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<FlyingClubMember> { Status = Status.OK, Value = flyingclubDoc });
            _mockMediator.Setup(m => m.Send(It.IsAny<MembershipChangeDocumentServiceQuery>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerTransactionDocumentsServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response<IEnumerable<TransactionDocument>> { Status = Status.OK, Value = transactionDoc });
            _mockCachedPartners.Setup(x => x.GetCachedPartnersReferenceData()).ReturnsAsync(CSharpFunctionalExtensions.Result.Success(partnersResponse));
            _mockCommonRefDBServices.Setup(x => x.GetProductReferenceDataByName(It.IsAny<string>())).ReturnsAsync(productResponse);

            var result = await _memberAccountDetailsService.GetMemberAccountDetails(request);
            var value = result.Value.Value;
            Assert.Null(value.loyaltyMemberAccount.TierStartDate);
        }
    }
}
