using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyOCNDetails;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Command;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.IFGAgentFopLookUp;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Triggers.Http;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Tests
{
    public class ValidationControllerTests
    {
        [Fact]
        public async Task AgencyOCNDetails_WhenAggregatorIdIsProvided_PassesItToTheQuery()
        {
            var mediator = new CapturingMediator();
            var controller = new ValidationController(mediator, new IFGAgentFopLookUpStub());
            var context = new TestFunctionContext();
            var request = new TestHttpRequestData(
                context,
                new Uri("http://localhost/api/AgencyOCNDetails/123456?aggregatorId=654321"),
                headers: new Dictionary<string, string> { ["environment"] = "sit" });

            await controller.AgencyOCNDetails(request, "123456");

            Assert.NotNull(mediator.AgencyOCNDetailsQuery);
            Assert.Equal("123456", mediator.AgencyOCNDetailsQuery.AgencyId);
            Assert.Equal("654321", mediator.AgencyOCNDetailsQuery.AggregatorId);
            Assert.Equal("sit", mediator.AgencyOCNDetailsQuery.Environment);
        }

        [Theory]
        [InlineData("http://localhost/api/AgencyOCNDetails/123456")]
        [InlineData("http://localhost/api/AgencyOCNDetails/123456?aggregatorId=%20%20%20")]
        public async Task AgencyOCNDetails_WhenAggregatorIdIsOmittedOrBlank_PassesNullToTheQuery(string requestUri)
        {
            var mediator = new CapturingMediator();
            var controller = new ValidationController(mediator, new IFGAgentFopLookUpStub());
            var context = new TestFunctionContext();
            var request = new TestHttpRequestData(
                context,
                new Uri(requestUri),
                headers: new Dictionary<string, string> { ["environment"] = "sit" });

            await controller.AgencyOCNDetails(request, "123456");

            Assert.NotNull(mediator.AgencyOCNDetailsQuery);
            Assert.Null(mediator.AgencyOCNDetailsQuery.AggregatorId);
        }

        private sealed class CapturingMediator : IMediator
        {
            public AgencyOCNDetailsQuery AgencyOCNDetailsQuery { get; private set; }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                if (request is AgencyOCNDetailsQuery agencyOCNDetailsQuery)
                {
                    AgencyOCNDetailsQuery = agencyOCNDetailsQuery;
                    return Task.FromResult((TResponse)(object)new Response<IEnumerable<Domain.AgencyOCNDetails.AgencyOCNDetails>>
                    {
                        Status = Status.OK,
                        Value = []
                    });
                }

                throw new NotSupportedException($"Unsupported request type: {request.GetType().FullName}");
            }

            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task Publish(object notification, CancellationToken cancellationToken = default) => throw new NotSupportedException();
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => throw new NotSupportedException();
        }

        private sealed class IFGAgentFopLookUpStub : IIFGAgentFopLookUp
        {
            public Task<Result<Response<IFGAgentFopLookUpResponse>>> Handle(IFGAgentFopLookUpRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
        }
    }
}
