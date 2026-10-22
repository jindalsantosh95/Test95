using System;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public class CalendarFE
    {
        public Dictionary<string, CalendarPrice> FromPrices { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public static explicit operator CalendarFE(CalendarGraph source)
        {
            if (source == null)
                return null;

            CalendarFE result = new();
            result.From = source.From;
            result.To = source.To;

            result.FromPrices = new();

            foreach (var item in source.FromPrices)
                result.FromPrices.Add(item.FromDate.ToString("yyyy-MM-dd"), item.Price);

            return result;

        }
    }
}
