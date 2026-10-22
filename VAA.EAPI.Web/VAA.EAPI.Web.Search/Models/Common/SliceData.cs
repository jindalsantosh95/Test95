using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace VAA.EAPI.Web.Search.Models.Common
{
    public class SliceData
    {
        public int CurrentSlice { get; set; }
        public int TotalSlices { get; set; }
        private static Regex sliceIdRegex = new(@"\d+.+\d", RegexOptions.Compiled);
        public SliceData(string sliceKey)
        {
            var sliceInfo = sliceIdRegex.Match(sliceKey).Value.Split(".");
            TotalSlices = Convert.ToInt16(sliceInfo.First());
            CurrentSlice = Convert.ToInt16(sliceInfo.Last());
        }
    }
}