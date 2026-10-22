using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using System.Diagnostics;
using VAA.EAPI.HealthStatusApp.Components;
using VAA.EAPI.HealthStatusApp.Factories;
using VAA.EAPI.HealthStatusApp.Helpers;
using VAA.EAPI.HealthStatusApp.Services;
using VAA.EAPI.HealthStatusApp.SignalRClasses;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;

namespace VAA.EAPI.HealthStatusApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStarted = activity => Console.WriteLine($"Activity started: {activity.OperationName}"),
                ActivityStopped = activity => Console.WriteLine($"Activity stopped: {activity.OperationName}, Duration: {activity.Duration}"),
            };

            ActivitySource.AddActivityListener(listener);

            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            });

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IHttpConnector, HttpConnector>();
            builder.Services.AddSingleton<IInsightsService, InsightsService>();
            builder.Services.AddSingleton<IAzureStatusFeedService, AzureStatusFeedService>();
            builder.Services.AddScoped<ISignalRService, SignalRService>();
            builder.Services.AddSingleton<IMetricsService, MetricsService>();
            builder.Services.AddSingleton<IDashboardFactory, DashboardFactory>();
            builder.Services.AddSingleton<IStatusTableFactory, StatusTableFactory>();
            builder.Services.AddSingleton<IMetricsTableFactory, MetricsTableFactory>();
            builder.Services.AddSingleton<IInsightsConnector, InsightsConnector>();
            builder.Services.AddSingleton<IServiceNowService, ServiceNowService>();
            builder.Services.AddSingleton<IDeploymentStatusService, DeploymentStatusService>();
            builder.Services.AddSingleton<IAzureAppSettingsService, AzureAppSettingsService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IGraphService, GraphService>();

            builder.Services.AddHostedService(sp =>
                new ApiHealthCheckService(
                    sp.GetRequiredService<IHubContext<HealthStatusHub, IHealthStatusClient>>(),
                    sp.GetRequiredService<IDashboardFactory>(),
                    sp.GetRequiredService<ILogger<ApiHealthCheckService>>(),
                    TimeSpan.FromSeconds(300),
                    TimeSpan.FromMinutes(5)));

            var azureAdConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"AzureAd:Instance", "https://login.microsoftonline.com/"},
                    {"AzureAd:Domain", builder.Configuration["AzureAD_Domain"]},
                    {"AzureAd:TenantId", builder.Configuration["AzureAD_TenantId"]},
                    {"AzureAd:ClientId", builder.Configuration["AzureAD_ClientId"]},
                    {"AzureAd:CallbackPath", "/signin-oidc"},
                    {"AzureAD:OAuth2AllowIdTokenImplicitFlow", "true" }
                })
                .Build();

            builder.Services.AddMicrosoftIdentityWebAppAuthentication(azureAdConfig);
            builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Events.OnRemoteFailure = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Invalid authentication response.");
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireSpecificGroup", policy =>
                    policy.Requirements.Add(new GroupRequirement(builder.Configuration["AADGroupId"])));
            });

            builder.Services.AddSingleton<IAuthorizationHandler, GroupAuthorizationHandler>();

            builder.Services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApplication =>
                {
                    errorApplication.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("An unexpected error occurred.");
                    });
                });
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
if (context.Request.Path == "/signin-oidc")
                {
                    var hasState = context.Request.Query.ContainsKey("state");

                    if (!hasState && HttpMethods.IsPost(context.Request.Method) && context.Request.HasFormContentType)
                    {
                        try
                        {
                            var form = await context.Request.ReadFormAsync(context.RequestAborted);
                            hasState = form.ContainsKey("state");
                        }
                        catch
                        {
                            hasState = false;
                        }
                    }

                    if (!hasState)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Invalid authentication response.");
                        return;
                    }
                }

                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                // Exclude the AccessDenied page from the custom logic to prevent loops
                if (context.Request.Path.StartsWithSegments("/Account/AccessDenied"))
                {
                    await next();
                    return;
                }

                if (context.User.Identity.IsAuthenticated)
                {
                    var graphService = context.RequestServices.GetRequiredService<IGraphService>();
                    var userId = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                    if (string.IsNullOrEmpty(userId))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("User ID not found in claims.");
                        return;
                    }

                    var requiredGroupId = builder.Configuration["AADGroupId"];
                    var isInGroup = await graphService.IsUserInGroupAsync(userId, requiredGroupId);

                    if (!isInGroup)
                    {
                        context.Response.Redirect("/Account/AccessDenied");
                        return;
                    }
                }

                await next();
            });


            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapHub<HealthStatusHub>("/healthStatusHub");

            await app.RunAsync();
        }
    }
}
