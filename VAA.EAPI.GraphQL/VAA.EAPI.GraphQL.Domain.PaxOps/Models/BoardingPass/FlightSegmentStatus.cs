namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.BoardingPass
{
    using VAA.EAPI.GraphQL.Common;
    using VAA.EAPI.GraphQL.Common.Extensions;

    public class FlightSegmentStatus
    {
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public bool? RebookEligible { get; set; }
        public bool IsCleaned { get; set; }
        public IropInformation? Information { get; set; }

        public static explicit operator FlightSegmentStatus(Infrastructure.Models.Trips.OperationalStatus response)
        {
            if (response == null)
                return null;

            var isIrop = response.Type.Is("IROP");
            var isCleaned = isIrop && response.SubType?.Contains("Cleaned", StringComparison.OrdinalIgnoreCase) == true;
            var information = isIrop ? GetIropInformation(response.SubType) : null;

            return new FlightSegmentStatus
            {
                Type = response.Type,
                SubType = response.SubType,
                RebookEligible = response.RebookEligible,
                IsCleaned = isCleaned,
                Information = information
            };
        }

        private static IropInformation? GetIropInformation(string? subType)
        {
            var information = IropContentCatalogue.GetInformation(subType);
            var card = information?.Card;

            if (card is null)
                return information;

            return new IropInformation
            {
                Card = new IropCard
                {
                    Title = card.Title,
                    Lines = AsyncContext.Current?.HasChannel("mobile") == true
                        ? card.MobileLines ?? card.Lines
                        : card.Lines
                },
                Page = information.Page
            };
        }
    }
}
