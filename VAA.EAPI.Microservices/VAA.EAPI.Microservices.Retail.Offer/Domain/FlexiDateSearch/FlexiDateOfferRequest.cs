using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;

public class FlexiDateOfferRequest
{
    public Pos Pos { get; set; }
    public Parties Parties { get; set; }
    public FlexiDateSearchRequest FlightSearchRequest { get; set; }
    public List<Customer> CustomerDetails { get; set; }
    public LoyaltyMemberDetails LoyaltyMember { get; set; }
    public string CurrencyCode { get; set; }
}