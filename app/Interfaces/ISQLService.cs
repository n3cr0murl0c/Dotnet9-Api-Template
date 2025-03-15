using Microsoft.Data.SqlClient;

namespace apiBase.Interfaces
{
    public interface ISQLService
    {
        Task<SqlConnection> CreateSQLConnection(string connectionString = "");
    }
}
