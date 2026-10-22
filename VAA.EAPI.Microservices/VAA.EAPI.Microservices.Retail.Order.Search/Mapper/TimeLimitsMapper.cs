using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public class TimeLimitsMapper
    {
        public static List<TimeLimit> Air4ResponseToTimeLimitsMapper(RetrieveResponseType response)
        {
            var timelimits = new List<TimeLimit>();
            var pnrCreationDate = DateTimeHelper.Air4PNRDateTimeConversion(response.TeardownContextResponse.Pnr.RecordLocator.First().PnrCreationDateWithYear, "0000A");

            if (response.TeardownContextResponse.Pnr.TicketingTimeLimit != null)
            {
                foreach (var ttl in response.TeardownContextResponse.Pnr.TicketingTimeLimit)
                {
                    timelimits.Add(new()
                    {
                        Limit = ttl.FreeformText.Contains('/') ? DateTimeHelper.Air4DepartDateFormatConversion(ttl.FreeformText.Split('/')[1], GetTTLYear(ttl.FreeformText, pnrCreationDate), ttl.FreeformText.Split('/')[0])?.ToString(Constants.Application.TimeLimitDateTimeFormat) : null,
                        Type = TimeLimit.Types.TicketingTimeLimit,
                        Remarks = ttl.FreeformText
                    });
                }
                return timelimits;
            }
            else
                return null;
        }

        private static string GetTTLYear(string freeFormText, DateTime? pnrCreationDate)
        {
            var year = pnrCreationDate.Value.Year;
            var ttlLimit = DateTimeHelper.Air4DepartDateFormatConversion(freeFormText.Split('/')[1], year.ToString(), freeFormText.Split('/')[0]);
            return ttlLimit > pnrCreationDate ? year.ToString() : (year + 1).ToString();

        }
    }
}
