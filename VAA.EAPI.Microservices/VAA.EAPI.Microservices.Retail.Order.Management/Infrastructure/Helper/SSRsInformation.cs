using System.Collections.Generic;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper
{
    internal class SSRsInformation
    {
        public static List<SSRsCodes> GetSSRsDetails()
        {
            string SSRsCodesString = ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, Constants.Application.SSRsPath);
            return Json.Deserialize<List<SSRsCodes>>(SSRsCodesString);
        }
    }
}
