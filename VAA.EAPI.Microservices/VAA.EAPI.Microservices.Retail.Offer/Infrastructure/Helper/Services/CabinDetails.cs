using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Services
{
    public class CabinDetails : ICabinDetails
    {
        List<Cabins> ICabinDetails.GetCabinDetails()
        {
            return Json.Deserialize<List<Cabins>>(ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, Constants.Application.CabinConfigPath));
        }
    }
}
