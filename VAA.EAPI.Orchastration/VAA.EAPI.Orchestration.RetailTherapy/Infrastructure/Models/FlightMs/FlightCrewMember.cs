using System.Text;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs
{
    public sealed class FlightCrewMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pos { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendJoin(';', Id.EscapeCsvValue()
                                        , Name.EscapeCsvValue()
                                        , Pos.EscapeCsvValue());

            return stringBuilder.ToString();
        }
    }
}
