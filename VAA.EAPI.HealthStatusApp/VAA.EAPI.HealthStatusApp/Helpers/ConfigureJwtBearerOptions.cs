using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace VAA.EAPI.HealthStatusApp.Helpers
{
    public class JwtBearerOptionsConfiguration : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string name, JwtBearerOptions options)
        {
            // Ensure Events is not null
            options.Events ??= new JwtBearerEvents();

            var originalOnMessageReceived = options.Events.OnMessageReceived;
            options.Events.OnMessageReceived = async context =>
            {
                if (originalOnMessageReceived != null)
                {
                    await originalOnMessageReceived(context);
                }

                if (string.IsNullOrEmpty(context.Token))
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/healthStatusHub"))
                    {
                        context.Token = accessToken;
                    }
                }
            };
        }
    }
}