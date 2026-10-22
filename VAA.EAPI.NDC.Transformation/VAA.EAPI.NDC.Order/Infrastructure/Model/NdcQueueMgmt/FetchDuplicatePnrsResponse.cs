using System;
using System.Collections.Generic;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.NdcQueueMgmt
{
    public class FetchDuplicatePnrsResponse
    {
        public IEnumerable<string> DuplicatePnrs { get; set; } = Array.Empty<string>();
    }
}
