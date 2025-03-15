using apiBase.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace apiBase.Services
{
    public class SQLService : ISQLService
    {
        private readonly ILogger<SQLService> _logger;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public SQLService(ILogger<SQLService> logger, IConfiguration configuration)
        {
            _logger = logger;
            var builderBasisConnectionString =
                configuration.GetConnectionString("SQL_TARGET") ?? string.Empty;
            _configuration = configuration;
            if (string.IsNullOrEmpty(builderBasisConnectionString))
            {
                throw new ArgumentNullException("SQL_TARGET connection strings are missing");
            }
            _connectionString = builderBasisConnectionString;
        }

        /// <summary>
        /// Crea una conexion as ncrona a una base de datos SQL Server
        /// </summary>
        /// <param name="connectionString">Cadena de conexion a utilizar si se proporciona, de lo contrario se utiliza el valor de la configuracion "SQL_TARGET"</param>
        /// <returns>Una instancia de <see cref="SqlConnection"/> con la conexion abierta</returns>
        /// <exception cref="ArgumentNullException">Si la cadena de conexion solicitada no existe en la configuracion</exception>
        /// <exception cref="Exception">Si ocurre un error al intentar abrir la conexion</exception>
        public async Task<SqlConnection> CreateSQLConnection(string connectionString = "")
        {
            _logger.LogInformation($"In CreateSQLConnection arg:{connectionString}");

            try
            {
                SqlConnection connection;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    // Use the constructor's connection string when no argument is provided
                    connection = new SqlConnection(_connectionString);
                }
                else
                {
                    // Use the provided connection string name to get the actual connection string
                    var connString = _configuration.GetConnectionString(connectionString);
                    if (string.IsNullOrEmpty(connString))
                    {
                        throw new ArgumentNullException(
                            $"{connectionString} connection string is missing"
                        );
                    }
                    connection = new SqlConnection(connString);
                }

                await connection.OpenAsync();
                _logger.LogInformation($"SQL Connection created to {connection.ConnectionString}");
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SQL connection");
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL query and maps the results to a list of objects
        /// of type <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of object to map the results to.</typeparam>
        /// <param name="query">The SQL query to execute.</param>
        /// <param name="param">An anonymous object containing parameters to be
        /// passed to the query.</param>
        /// <returns>A list of objects of type <typeparamref name="TObject"/> containing
        /// the results of the query.</returns>
        public async Task<IEnumerable<TObject>> ExecuteQueryAsync<TObject>(
            string query,
            object? param = null
        )
        {
            using var connection = await CreateSQLConnection();
            return await connection.QueryAsync<TObject>(query, param);
        }
    }
}
