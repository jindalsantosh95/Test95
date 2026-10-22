using System.Collections.Generic;
using System.Text.RegularExpressions;
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Common.Helper;

public static class PassengerTypeList
{
    // Converts a string such as a2t1c1i1 into an array of delta passenger type codes ["ADT","ADT","GBE","CNN","INF"]
    public static string[] ParseString(string source)
    {
        if (source.IsNullOrEmpty()) return null;

        var pax = Regex.Matches(source.ToLower(), "([atci])([0-9]+)");
        var paxList = new List<string>();
        foreach (Match paxType in pax)
        {
            var numOfPaxType = int.Parse(paxType.Groups[2].Value);
            for (var paxNum = 0; paxNum < numOfPaxType; paxNum++)
            {
                switch (paxType.Groups[1].Value)
                {
                    case "a": paxList.Add(Constants.Application.DeltaPtcAdult.ToString()); break;
                    case "t": paxList.Add(Constants.Application.DeltaPtcTeen.ToString()); break;
                    case "c": paxList.Add(Constants.Application.DeltaPtcChild.ToString()); break;
                    case "i": paxList.Add(Constants.Application.DeltaPtcInfant.ToString()); break;
                }
            }
        }
        return paxList.ToArray();
    }
}
