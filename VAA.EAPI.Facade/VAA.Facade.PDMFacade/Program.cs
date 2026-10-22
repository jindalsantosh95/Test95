using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Middleware;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Common.Extensions;
using VAA.Facade.PDMFacade.Application.Command;
using System.Diagnostics;

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
    }
    )
    .ConfigureServices(services =>
    {
        services.AddDefaultServices();
        services.AddSingleton<IOAuthTokenProvider, OAuthTokenProvider>();
        services.AddSingleton<IHttpConnector, HttpConnector>();
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.RegisterServicesFromAssemblyContaining(typeof(RetroCreditCommand));
            config.RegisterServicesFromAssemblyContaining(typeof(RetroFeedbackCommand));
        });
    })
    .Build();

host.Run();
