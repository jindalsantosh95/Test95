using System.Text.RegularExpressions;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    internal class TripAdvisor
    {
        private static Regex regex = new Regex(".*/(?'rating'[0-5]\\.[05])-.*svg", RegexOptions.Compiled);

        /// <summary>
        /// Take the Trip Advisor Rating image and convert to number
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns>Rating</returns>
        public static decimal ConvertToRating(string imageFile)
        {
            Match m = regex.Match(imageFile);

            return m.Success ? decimal.Parse(m.Groups["rating"].Value) : 0.00m;
        }


    }
}
