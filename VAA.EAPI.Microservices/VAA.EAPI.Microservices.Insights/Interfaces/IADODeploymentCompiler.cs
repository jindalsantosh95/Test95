using System.Threading.Tasks;
using VAA.EAPI.Microservices.Insights.Models;

namespace VAA.EAPI.Microservices.Insights.Interfaces
{
    public interface IADODeploymentCompiler
    {
        Task CompileDeploymentDataAsync();
        Task<ADODeploymentCollection> GetCompiledData();

        Task Reset();

    }
}
