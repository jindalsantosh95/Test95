namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    internal class DateTimeHelper
    {
        /// <summary>
        /// Take the colon Duration for e.g. 08:00 and convert into Iso format 
        /// </summary>
        /// <param name="colonDuration"></param>
        /// <returns>IsoFormat Duration</returns>
        public static string ConvertColonDurationToIso(string colonDuration)
        {
            var duration = colonDuration.Split(':');

            var hours = duration[0] == "00" ? string.Empty : $"{duration[0]}H";
            var minutes = duration[1] == "00" ? string.Empty : $"{duration[1]}M";

            return $"PT{hours}{minutes}";
        }
    }
}
