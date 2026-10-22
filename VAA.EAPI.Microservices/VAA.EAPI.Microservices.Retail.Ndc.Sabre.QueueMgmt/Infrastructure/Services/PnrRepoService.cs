using CSharpFunctionalExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Infrastructure.Services
{
    public class PnrRepoService : IPnrRepoService
    {
        private readonly IDuplicatePnrRepoCommand _duplicatePnrRepoCommand;
        private readonly IRemoveDuplicatePnrRepoCommand _removeDuplicatePnrRepoCommand;

        public PnrRepoService(IDuplicatePnrRepoCommand duplicatePnrRepoCommand, IRemoveDuplicatePnrRepoCommand removeDuplicatePnrRepoCommand)
        {
            _duplicatePnrRepoCommand = duplicatePnrRepoCommand ?? throw new ArgumentNullException(nameof(duplicatePnrRepoCommand));
            _removeDuplicatePnrRepoCommand = removeDuplicatePnrRepoCommand ?? throw new ArgumentNullException(nameof(removeDuplicatePnrRepoCommand));
        }

        public Task<Result<Response<DuplicatePnrResponse>>> FetchDuplicatePnrs(string queue, string pcc, CancellationToken cancellationToken)
        {
            return _duplicatePnrRepoCommand.Handle(new DuplicatePnrRequest { Queue = queue, PCC = pcc }, cancellationToken);
        }

        public Task<Result<Response<RemoveDuplicatePnrResponse>>> RemoveDuplicate(string pcc, string pnr, CancellationToken cancellationToken)
        {
            return _removeDuplicatePnrRepoCommand.Handle(new RemoveDuplicatePnrRequest { PCC = pcc, Pnr = pnr }, cancellationToken);
        }
    }
}