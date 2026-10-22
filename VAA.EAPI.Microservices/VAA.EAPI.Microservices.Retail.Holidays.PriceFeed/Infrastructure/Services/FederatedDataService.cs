using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Services
{
    public class FederatedDataService(ILogger<FederatedDataService> logger, IHttpConnector httpConnector) : IFederatedDataService
    {
        private readonly ILogger<FederatedDataService> _logger = logger;
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        
        public async Task<FederatedDataAPIResponse> GetFederatedDataAsync(string destination)
        {
            try
            {
                var url = string.Format(CultureInfo.InvariantCulture, ConfigReader.Get<string>(Constants.Environment.FederatedAPIURL), Uri.EscapeDataString(destination));

                return await  _httpConnector.GetAsync<FederatedDataAPIResponse>(url, headers: null, bearerToken: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }

        }
    }
}
