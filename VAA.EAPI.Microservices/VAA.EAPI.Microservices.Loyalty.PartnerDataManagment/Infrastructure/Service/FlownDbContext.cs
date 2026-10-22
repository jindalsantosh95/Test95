using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service
{
    public class FlownDbContext
    {
        private readonly ILogger<FlownDbContext> _logger;

        private readonly string _sqlconnectionString;

        public FlownDbContext(string sqlconnectionString)
        {
            _sqlconnectionString = sqlconnectionString;
        }

        public IDbConnection CreateConnection()
        {
            var conn = new SqlConnection(_sqlconnectionString);

            return conn;
        }
    }
}
