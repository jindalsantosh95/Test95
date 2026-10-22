using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Interfaces;

namespace VAA.EAPI.Microservices.Insights.Triggers.Timer
{
    public sealed class ADODeployments
    {
        private readonly IADODeploymentCompiler compiler;

        public ADODeployments(IADODeploymentCompiler compiler)
        {
            this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        }

        [Function("CompileADODeploymentStatus")]
        public async Task Run([TimerTrigger("0 0 4 * * *", RunOnStartup = false)] TimerInfo timer)
        {
            await compiler.CompileDeploymentDataAsync();
        }
    }
}
