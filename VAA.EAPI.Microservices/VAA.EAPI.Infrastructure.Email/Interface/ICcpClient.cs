using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Email.Entities;

namespace VAA.EAPI.Infrastructure.Email.Interface
{
    public interface ICcpClient
    {
        Task<KeyValuePair<bool, string>> SendEmail(CcpRequest customerCommRequest);
    }
}
