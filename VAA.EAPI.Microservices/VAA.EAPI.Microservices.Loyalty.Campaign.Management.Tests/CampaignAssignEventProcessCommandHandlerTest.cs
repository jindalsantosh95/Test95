using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRetrieval;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper.Cache.Interface;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Models;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Services.Interface;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public class CampaignAssignEventProcessCommandHandlerTest
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CampaignAssignEventProcessCommandHandler>> _logger;
        private readonly Mock<ICachedCampaignDetailsById> _cachedCampaignDetailsById;
        private readonly Mock<ICustomerProvider> _customerProvider;
        private readonly Mock<ITargetedMemberDocumentService> _targetedMemberDocumentService;
        private readonly CampaignAssignEventProcessCommandHandler _handler;

        public CampaignAssignEventProcessCommandHandlerTest()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CampaignAssignEventProcessCommandHandler>>();
            _cachedCampaignDetailsById = new Mock<ICachedCampaignDetailsById>();
            _customerProvider = new Mock<ICustomerProvider>();
            _targetedMemberDocumentService = new Mock<ITargetedMemberDocumentService>();
            _handler = new CampaignAssignEventProcessCommandHandler(_mediator.Object, _logger.Object, _cachedCampaignDetailsById.Object, _customerProvider.Object, _targetedMemberDocumentService.Object);

            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };

            CampaignManagementHelpers.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void ConstructorShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new CampaignAssignEventProcessCommandHandler(null, _logger.Object, _cachedCampaignDetailsById.Object, _customerProvider.Object, _targetedMemberDocumentService.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CampaignAssignEventProcessCommandHandler(_mediator.Object, null, _cachedCampaignDetailsById.Object, _customerProvider.Object, _targetedMemberDocumentService.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CampaignAssignEventProcessCommandHandler(_mediator.Object, _logger.Object, null, _customerProvider.Object, _targetedMemberDocumentService.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CampaignAssignEventProcessCommandHandler(_mediator.Object, _logger.Object, _cachedCampaignDetailsById.Object, null, _targetedMemberDocumentService.Object));

            Assert.Throws<ArgumentNullException>(() =>
           new CampaignAssignEventProcessCommandHandler(_mediator.Object, _logger.Object, _cachedCampaignDetailsById.Object, _customerProvider.Object, null));
        }

        [Fact]
        public void HandlerShouldSuccessWithNoErrorCustomerIdNotNull()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldSuccessWithNoErrorCustomerIdNull()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            request.Data = request.Data.ToString().Replace("c220a7fb-8b85-4f4f-bac8-db94252a3e45", "");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.Null(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldSuccessWithInvalidRequest()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            request.Data = request.Data.ToString().Replace("1-3W-233", "");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldSuccessForDuplicateWhenCustomerIdNotNull()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            targetedMembershipResponse.CustomerId = customerResponse.Customer.CustomerId;
            request.Data = request.Data.ToString().Replace("1-3W-233", "1-F8-41");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldSuccessForDuplicateWhenCustomerIdNull()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            targetedMembershipResponse.CustomerId = customerResponse.Customer.CustomerId;
            request.Data = request.Data.ToString().Replace("1-3W-233", "1-F8-41");
            request.Data = request.Data.ToString().Replace("c220a7fb-8b85-4f4f-bac8-db94252a3e45", "");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }


        [Fact]
        public void HandlerShouldFailWhenCustomerDocIsNull()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            request.Data = request.Data.ToString().Replace("c220a7fb-8b85-4f4f-bac8-db94252a3e45", "");
            CustomerDTO customerResponse = null;

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsFailure);
        }

        [Fact]
        public void HandlerShouldSuccessWithInvalidCampaign()
        {
            CampaignDetails campaignData = null;
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsSuccess);
            Assert.NotNull(result.Result.Value.Errors);
        }

        [Fact]
        public void HandlerShouldFailWithException()
        {
            CampaignDetails campaignData = TestDataProvider.GetTestData<CampaignDetails>("CampaignManagementMS\\CampaignDetails");
            CampaignAssignEventProcessCommand request = TestDataProvider.GetTestData<CampaignAssignEventProcessCommand>("CampaignManagementMS\\CampaignAssignCloudEvent");
            CustomerDTO customerResponse = TestDataProvider.GetTestData<CustomerDTO>("CampaignManagementMS\\CustomerDTO");
            TargetedMembershipResponse targetedMembershipResponse = TestDataProvider.GetTestData<TargetedMembershipResponse>("CampaignManagementMS\\TargetedMembershipDocResponse");

            _mediator.Setup(m => m.Send(It.IsAny<CampaignAssignEventProcessCommandHandler>(), System.Threading.CancellationToken.None)).ReturnsAsync(new Response<string> { });
            _cachedCampaignDetailsById.Setup(m => m.GetCachedCampaignDetailsById(It.IsAny<string>())).ReturnsAsync(campaignData);
            _customerProvider.Setup(x => x.GetCustomer(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(customerResponse);
            _targetedMemberDocumentService.Setup(x => x.GetMemberCampaigns(It.IsAny<string>())).ReturnsAsync(targetedMembershipResponse);
            _mediator.Setup(m => m.Publish(It.IsAny<CloudEvent>(), System.Threading.CancellationToken.None)).Throws(new Exception());

            var result = _handler.Handle(request, System.Threading.CancellationToken.None);

            Assert.True(result.Result.IsFailure);
        }
    }
}
