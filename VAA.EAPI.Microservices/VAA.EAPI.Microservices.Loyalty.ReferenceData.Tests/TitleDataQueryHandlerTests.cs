using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Query;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Tests;

public sealed class TitleDataQueryHandlerTests
{
    private readonly IServiceProvider _serviceProvider;

    public TitleDataQueryHandlerTests()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<FakeTitleRepository>()
            .AddSingleton<ITitleRepository>(serviceProvider => serviceProvider.GetRequiredService<FakeTitleRepository>())
            .AddSingleton<ITitleDataQueryHandler, TitleDataQueryHandler>()
            .BuildServiceProvider();
    }

    [Fact]
    public async Task Handle_ReturnsAllTitlesFromRepository()
    {
        var titles = new[]
        {
            new TitleDetails { Title = "Mr" },
            new TitleDetails { Title = "Mrs" },
            new TitleDetails { Title = "Ms" },
            new TitleDetails { Title = "Miss" },
            new TitleDetails { Title = "Mstr" },
            new TitleDetails { Title = "Prof" },
            new TitleDetails { Title = "Captain" },
            new TitleDetails { Title = "Reverend" },
            new TitleDetails { Title = "Sir" },
            new TitleDetails { Title = "Lady" },
            new TitleDetails { Title = "Lord" },
            new TitleDetails { Title = "Dame" },
            new TitleDetails { Title = "Count" },
            new TitleDetails { Title = "Countess" },
            new TitleDetails { Title = "Baroness" },
            new TitleDetails { Title = "Baron" },
            new TitleDetails { Title = "Sheikh" },
            new TitleDetails { Title = "Master" },
            new TitleDetails { Title = "Hon" },
            new TitleDetails { Title = "Eng" },
            new TitleDetails { Title = "Chief" },
            new TitleDetails { Title = "Prince" },
            new TitleDetails { Title = "Justice" },
            new TitleDetails { Title = "Princess" },
            new TitleDetails { Title = "" },
            new TitleDetails { Title = "." },
            new TitleDetails { Title = "Major" },
            new TitleDetails { Title = "Madam" },
            new TitleDetails { Title = "Comm" },
            new TitleDetails { Title = "Viscountess" },
            new TitleDetails { Title = "Viscount" },
            new TitleDetails { Title = "Laird" },
            new TitleDetails { Title = "Mayor" },
            new TitleDetails { Title = "Mx" },
            new TitleDetails { Title = "Dr" }
        };
        var repository = _serviceProvider.GetRequiredService<FakeTitleRepository>();
        repository.SetTitles(titles);
        var handler = _serviceProvider.GetRequiredService<ITitleDataQueryHandler>();

        var result = await handler.Handle(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(titles.Select(title => title.Title), result.Value.Value.Select(title => title.Title));
    }

    [Fact]
    public async Task Handle_ReturnsEmptyCollection_WhenNoTitlesExist()
    {
        var repository = _serviceProvider.GetRequiredService<FakeTitleRepository>();
        repository.SetTitles(Array.Empty<TitleDetails>());
        var handler = _serviceProvider.GetRequiredService<ITitleDataQueryHandler>();

        var result = await handler.Handle(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Value);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenRepositoryThrows()
    {
        var repository = _serviceProvider.GetRequiredService<FakeTitleRepository>();
        repository.SetException(new InvalidOperationException("Sensitive database detail"));
        var handler = _serviceProvider.GetRequiredService<ITitleDataQueryHandler>();

        var result = await handler.Handle(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Unable to retrieve titles", result.Error);
    }

    private sealed class FakeTitleRepository : ITitleRepository
    {
        private IReadOnlyCollection<TitleDetails> _titles = Array.Empty<TitleDetails>();
        private Exception? _exception;

        public void SetTitles(IReadOnlyCollection<TitleDetails> titles)
        {
            _titles = titles;
            _exception = null;
        }

        public void SetException(Exception exception) => _exception = exception;

        public Task<IReadOnlyCollection<TitleDetails>> GetAllTitlesAsync(CancellationToken cancellationToken)
        {
            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_titles);
        }
    }
}
