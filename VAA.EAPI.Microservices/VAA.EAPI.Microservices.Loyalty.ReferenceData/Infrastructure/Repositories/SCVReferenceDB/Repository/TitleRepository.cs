#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Repository;

public sealed class TitleRepository(ScvReferenceDbContext dbContext, ICacheService cacheService) : ITitleRepository
{
    public async Task<IReadOnlyCollection<TitleDetails>> GetAllTitlesAsync(CancellationToken cancellationToken)
    {
        var cachedTitles = await cacheService.GetCacheValue<IReadOnlyCollection<TitleDetails>?>(
            Constants.Application.TitleMasterCacheKey);

        if (cachedTitles is not null)
        {
            return cachedTitles;
        }

        IReadOnlyCollection<TitleDetails> titles = await dbContext.TitleMaster
            .AsNoTracking()
            .OrderBy(title => title.Priority)
            .ThenBy(title => title.Title)
            .Select(title => new TitleDetails
            {
                Title = title.Title
            })
            .ToListAsync(cancellationToken);

        await cacheService.SetCacheValue(Constants.Application.TitleMasterCacheKey, titles, true);

        return titles;
    }
}
