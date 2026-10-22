using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.Graphql
{
    public class ServiceErrors
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public IEnumerable<Info> MoreInfo { get; set; }
    }

}
