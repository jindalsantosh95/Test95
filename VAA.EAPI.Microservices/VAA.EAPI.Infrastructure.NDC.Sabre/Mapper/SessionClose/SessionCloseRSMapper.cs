using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.SessionClose
{
    public class SessionCloseRSMapper
    {
        public static bool CloseSessionRSMapper(SessionCloseRQResponse SessionCloseResponse)
        {
            return SessionCloseResponse.SessionCloseRS?.status == "Approved" ? true : false;
        }
    }
}
