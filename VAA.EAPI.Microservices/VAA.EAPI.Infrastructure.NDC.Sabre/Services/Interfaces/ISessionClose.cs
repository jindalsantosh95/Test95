using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ISessionClose
    {
        Task<SessionCloseRQResponse> SessionCloseAsync(SessionCloseRQRequest sessionCloseRQ);
    }
}
