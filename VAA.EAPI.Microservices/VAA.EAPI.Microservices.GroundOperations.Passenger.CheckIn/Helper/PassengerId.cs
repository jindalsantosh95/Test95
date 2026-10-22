using System.ComponentModel.DataAnnotations;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper
{
    public class PassengerId
    {
        public string FirstNin { get; set; } = string.Empty;
        public string LastNin { get; set; } = string.Empty;
        public string Nins => $"{LastNin}.{FirstNin}";
        public string SsciPassengerId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string TrmPassengerResourceId { get; set; } = string.Empty;

        private PassengerId()
        {
        }

        public PassengerId(string passengerId)
        {
            if (passengerId.IsNullOrWhiteSpace())
            {
                throw new ValidationException("PassengerId is required.");
            }

            var parts = passengerId.Split('|');
            if (parts.Length != 4 || parts.Take(3).Any(part => part.IsNullOrWhiteSpace()))
            {
                throw new ValidationException("PassengerId must be in the expected format '<firstNin>.<lastNin>|<ssciPassengerId>|<customerId>|<trmPassengerResourceId>'.");
            }

            var nins = parts[0].Split('.');
            if (nins.Length != 2 || nins.Any(nin => nin.IsNullOrWhiteSpace()))
            {
                throw new ValidationException("PassengerId must include passenger NINs in the expected format '<firstNin>.<lastNin>'.");
            }

            FirstNin = nins[0];
            LastNin = nins[1];
            SsciPassengerId = parts[1];
            CustomerId = parts[2];
            TrmPassengerResourceId = parts[3];
        }

        public static PassengerId FromTravelRequirements(string passengerId)
        {
            if (string.IsNullOrWhiteSpace(passengerId))
            {
                throw new ValidationException("PassengerId is required.");
            }

            var parts = passengerId.Split('|');
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                throw new ValidationException("PassengerId must be in the expected format '<lastNin>.<firstNin>|<trmPassengerResourceId>'.");
            }

            var nins = parts[0].Split('.');
            if (nins.Length != 2 || string.IsNullOrWhiteSpace(nins[0]) || string.IsNullOrWhiteSpace(nins[1]))
            {
                throw new ValidationException("PassengerId must include passenger NINs in the expected format '<lastNin>.<firstNin>'.");
            }

            return new PassengerId
            {
                LastNin = nins[0],
                FirstNin = nins[1],
                TrmPassengerResourceId = parts[1]
            };
        }

        public string Get()
        {
            return $"{Nins}|{SsciPassengerId}|{CustomerId}|{TrmPassengerResourceId}";
        }
    }
}
