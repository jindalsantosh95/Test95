using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.BaseClasses
{
    public class AuditBaseCommand : AuditBase
    {
        internal string Subject { get; set; }
        internal string Originator { get; set; }

        public AuditBaseCommand()
        {
            // Required for Serialization
        }

        public static async Task<T> MapToCommand<T>(HttpRequestData req, string customerID) where T : AuditBaseCommand
        {
            var request = await new StreamReader(req.Body).ReadToEndAsync();
            T command = string.IsNullOrEmpty(request) ? (T)Activator.CreateInstance(typeof(T)) : Json.Deserialize<T>(request);


            if (req.FunctionContext.Items?.Any(item => (string)item.Key == "ClaimsPrincipal") ?? false)
            {
                command.Originator = ((ClaimsPrincipal)req.FunctionContext.Items["ClaimsPrincipal"]).Identity.Name;
            }
            else
            {
                command.Originator = string.Empty;
            }
            command.CustomerId = customerID;

            return command;
        }
    }
}
