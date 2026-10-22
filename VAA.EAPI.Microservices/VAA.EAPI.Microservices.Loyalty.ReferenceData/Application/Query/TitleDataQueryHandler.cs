#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;

public sealed class TitleDataQueryHandler(ITitleRepository titleRepository, ILogger<TitleDataQueryHandler> logger) : ITitleDataQueryHandler
{
    public async Task<Result<Response<IReadOnlyCollection<TitleDetails>>>> Handle(CancellationToken cancellationToken)
    {
        try
        {
            var titles = await titleRepository.GetAllTitlesAsync(cancellationToken);

            return Result.Success(new Response<IReadOnlyCollection<TitleDetails>>
            {
                Status = Status.OK,
                Value = titles
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to retrieve titles");
            return Result.Failure<Response<IReadOnlyCollection<TitleDetails>>>("Unable to retrieve titles");
        }
    }
}
