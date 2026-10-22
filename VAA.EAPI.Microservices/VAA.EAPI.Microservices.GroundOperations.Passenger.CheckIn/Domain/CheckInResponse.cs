using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class CheckInResponse
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public Slice CurrentSlice { get; set; }
        public OnlineCheckInStatus OnlineCheckInStatus { get; set; }
        public bool? ApplyEmergencyContactToAllPax { get; set; }
        public bool? ApplyContactTracingToAllPax { get; set; }
        public bool ShowBaggageAllowance { get; set; }

        static string ReplaceTokens(string template, Dictionary<string, string> replacements) =>
           RegexCompiled.Replace(template, @"{(\w+)}",
               match => replacements.TryGetValue(match.Groups[1].Value, out string replacement) ? replacement : match.Value);

        public static explicit operator CheckInResponse((DeltaSsci.TravelParty? travelParty, object? requirementsResponse, bool blockOnTrmPassportError) source)
            => (CheckInResponse)(source.travelParty, source.requirementsResponse, null, null, source.blockOnTrmPassportError);

        public static explicit operator CheckInResponse((DeltaSsci.TravelParty? travelParty, object? requirementsResponse,
            DeltaSsci.AddPassportResponse? ssciAddPassport, PassengerId? selectedPassengerId, bool blockOnTrmPassportError) source)
        {
            var requirements = source.requirementsResponse switch
            {
                DeltaTrm.RequirementsResponse r => r.TravelRequirements,
                DeltaTrm.RequirementFormResponse f => f.TravelRequirements,
                _ => null
            };

            var response = new CheckInResponse()
            {
                TravelPartyId = source.travelParty.Id,
                RequirementsId = ((dynamic)source.requirementsResponse).RequirementsResourceId,
                Reference = (Reference)requirements,
                OnlineCheckInStatus = (OnlineCheckInStatus)source.travelParty
            };

            if (source.travelParty.HasGroupPNRFlag)
            {
                response.OnlineCheckInStatus = new OnlineCheckInStatus()
                {
                    Available = false,
                    Title = "Airport check in required",
                    Message = "Unfortunately you can't check in online for this booking. Please check in at the airport, either at the kiosk or with an agent.",
                    Code = "GroupPnrRestricted"
                };
                return response;
            }

            // SSCI return a trip containing the flights that are open for check in as one "trip".
            // TRM returns an array of trips, we need to pick the relevant one(s) ourselves.
            var ssciSegments = source.travelParty?.Trip?.Segments;
            List<DeltaTrm.Trip> trmTrips = new List<DeltaTrm.Trip>();
            if (source.travelParty?.Trip?.Segments != null)
            {
                foreach (var ssciLeg in source.travelParty!.Trip.Segments)
                {
                    var trmTrip = requirements?.Trips.Single(t => t.Segments.Any(s =>
                        s.Legs.Any(l => l.Departure.Station.Code == ssciLeg.OriginCode && l.Arrival.Station.Code == ssciLeg.DestinationCode)))!;
                    if (trmTrips.Any(t => t.Id == trmTrip.Id) == false)
                    {
                        trmTrips.Add(trmTrip);
                    }
                }
            }

            if (source.travelParty?.Passengers?.Count > 0 && response.OnlineCheckInStatus.Available)
            {
                var trmPassengers = requirements?.Passengers;

                var passengers = source.travelParty.Passengers
                        .Select(pax =>
                        {
                            var tmPassenger = trmPassengers?.FirstOrDefault(tp => tp.NameNum == $"{pax.LastNIN}.{pax.FirstNIN}");
                            return (Passenger)(pax, tmPassenger, trmTrips, null, source.ssciAddPassport, source.selectedPassengerId, source.travelParty.IsInBoundToUS);
                        })
                        .ToList();

                foreach (var pax in passengers.Where(p => p.IsInfantPassenger == true && !string.IsNullOrWhiteSpace(p.AssociatedAdultFullName)))
                {
                    pax.AssociatedAdultPassengerId = passengers
                        .FirstOrDefault(adult => string.Equals(
                            $"{adult.GivenNames} {adult.Surname}".Trim(),
                            pax.AssociatedAdultFullName.Trim(),
                            StringComparison.OrdinalIgnoreCase))?.PassengerId ?? string.Empty;
                }

                response.Passengers = GetOrderedPassengers(passengers);

                foreach (var passenger in response.Passengers)
                {
                    var passengerRequirements = passenger.Requirements;
                    if (passengerRequirements?.TravelRequirementErrors.Any() != true)
                        continue;

                    passengerRequirements.PassportErrors = passengerRequirements.PassportErrors
                        .Concat(passengerRequirements.TravelRequirementErrors)
                        .ToList();
                }

                response.ApplyContactTracingToAllPax = ShouldEnableApplyContactTracingToAll(response.Passengers);

                if (source.blockOnTrmPassportError)
                {
                    var selectedPassengerWithPassportErrors = response.Passengers?.FirstOrDefault(p => p.PassengerId.Split("|")[3] == source.selectedPassengerId?.TrmPassengerResourceId && p.Requirements.PassportErrors.Any());
                    if (selectedPassengerWithPassportErrors != null)
                    {
                        response.OnlineCheckInStatus = (OnlineCheckInStatus)selectedPassengerWithPassportErrors.Requirements.PassportErrors.First();
                        return response;
                    }
                }
            }
            else
            {
                response.Passengers = Enumerable.Empty<Passenger>();
            }

            // SSCI advisories don't tell us very much, so if both TRM and SSCI return advisories, we prefer TRM (error returned in previous block).
            if (source.ssciAddPassport?.Advisory != null)
            {
                var statusMessage = CheckInMessageHelper.GetStatusMessage(source.ssciAddPassport.Advisory.ErrorMessage);
                Dictionary<string, string>? replacements;
                switch (source.ssciAddPassport.Advisory.ErrorMessage)
                {
                    case "redirectJVPartner":
                        replacements = new Dictionary<string, string>
                        {
                            ["firstAirlineName"] = source.travelParty.Trip.OperatedByAirlineName
                        };
                        break;

                    default:
                        replacements = null;
                        break;
                }
                response.OnlineCheckInStatus = new OnlineCheckInStatus()
                {
                    Available = false,
                    Title = replacements == null ? statusMessage.Item1 : ReplaceTokens(statusMessage.Item1, replacements),
                    Message = replacements == null ? statusMessage.Item2 : ReplaceTokens(statusMessage.Item2, replacements),
                    Code = source.ssciAddPassport.Advisory.ErrorMessage
                };
                return response;
            }

            response.ApplyEmergencyContactToAllPax = EmergencyContactHelper.ShouldApplyEmergencyContactToAllPassengers(response.Passengers);

            if (source.travelParty?.Trip?.Segments?.Any() == true)
                response.CurrentSlice = (Slice)(source.travelParty, trmTrips);

            response.ShowBaggageAllowance = source.travelParty?.Trip?.Segments?.FirstOrDefault()?.OperatingAirlineCode == "VS";

            return response;
        }

        public static explicit operator CheckInResponse(HttpRequestException source)
        {
            return new CheckInResponse()
            {
                OnlineCheckInStatus = (OnlineCheckInStatus)source
            };
        }
        private static bool ShouldEnableApplyContactTracingToAll(IEnumerable<Passenger> passengers)
        {
            var pax = (passengers?.Where(p => p.Requirements is not null && p.Requirements.ContactTracing != ContactTracingStatus.NOT_REQUIRED))?.ToList() ?? [];
 
            if (pax.Count <= 1)
                return false;  

            if (pax.All(p => p?.Requirements?.ContactTracing == ContactTracingStatus.TO_COMPLETE))
                return true;

            var passengersWithContactTracing = pax.Where(p => p?.Requirements?.ContactTracing == ContactTracingStatus.COMPLETE).ToList();

            if (passengersWithContactTracing?.Count < 2)
                return true;

            return CompareContactTracingDetails(passengersWithContactTracing);
        }

        private static bool CompareContactTracingDetails(List<Passenger> passengersWithContactTracing)
        {
            var firstContactTracing = passengersWithContactTracing.First().ContactTracing;

            if (firstContactTracing is null)
                return false;

            static string Normalize(string? value) => value?.Trim() ?? string.Empty;

            var firstStreetAddress = Normalize(firstContactTracing.StreetAddress);
            var firstCity = Normalize(firstContactTracing.City);
            var firstState = Normalize(firstContactTracing.State);
            var firstZip = Normalize(firstContactTracing.Zip);
            var firstEmail = Normalize(firstContactTracing.Email);
            var firstPrimaryPhoneNumberCountryCode = Normalize(firstContactTracing.PrimaryPhoneNumberCountryCode);
            var firstPrimaryPhoneNumber = Normalize(firstContactTracing.PrimaryPhoneNumber);
            var firstSecondaryPhoneNumberCountryCode = Normalize(firstContactTracing.SecondaryPhoneNumberCountryCode);  
            var firstSecondaryPhoneNumber = Normalize(firstContactTracing.SecondaryPhoneNumber);

            return passengersWithContactTracing.All(p =>
            {
                var contactTracing = p.ContactTracing;

                if (contactTracing is null)
                    return false;

                return Normalize(contactTracing.StreetAddress).Is(firstStreetAddress)
                    && Normalize(contactTracing.City).Is(firstCity)
                    && Normalize(contactTracing.State).Is(firstState)
                    && Normalize(contactTracing.Zip).Is(firstZip)
                    && Normalize(contactTracing.Email).Is(firstEmail)
                    && Normalize(contactTracing.PrimaryPhoneNumberCountryCode).Is(firstPrimaryPhoneNumberCountryCode)
                    && Normalize(contactTracing.PrimaryPhoneNumber).Is(firstPrimaryPhoneNumber)
                    && Normalize(contactTracing.SecondaryPhoneNumberCountryCode).Is(firstSecondaryPhoneNumberCountryCode)
                    && Normalize(contactTracing.SecondaryPhoneNumber).Is(firstSecondaryPhoneNumber)
                    && contactTracing.NoEmail == firstContactTracing.NoEmail
                    && contactTracing.NoSecondaryPhoneNumber == firstContactTracing.NoSecondaryPhoneNumber;
            });
        }

        private static IEnumerable<Passenger> GetOrderedPassengers(IEnumerable<Passenger> passengerList)
        {
            //var passengerList = passengers.ToList();

            // If no infants exist, return same order
            if (!passengerList.Any(p => p?.IsInfantPassenger == true))
                return passengerList;

            var ordered = new List<Passenger>();
            var addedPassengerIds = new HashSet<string>();

            // Loop through infants and add parent + infant pairs
            foreach (var infant in passengerList.Where(p => p?.IsInfantPassenger == true))
            {

                var parent = passengerList.FirstOrDefault(p =>
                    string.Equals(p.PassengerId, infant.AssociatedAdultPassengerId, StringComparison.OrdinalIgnoreCase));

                // Add parent if not already added
                if (parent != null && addedPassengerIds.Add(parent.PassengerId))
                {
                    ordered.Add(parent);
                }

                if (addedPassengerIds.Add(infant.PassengerId))
                {
                    ordered.Add(infant);
                }
            }

            // Add remaining passengers in original order
            foreach (var pax in passengerList)
            {
                if (addedPassengerIds.Add(pax.PassengerId))
                {
                    ordered.Add(pax);
                }
            }

            return ordered;
        }
    }
}

