using MediatR;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Net;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Loyalty.Audit.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Triggers.Http;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class AuditHistoryControllerTest
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly AuditHistoryController _controller;
        private readonly Mock<FunctionContext> _context;

        public AuditHistoryControllerTest()
        {
            _mockMediator = new Mock<IMediator>();
            _context = new Mock<FunctionContext>();
            _controller = new AuditHistoryController(_mockMediator.Object);
        }

        [Fact]
        public void ConstructorShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new AuditHistoryController(null));
        }

        [Fact]
        public void FetchAuditHistoryDocumentsByCustomerIdSuccsess()
        {
            string custID = "edceec11-2eaf-4085-9324-5e1122e1bd07";
            _mockMediator.Setup(m => m.Send(It.IsAny<PreferenceAuditHistoryQuery>(), CancellationToken.None)).ReturnsAsync(new List<PreferenceAudit>());
            var req = MockHttpRequestData.Create(_context.Object);

            var result = _controller.FetchPreferenceHistoryDocumentsByCustomerIdAsync(req.Object, custID);
            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
        }

        [Fact]
        public void FetchAwardAuditHistoryDocumentsByCustomerIdSuccsess()
        {
            string custID = "edceec11-2eaf-4085-9324-5e1122e1bd07";
            _mockMediator.Setup(m => m.Send(It.IsAny<AwardAuditHistoryQuery>(), CancellationToken.None)).ReturnsAsync(new List<AwardAudit>());
            var req = MockHttpRequestData.Create(_context.Object);

            var result = _controller.FetchAwardHistoryDocumentsByCustomerIdAsync(req.Object, custID);
            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
        }

        [Fact]
        public void FetchCustomerProfileAuditHistoryDocumentsByCustomerIdSuccsess()
        {
            string custID = "edceec11-2eaf-4085-9324-5e1122e1bd07";
            _mockMediator.Setup(m => m.Send(It.IsAny<CustomerProfileAuditHistoryQuery>(), CancellationToken.None)).ReturnsAsync(new List<CustomerProfileAudit>());
            var req = MockHttpRequestData.Create(_context.Object);

            var result = _controller.FetchCustomerProfileHistoryDocumentsByCustomerIdAsync(req.Object, custID);
            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
        }

        [Fact]
        public void FetchSupplementarySilverAuditHistoryDocumentsByCustomerIdSuccsess()
        {
            string custID = "edceec11-2eaf-4085-9324-5e1122e1bd07";
            _mockMediator.Setup(m => m.Send(It.IsAny<SupplementarySilverAuditHistoryQuery>(), CancellationToken.None)).ReturnsAsync(new List<LoyaltyGenericAudit>());
            var req = MockHttpRequestData.Create(_context.Object);

            var result = _controller.FetchSupplementarySilverHistoryDocumentsByCustomerIdAsync(req.Object, custID);
            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
        }

        [Fact]
        public void FetchAmexCenturionHistDocsByCustIdSuccsess()
        {
            string custID = "edceec11-2eaf-4085-9324-5e1122e1bd07";
            _mockMediator.Setup(m => m.Send(It.IsAny<AmexCenturionAuditHistoryQuery>(), CancellationToken.None)).ReturnsAsync(new List<CardLinkingAudit>());
            var req = MockHttpRequestData.Create(_context.Object);

            var result = _controller.FetchAmexCenturionHistDocsByCustIdAsync(req.Object, custID);
            Assert.Equal(HttpStatusCode.OK, result.Result.StatusCode);
        }
    }
}