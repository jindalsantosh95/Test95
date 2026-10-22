using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Automation.Core.Http.Mocking;

namespace VAA.EAPI.Automation.Utility.HttpMocker.Application
{
    public interface IMockStorageService
    {
        Task<HttpMock?> FindMockAsync(FunctionContext context);

        Task<bool> SynchroniseMock(string name);

        Task Synchronise();
    }
}
