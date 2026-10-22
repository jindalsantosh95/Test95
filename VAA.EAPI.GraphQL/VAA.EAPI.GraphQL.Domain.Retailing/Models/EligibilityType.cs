using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Models
{
    [Flags]
    public enum EligibilityType
    {
        None = 0,
        DutyFree = 1,
        POPS = 2,
        Both = 4,
    }
}
