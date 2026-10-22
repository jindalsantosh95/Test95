using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Common.Extensions;
using System.Reflection;
using System.Diagnostics;
using VAA.EAPI.LoungeManagement.SkyTeam.Application.Query;
using MediatR;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;

var host = new HostBuilder()
    .ConfigureAppConfiguration(configurationBuilder =>
    {
        configurationBuilder.AddCommandLine(args);
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
        services.AddSingleton<IHttpConnector, HttpConnector>();
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.RegisterServicesFromAssemblyContaining(typeof(RetriveTicketQuery));
        });
    })
    .Build();

host.Run();
