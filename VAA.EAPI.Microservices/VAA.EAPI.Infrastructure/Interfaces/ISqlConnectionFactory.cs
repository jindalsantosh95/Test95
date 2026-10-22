using Microsoft.Data.SqlClient;

namespace VAA.EAPI.Infrastructure.Interfaces
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection(string connectionString);
    }
}
