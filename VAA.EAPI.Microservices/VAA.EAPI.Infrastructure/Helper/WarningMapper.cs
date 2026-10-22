using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Infrastructure.Helper
{
    public static class WarningMapper
    {
        public static IEnumerable<Warning> MapWarnings(string warningCode)
        {
            var configWarnings = Json.Deserialize<IEnumerable<Warning>>(ConfigReader.ReadAllText(Constants.Constants.FunctionAppDirPath, Constants.Constants.WarningCodesPath));
            Warning warning = configWarnings.FirstOrDefault(w => w.Code == warningCode);

            if (warning != null)
            {
                yield return warning;
            }
        }

    }

}
