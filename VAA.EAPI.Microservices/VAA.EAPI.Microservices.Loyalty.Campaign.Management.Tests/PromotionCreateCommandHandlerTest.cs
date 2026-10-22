using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionCreate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionCreateCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly PromotionCreateCommandHandler _handler;

        public PromotionCreateCommandHandlerTest()
        {
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new PromotionCreateCommandHandler(_commonRefDBRepository.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionCreateCommandHandler(null));
        }

        [Fact]
        public void HandlerShouldSuccess()
        {
            PromotionCreateCommand request = TestDataProvider.GetTestData<PromotionCreateCommand>("CampaignManagementMS\\PromotionCreateValid");

            _commonRefDBRepository.Setup(x => x.CreatePromotion(request.PromotionName, request.PartnerName, request.StartDate, request.EndDate, request.Active, request.InitiatedBy)).ReturnsAsync("Success");

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.Equal("RanToCompletion", result.Status.ToString());
        }
    }
}
