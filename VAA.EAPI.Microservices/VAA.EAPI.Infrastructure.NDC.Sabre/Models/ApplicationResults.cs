using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class ApplicationResults
    {
        public string status { get; set; }
        public IEnumerable<Success> Success { get; set; }
    }
}
