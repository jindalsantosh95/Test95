using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Tests
{
    public sealed class PnrRepoServiceTests
    {
        [Fact]
        public async Task FetchDuplicatePnrs_PassesQueueAndPccToRepoCommand()
        {
            var tokenSource = new CancellationTokenSource();
            var repoCommand = new FakeDuplicatePnrRepoCommand();
            var service = new PnrRepoService(repoCommand, new FakeRemoveDuplicatePnrRepoCommand());

            var result = await service.FetchDuplicatePnrs("20", "1AB", tokenSource.Token);

            Assert.True(result.IsSuccess);
            Assert.NotNull(repoCommand.LastRequest);
            Assert.Equal("20", repoCommand.LastRequest!.Queue);
            Assert.Equal("1AB", repoCommand.LastRequest!.PCC);
            Assert.Equal(tokenSource.Token, repoCommand.LastCancellationToken);
        }

        [Fact]
        public async Task RemoveDuplicate_PassesPccAndPnrToRepoCommand()
        {
            var tokenSource = new CancellationTokenSource();
            var repoCommand = new FakeRemoveDuplicatePnrRepoCommand();
            var service = new PnrRepoService(new FakeDuplicatePnrRepoCommand(), repoCommand);

            var result = await service.RemoveDuplicate("9ZZ", "PLQHCB", tokenSource.Token);

            Assert.True(result.IsSuccess);
            Assert.NotNull(repoCommand.LastRequest);
            Assert.Equal("9ZZ", repoCommand.LastRequest!.PCC);
            Assert.Equal("PLQHCB", repoCommand.LastRequest.Pnr);
            Assert.Equal(tokenSource.Token, repoCommand.LastCancellationToken);
        }

        private sealed class FakeDuplicatePnrRepoCommand : IDuplicatePnrRepoCommand
        {
            public DuplicatePnrRequest? LastRequest { get; private set; }
            public CancellationToken LastCancellationToken { get; private set; }

            public Task<Result<Response<DuplicatePnrResponse>>> Handle(DuplicatePnrRequest request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                LastCancellationToken = cancellationToken;

                return Task.FromResult(Result.Success(new Response<DuplicatePnrResponse>
                {
                    Status = Status.OK,
                    Value = new DuplicatePnrResponse()
                }));
            }
        }

        private sealed class FakeRemoveDuplicatePnrRepoCommand : IRemoveDuplicatePnrRepoCommand
        {
            public RemoveDuplicatePnrRequest? LastRequest { get; private set; }
            public CancellationToken LastCancellationToken { get; private set; }

            public Task<Result<Response<RemoveDuplicatePnrResponse>>> Handle(RemoveDuplicatePnrRequest request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                LastCancellationToken = cancellationToken;

                return Task.FromResult(Result.Success(new Response<RemoveDuplicatePnrResponse>
                {
                    Status = Status.OK,
                    Value = new RemoveDuplicatePnrResponse()
                }));
            }
        }
    }
}

