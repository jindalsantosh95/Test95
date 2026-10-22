using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public class GetAdUserByFcNumberRepository : IGetAdUserByFcNumberRepository
    {
        private readonly IGetAdUserDetailsService _getUserDetailsService;
        private readonly ILogger<GetAdUserByFcNumberRepository> _logger;


        public GetAdUserByFcNumberRepository(IGetAdUserDetailsService getUserDetailsService, ILogger<GetAdUserByFcNumberRepository> logger)
        {
            _getUserDetailsService = getUserDetailsService ?? throw new ArgumentNullException( nameof(getUserDetailsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetCustomerIdByFcNumber(int fcNumber, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsValidFcNumber(fcNumber))
                    return null;

                var graphResponse = await _getUserDetailsService.GetDetailsFromGraphApi("/users", GetB2CQuery(fcNumber), cancellationToken);

                if (graphResponse.IsNullOrWhiteSpace())
                    return null;

                MemberInfo memberInfo = new();
                JsonDocument doc = JsonDocument.Parse(graphResponse);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("value", out JsonElement users) && users.GetArrayLength() > 0)
                {
                    JsonElement firstItem = users[0];
                    B2CGraphResponse customerResponse = Json.Deserialize<B2CGraphResponse>(graphResponse);
                    return customerResponse?.Value?.First().Id;
                }
                else
                {
                    var response = Json.Deserialize<Value>(graphResponse);
                    return response?.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetAdUserByFcNumberRepository)}-Exception fecthing Ad user : {ex.Message}");
                return null;
            }
        }
        private static string GetB2CQuery(int fcNumber)
        {
            return $"$select=id,userPrincipalName,accountEnabled,mail,Identities" +
                            $"&$filter={ConfigReader.Get(Environment.Identities)}/any(x: x / issuer eq {"'"}{ConfigReader.Get(Environment.B2CDomain)}{"'"} and issuerAssignedId eq {"'"}{fcNumber}{"'"})";
        }

        private static bool IsValidFcNumber(int fcNumber)
        { 
            return Regex.IsMatch(fcNumber.ToString(), @"^\d{9,11}$");
        }
    }
}
