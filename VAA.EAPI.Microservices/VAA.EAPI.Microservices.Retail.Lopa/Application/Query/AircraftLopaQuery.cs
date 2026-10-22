using MediatR;

namespace VAA.EAPI.Microservices.Retail.Lopa.Application.Query
{
    public class AircraftLopaQuery : IRequest<string>
    {
        public string FleetTypeCode { get; set; }
        public string EquipmentCode { get; set; }

    }

}
