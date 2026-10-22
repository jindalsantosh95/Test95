using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class SeatColumn
    {
        public bool Available { get; set; }
        public bool Aisle { get; set; }
        public bool Reserved { get; set; }
        public bool Bassinet { get; set; }
        public bool Blocked { get; set; }
        public bool Broken { get; set; }
        public bool Bulkhead { get; set; }
        public string CabinColumnID { get; set; }
        public IEnumerable<string> CatalogueRefIds { get; set; }
        public IEnumerable<string> Attributes { get; set; }
        public string Pos { get; set; }
        public bool Exit { get; set; }
        public bool Empty { get; set; }
        public bool Galley { get; set; }
        public bool Closet { get; set; }
        public bool ForDisabled { get; set; }
        public bool Missing { get; set; }
        public bool Toilet { get; set; }
        public bool NoSeat { get; set; }
        public bool NoStorage { get; set; }
        public bool NoWindow { get; set; }
        public bool Seat { get; set; }
        public bool Assigned { get; set; }
        public string BrandId { get; set; }
        public string SeatProductId { get; set; }
        public int Index { get; set; }
        public string IndexAlpha { get; set; }
        public string OccupationCode { get; set; }
        public string SeatIdentifer { get; set; }
        public IEnumerable<string> CharacteristicCodes { get; set; }
        public IEnumerable<SeatEligibility> Eligibility { get; set; }
        public bool EconomyComfort { get; set; }
        public bool ExitOverride { get; set; }
    }
}
