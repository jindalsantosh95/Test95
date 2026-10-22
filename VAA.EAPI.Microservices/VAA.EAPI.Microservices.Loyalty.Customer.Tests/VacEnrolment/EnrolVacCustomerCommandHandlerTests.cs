using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.VacEnrolment
{
    public class EnrolVacCustomerCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<EnrolVacCustomerCommandHandler>> _mockLogger;
        private readonly Mock<ICustomerRepository> _mockCustomerRepository;
        private readonly Mock<ICustomerSearchRepository> _mockCustomerSearchRepository;
        private readonly Mock<ICustomerProfileRepository> _mockCustomerProfileRepository;
        private readonly Mock<IMemberProfileRepository> _mockMemberProfileRepository;
        private readonly Mock<IPartnerDocumentRepository> _mockPartnerDocumentRepository;
        private readonly Mock<IMembershipChangeRepository> _mockMembershipChangeRepository;
        private readonly Mock<IGetCustTransDocumentService<MembershipChange>> _mockCustTransServiceMemberChange;
        private readonly Mock<IGetCustTransDocumentService<MemberProfile>> _mockCustoTransServiceMbrProfile;
        private readonly Mock<IGetCustTransDocumentService<PartnerMembership>> _mockCustoTransServiceMbrPartnerMbr;
        private readonly Mock<IGetCustTransDocumentService<CustomerProfile>> _mockCustoTransServiceCustomerProfile;
        private readonly EnrolVacCustomerCommandHandler _handler;

        public EnrolVacCustomerCommandHandlerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<EnrolVacCustomerCommandHandler>>();
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockCustomerSearchRepository = new Mock<ICustomerSearchRepository>();
            _mockCustomerProfileRepository = new Mock<ICustomerProfileRepository>();
            _mockMemberProfileRepository = new Mock<IMemberProfileRepository>();
            _mockPartnerDocumentRepository = new Mock<IPartnerDocumentRepository>();
            _mockMembershipChangeRepository = new Mock<IMembershipChangeRepository>();
            _mockCustTransServiceMemberChange = new Mock<IGetCustTransDocumentService<MembershipChange>>();
            _mockCustoTransServiceMbrProfile = new Mock<IGetCustTransDocumentService<MemberProfile>>();
            _mockCustoTransServiceMbrPartnerMbr = new Mock<IGetCustTransDocumentService<PartnerMembership>>();
            _mockCustoTransServiceCustomerProfile = new Mock<IGetCustTransDocumentService<CustomerProfile>>();
            _handler = new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object);
            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMediatorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(null, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, null, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, null, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMemberProfileRepositorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, null, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerSearchRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, null, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerProfileRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, null, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenPartnerDocumentRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, null, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenMembershipChangeRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, null, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustTransServiceMemberChangeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, null, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustoTransServiceMbrProfileIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, null, _mockCustoTransServiceMbrPartnerMbr.Object, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustoTransServiceMbrPartnerMbrIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, null, _mockCustoTransServiceCustomerProfile.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustoTransServiceCustomerProfileIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EnrolVacCustomerCommandHandler(_mockMediator.Object, _mockLogger.Object, _mockCustomerRepository.Object, _mockMemberProfileRepository.Object, _mockCustomerSearchRepository.Object, _mockCustomerProfileRepository.Object, _mockPartnerDocumentRepository.Object, _mockMembershipChangeRepository.Object, _mockCustTransServiceMemberChange.Object, _mockCustoTransServiceMbrProfile.Object, _mockCustoTransServiceMbrPartnerMbr.Object, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowError_ForNullRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldCreateDocumnet_ForNewVacMember()
        {
            //Arrage
            var vacEnrolmentRequest = TestDataProvider.GetTestData<VacEventEnrolmentCommand>("CustomerMS\\VACEnrolment\\VacEnrolmenetRequest");
            var docCreationRequest = new EnrolVacCustomerCommand(Json.Deserialize<CreateCustomerEntities>(vacEnrolmentRequest.Data.ToString()), false);

            //Act
            var result = await _handler.Handle(docCreationRequest, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Unit>(result.Value);
        }

        [Fact]
        public async Task Handler_ShouldUpdateDocumnet_ForBasicMember()
        {
            //Arrage
            var vacEnrolmentRequest = TestDataProvider.GetTestData<VacEventEnrolmentCommand>("CustomerMS\\VACEnrolment\\VacUpgradeRequest");
            var docCreationRequest = new EnrolVacCustomerCommand(Json.Deserialize<CreateCustomerEntities>(vacEnrolmentRequest.Data.ToString()), true);

            var customerDocResponse = new Response<Customers>
            {
                Value = TestDataProvider.GetTestData<Customers>("CustomerMS\\CustomerDocument\\CustomerDoc")
            };

            var memberShipChangeDocResponse = TestDataProvider.GetTestData<MembershipChange>("MembershipChanges\\MemberShipChangeDocument");

            var flyingClubMemberDocResponse = TestDataProvider.GetTestData<MemberProfile>("FlyingClubMember\\FcmDoc");

            var partnerMemberShipDocResponse = TestDataProvider.GetTestData<PartnerMembership>("PartnerMembership\\PartnerMemberShipDoc");

            var customerProfileDocResponse = TestDataProvider.GetTestData<CustomerProfile>("CustomerProfile\\CustomerProfileDoc");

            //Act
            _mockMediator.Setup(c => c.Send(It.IsAny<FetchCustomerDocumentQuery>(), CancellationToken.None)).ReturnsAsync(customerDocResponse);

            _mockCustTransServiceMemberChange.Setup(x => x.GetCosmosDoc(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(memberShipChangeDocResponse);

            _mockCustoTransServiceMbrProfile.Setup(x => x.GetCosmosDoc(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flyingClubMemberDocResponse);

            _mockCustoTransServiceMbrPartnerMbr.Setup(x => x.GetCosmosDoc(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(partnerMemberShipDocResponse);

            _mockCustoTransServiceCustomerProfile.Setup(x => x.GetCosmosDoc(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerProfileDocResponse);

            var result = await _handler.Handle(docCreationRequest, CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.IsType<Unit>(result.Value);
        }
    }
}
