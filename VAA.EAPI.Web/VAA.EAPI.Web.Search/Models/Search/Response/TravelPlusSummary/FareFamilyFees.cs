using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlusSummary;

public class FareFamilyFees
{
    public IEnumerable<FareFamilyRule>? Change { get; set; }
    public IEnumerable<FareFamilyRule>? Cancellation { get; set; }
}