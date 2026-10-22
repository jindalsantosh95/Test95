using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Models.B2CGraph;
using CommonConstants = VAA.EAPI.Microservices.Loyalty.Identity.Constants;


public static class B2CExtensionHelper
{
    public static string GetExtensionKey(string attributeName)
    {
        return $"{ConfigReader.Get(CommonConstants.Environment.ExtensionPrefix)}{attributeName}";
    }

    public static void SetExtensionValue(Value user, string attributeName, string value)
    {
        if (user == null)
            return;

        if (user.AdditionalData == null)
            user.AdditionalData = new Dictionary<string, JToken>();

        string key = GetExtensionKey(attributeName);
        user.AdditionalData[key] = value;
    }
}
