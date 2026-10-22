using System.Text;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Orchestration.RetailTherapy.Infrastructure.Models.FlightMs
{
    public sealed class CrewMember
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Base { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendJoin(';', Id.EscapeCsvValue()
                                        , FirstName.EscapeCsvValue()
                                        , LastName.EscapeCsvValue()
                                        , Email.EscapeCsvValue()
                                        , Base.EscapeCsvValue());

            return stringBuilder.ToString();
        }
    }
}
