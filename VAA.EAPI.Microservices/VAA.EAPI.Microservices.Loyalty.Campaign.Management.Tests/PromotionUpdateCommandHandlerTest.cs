using Moq;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Application.Command.PromotionUpdate;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class PromotionUpdateCommandHandlerTest
    {
        private readonly Mock<ICommonRefDBRepository> _commonRefDBRepository;
        private readonly PromotionUpdateCommandHandler _handler;

        public PromotionUpdateCommandHandlerTest()
        {
            _commonRefDBRepository = new Mock<ICommonRefDBRepository>();
            _handler = new PromotionUpdateCommandHandler(_commonRefDBRepository.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PromotionUpdateCommandHandler(null));
        }

        [Fact]
        public void HandlerShouldSuccess()
        {
            PromotionUpdateCommand req = TestDataProvider.GetTestData<PromotionUpdateCommand>("CampaignManagementMS\\PromotionUpdateValid");

            _commonRefDBRepository.Setup(x => x.UpdatePromotion(req.PromotionId, null, null, null, null, null, null, null)).ReturnsAsync("Success");

            var result = _handler.Handle(req, System.Threading.CancellationToken.None);

            Assert.Equal("RanToCompletion", result.Status.ToString());
        }
    }
}
