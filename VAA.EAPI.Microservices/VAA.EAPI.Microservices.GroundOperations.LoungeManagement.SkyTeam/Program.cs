using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Application.Command;
using VAA.EAPI.Common.Extensions;
using System.Diagnostics;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using VAA.EAPI.Infrastructure.Configuration;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Service;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure;
using VAA.EAPI.Microservices.GroundOperations.LoungeManagement.SkyTeam.Infrastructure.Services;

var host = new HostBuilder()
    .ConfigureAppConfiguration(configurationBuilder =>
    {
        configurationBuilder.AddCommandLine(args);
        configurationBuilder.AddEnvironmentConfiguration();
    })
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<FunctionContextAccessorMiddleware>();
        worker.UseMiddleware<ExceptionHandlerMiddleware>();
        worker.UseWhen<AuthenticationMiddleware>(x => !Debugger.IsAttached);
        worker.UseWhen<AuthorizationMiddleware>(x => !Debugger.IsAttached);
    })
    .ConfigureOpenApi()
    .ConfigureServices(services =>
    {
        services.AddDefaultServices();
        services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
        services.AddSingleton(typeof(IHttpConnector), typeof(HttpConnector));
        services.AddSingleton(typeof(ITokenGenerator), typeof(OAuthTokenGenerator));
        services.AddSingleton(typeof(ISkyTeamEligibilityService), typeof(SkyTeamEligibilityService));
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.RegisterServicesFromAssemblyContaining(typeof(LoungeEligibilityCommandHandler));
            config.RegisterServicesFromAssemblyContaining(typeof(LoungeEligibilityOverrideCommandHandler));
        });
    })
    .Build();

host.Run();
