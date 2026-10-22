using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace VAA.EAPI.GraphQL.Rest.Middleware.Mcp
{
    public sealed class Tool
    {
        [JsonProperty("name")]
        public string Name { get; init; } = default!;

        [JsonProperty("description")]
        public string Description { get; init; } = default!;

        [JsonProperty("parameters")]
        public Parameter[] Parameters { get; init; } = default!;

        [JsonProperty("inputSchema")]
        public object InputSchema { get; init; } = default!;

        [JsonProperty("_meta")]
        public Dictionary<string, object> Meta { get; set; }

        [JsonProperty("annotations")]
        public ToolAnnotations Annotations { get; set; }

        [JsonProperty("execution")]
        public ToolExecution Execution { get; set; }

        [JsonProperty("icons")]
        public IEnumerable<Icon> Icons { get; set; }

        [JsonProperty("outputSchema")]
        public Dictionary<string, object> OutputSchema { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonIgnore]
        public string[] AuthorizedRoles { get; init; } = [];

        [JsonIgnore]
        public bool RequiresAuthentication { get; init; }

        [JsonIgnore]
        public bool RequiresMcpCallerContext { get; init; }

        public bool CanAccess(ClaimsPrincipal principal) =>
            !RequiresAuthentication || principal?.Identity?.IsAuthenticated == true &&
            (AuthorizedRoles.Length == 0 || AuthorizedRoles.Any(principal.IsInRole));

        public bool HasRequiredCallerContext(ClaimsPrincipal bearer, ClaimsPrincipal callerContext) =>
            !RequiresMcpCallerContext ||
            bearer?.Identity?.IsAuthenticated == true && callerContext?.Identity?.IsAuthenticated == true &&
            CanAccess(bearer) && CanAccess(callerContext);
    }
}
