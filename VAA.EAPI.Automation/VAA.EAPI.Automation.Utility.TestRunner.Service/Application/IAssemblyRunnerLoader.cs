using System;
using Xunit.Runners;

namespace VAA.EAPI.Automation.Utility.TestRunner.Service.Application
{
    public interface IAssemblyRunnerLoader : IDisposable
    {
        AssemblyRunner[] Runners { get; }

        void DisconnectAll();
    }
}
