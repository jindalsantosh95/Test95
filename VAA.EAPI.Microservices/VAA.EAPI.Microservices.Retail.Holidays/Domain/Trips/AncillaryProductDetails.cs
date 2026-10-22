using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class AncillaryProductDetails
    {
        public string ElementGroup { get; set; }
        public string ElementGroupName { get; set; }

        public IEnumerable<Location> PickupPoints { get; set; }
        public IEnumerable<Location> DropoffPoints { get; set; }
        public Location Destination { get; set; }
        public Location SelectedPickupPoint { get; set; }
        public Location SelectedDropoffPoint { get; set; }
        public OneWayTripDetails OneWayTrip { get; set; }
        public bool? IsElementGroupDetailsOverridden { get; set; }
        public bool Return { get; set; }

        private const string TransferGroupCode = "TRANSFER";
        private const string TransferGroupName = "Transfer";

        public static explicit operator AncillaryProductDetails((JToken details, string productCode) source)
        {
            var (details, productCode) = source;

            if (source.details == null)
                return null;
            bool isElementGroupDetailsOverridden = false;
            var isTransfer = productCode == "TRS";
            var elementGroup = source.details["elementGrp"]?.Value<string>();
            var elementGroupName = source.details["elementGrpName"]?.Value<string>();
            if (elementGroup.IsNullOrEmpty() && elementGroupName.IsNullOrEmpty())
            {
                if (isTransfer == true)
                {
                    elementGroup = TransferGroupCode;
                    elementGroupName = TransferGroupName;
                    isElementGroupDetailsOverridden = true;
                }
            }

            return new()
            {
                ElementGroup = elementGroup,
                ElementGroupName = elementGroupName,
                PickupPoints = isTransfer ? source.details["pickupPoints"]?.Select(p => (Location)p) : default,
                DropoffPoints = isTransfer ? source.details["dropoffPoints"]?.Select(p => (Location)p) : default,
                Destination = (Location)source.details["destination"],
                SelectedDropoffPoint = isTransfer ? (Location)source.details["selectedDropoffPoint"] : default,
                SelectedPickupPoint = isTransfer ? (Location)source.details["selectedPickupPoint"] : default,
                Return = isTransfer && (source.details["return"]?.Value<bool>() ?? false),
                IsElementGroupDetailsOverridden = isElementGroupDetailsOverridden
            };
        }
    }
}
