using System.Data;

using Microsoft.Data.SqlClient;

using XperienceCommunity.Manager.Services.Interfaces;

namespace XperienceCommunity.Manager.Services
{
    public class ConnectionFactory : IConnectionFactory
    {
        public IDbConnection GetConnection(string connectionString) => new SqlConnection(connectionString);
    }
}
