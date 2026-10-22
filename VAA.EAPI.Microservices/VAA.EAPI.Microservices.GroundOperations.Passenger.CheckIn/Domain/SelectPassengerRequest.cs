using Azure.Storage.Blobs.Models;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Helper;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class SelectPassengerRequest
    {
        public string TravelPartyId { get; set; }
        public IEnumerable<string> SelectedPassengerIds { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PnrLocator { get; set; }
        public string OriginCode { get; set; }
        public IEnumerable<(string passengerId, bool isInfant)>? SelectablePassengers { get; set; }

        public static explicit operator DeltaSsci.SelectPassengerRequest(SelectPassengerRequest source)
        {
            if(source.SelectablePassengers != null && source.SelectablePassengers.Any())
            {
                return new DeltaSsci.SelectPassengerRequest()
                {
                    SelectedPassenger = source.SelectablePassengers?.Select(psgInfo =>
                    {
                        var values =  new PassengerId(psgInfo.passengerId);

                        return new Passenger1
                        {
                            Id = values.SsciPassengerId,
                            CustomerId = values.CustomerId,
                            FirstNIN = values.FirstNin,
                            LastNIN = values.LastNin,
                            InfantPassenger = psgInfo.isInfant
                        };
                    }).ToList() ?? []
                };
            }

            return new DeltaSsci.SelectPassengerRequest()
            {
                SelectedPassenger = source.SelectedPassengerIds?.Select(id =>
                {
                    var values = new PassengerId(id);

                    return new Passenger1
                    {
                        Id = values.SsciPassengerId,
                        CustomerId = values.CustomerId,
                        FirstNIN = values.FirstNin,
                        LastNIN = values.LastNin,
                    };
                }).ToList() ?? []
            };
        }
    }
}