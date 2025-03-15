using System.Data.Odbc;

namespace apiBase.Interfaces
{
    public interface IOdbcService
    {
        Task<OdbcConnection> CreateOdbcConnection();

        // Task ReturnOdbcConnection(OdbcConnection connection);

        Task<int> CreateOdbcTempDb(string tableStructure, string tempTable);
    }
}
