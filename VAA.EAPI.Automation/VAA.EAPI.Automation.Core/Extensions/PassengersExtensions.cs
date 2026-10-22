using VAA.EAPI.Automation.Core.TestDataConfig;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class PassengersExtensions
    {
        /// <summary>
        /// Adds the specified number of adults, youngAdults, children and infants.
        /// </summary>
        /// <param name="adults">The number of adults to add.</param>
        /// <param name="youngAdults">The number of young adults to add.</param>
        /// <param name="children">The number of children to add.</param>
        /// <param name="infants">The number of infants to add.</param>
        public static void AddPassengers(this Passengers passengers, int adults = 1, int youngAdults = 0, int children = 0, int infants = 0)
        {
            AddAdults(passengers, adults);
            AddYoungAdults(passengers, youngAdults);
            AddChildren(passengers, children);
            AddInfants(passengers, infants);
        }

        /// <summary>
        /// Adds the specified number of adults.
        /// </summary>
        /// <param name="adults">The number of adults to add.</param>
        public static void AddAdults(this Passengers passengers, int adults)
        {
            for (int i = 0; i < adults; i++)
            {
                passengers.Adults.Add(new());
            }
        }

        /// <summary>
        /// Adds the specified number of young adults.
        /// </summary>
        /// <param name="youngAdults">The number of young adults to add.</param>
        public static void AddYoungAdults(this Passengers passengers, int youngAdults)
        {
            for (int i = 0; i < youngAdults; i++)
            {
                passengers.YoungAdults.Add(new());
            }
        }

        /// <summary>
        /// Adds the specified number of children.
        /// </summary>
        /// <param name="children">The number of children to add.</param>
        public static void AddChildren(this Passengers passengers, int children)
        {
            for (int i = 0; i < children; i++)
            {
                passengers.Children.Add(new());
            }
        }

        /// <summary>
        /// Adds the specified number of infants.
        /// </summary>
        /// <param name="infants">The number of infants to add.</param>
        public static void AddInfants(this Passengers passengers, int infants)
        {
            for (int i = 0; i < infants; i++)
            {
                passengers.Infants.Add(new());
            }
        }
    }
}