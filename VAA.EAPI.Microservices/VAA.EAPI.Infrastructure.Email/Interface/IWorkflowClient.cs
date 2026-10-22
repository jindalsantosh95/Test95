using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Email.Entities;

namespace VAA.EAPI.Infrastructure.Email.Interface
{
    public interface IWorkflowClient
    {
        Task<KeyValuePair<bool, string>> SendEmailCommunication(WorkFlowRequest request, CancellationToken cancellationToken);
    }
}
