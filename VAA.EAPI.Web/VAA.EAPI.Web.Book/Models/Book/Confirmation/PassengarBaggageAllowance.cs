using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.Book.Confirmation
{
    public class PassengerBaggageAllowance
    {
        public FreeBaggageAllowance FreeBaggageAllowance { get; set; }
        public IEnumerable<PaidBaggage>? PaidBaggage { get; set; }

    }
}
