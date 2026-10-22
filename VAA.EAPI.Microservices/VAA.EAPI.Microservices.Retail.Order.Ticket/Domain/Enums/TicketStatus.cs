using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums
{
    public enum TicketStatus
    {
        [EnumMember(Value = "Void")]
        V,
        [EnumMember(Value = "VoidUpdate")]
        EVUT
    }
}
