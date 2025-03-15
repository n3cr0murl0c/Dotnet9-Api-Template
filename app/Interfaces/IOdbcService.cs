using System.Data.Odbc;

namespace apiBase.Interfaces
{
    public interface IOdbcService
    {
        Task<OdbcConnection> CreateOdbcConnection();

        // Task ReturnOdbcConnection(OdbcConnection connection);

        public string GetBasisLibrary(string negocio);

        // public string _connectionString;
        public string GetBasisTempLibFile();
        Task<int> CreateBasisTempDb(string tableStructure, string tempTable);
    }
}
