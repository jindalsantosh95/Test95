using System;
using System.ServiceModel;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Services
{
    public class Air4RemarksV3 : IAir4RemarksV3
    {
        readonly Remark1Client _Remark1Client;
        public Air4RemarksV3(Remark1Client remark1Client)
        {
            _Remark1Client = remark1Client ?? throw new ArgumentNullException(nameof(remark1Client));
        }
        public async Task<AddSpecialRemarkResponseType> AddSpecialRemarkRequest(AddSpecialRemarkRequestType addSpecialRemarkRequest)
        {
            try
            {
                if (_Remark1Client.ClientCredentials.ClientCertificate.Certificate == null)
                {
                    _Remark1Client.ClientCredentials.ClientCertificate.Certificate = CertificateHelper.GetCachedCertificate();
                }
                return (await _Remark1Client.AddSpecialRemarkAsync(addSpecialRemarkRequest)).AddSpecialRemarkResponse1;
            }
            catch (FaultException exception)
            {
                throw Air4FaultExceptionChecker.RemarksV3Exception(exception);
            }
        }
    }
}
