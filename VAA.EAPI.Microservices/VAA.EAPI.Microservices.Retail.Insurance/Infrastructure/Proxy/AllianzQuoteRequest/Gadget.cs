using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{
    public class Gadget
    {
        public string BrowserType { get; set; }
        public string BrowserVersion { get; set; }
        public string DeviceCategory { get; set; }
        public string DeviceType { get; set; }
        public string IpAddress { get; set; }
        public string OperatingSystem { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string RequestingPlatform { get; set; }
        public string RequestingPlatformVersion { get; set; }
        public string UserAgent { get; set; }
    }
}
