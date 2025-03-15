using System.Data.Odbc;
using apiBase.Interfaces;

namespace apiBase.Services
{
    public class OdbcService : IOdbcService
    {
        public readonly string _connectionString;
        private readonly ILogger<OdbcService> _logger;
        private readonly IConfiguration _configuration;

        public OdbcService(IConfiguration configuration, ILogger<OdbcService> logger)
        {
            string builderddOdbcConnectionString;
            // var odbcddOdbcConnectionString = configuration.GetConnectionString("ODBC_ddOdbc_DEV");
            var settings = configuration.GetSection("Settings");
            int maxRetries = settings.GetValue<int>("maxRetries");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                Console.WriteLine(
                    $"Enviroment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
                );

                builderddOdbcConnectionString =
                    configuration.GetConnectionString("ddOdbc_DEV") ?? string.Empty;
            }
            else
            {
                builderddOdbcConnectionString =
                    configuration.GetConnectionString("ddOdbc_PROD") ?? string.Empty;
            }

            if (string.IsNullOrEmpty(builderddOdbcConnectionString))
            {
                throw new ArgumentNullException("ddOdbc connection strings are missing");
            }

            if (int.IsNegative(maxRetries))
            {
                throw new ArgumentException("Max Retries no puede ser negativo");
            }

            _logger = logger;
            _connectionString = builderddOdbcConnectionString;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a temporary table in the database.
        /// </summary>
        /// <param name="tableStructure">The structure of the table, including columns and types.</param>
        /// <param name="tempTable">The name of the temporary table to create.</param>
        /// <returns>1 if the table was created, -1 if an error occurred.</returns>
        /// <exception cref="OverflowException">If the table already exists.</exception>
        /// <exception cref="OdbcException">If the table cannot be created.</exception>
        /// <exception cref="Exception">If an error occurs while creating the table.</exception>
        public async Task<int> CreateOdbcTempDb(string tableStructure, string tempTable)
        {
            // using var connection = await CreateJdbcConnection();
            // using var connection = await CreateIDB2Connection();
            using var connection = await CreateOdbcConnection();
            // Execute each statement separately without transaction for DDL
            try
            {
                using var createCommand = connection.CreateCommand();
                // Format the create table statement carefully
                createCommand.CommandText =
                    $@"
                    CREATE TABLE {tempTable} (
                        {tableStructure}
                    )";

                // Set command timeout to handle longer DDL operations
                createCommand.CommandTimeout = 120;

                // Use ExecuteNonQuery without async for DDL operations
                await createCommand.ExecuteNonQueryAsync();
                return 1;
                // var as = await createCommand.ExecuteNonQueryAsync().ToString();
                // return 1;
                /// First try to drop the table
            }
            catch (OverflowException es)
            {
                Console.WriteLine($"Error msg: {es.Message}");
                // await TruncateTable();
                return -1;
            }
            catch (OdbcException er)
            {
                Console.WriteLine($"Tabla {tempTable} ya existe...\n Truncandola");
                if (er.Message.Contains("ya existe") || er.Message.Contains("SQL0601"))
                {
                    await TruncateTable(tempTable);
                    return 1;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Creation of table failed. Query: \nError: {ex.Message}");
                // return 0;
                throw;
            }
        }

        /// <summary>
        /// Attempts to truncate the specified temporary table in the ODBC database.
        /// </summary>
        /// <param name="tempTable">The name of the temporary table to truncate.</param>
        /// <returns>Returns 1 if the table was successfully truncated, or 0 if the table does not exist or an error occurs.</returns>
        /// <exception cref="OverflowException">Thrown when an overflow error occurs during the operation.</exception>
        /// <exception cref="OdbcException">Thrown when an ODBC-related error occurs, except when the table does not exist.</exception>
        /// <exception cref="Exception">Thrown when any other error occurs during the operation.</exception>

        private async Task<int> TruncateTable(string tempTable)
        {
            using var connection = await CreateOdbcConnection();
            // var tempTable = GetddOdbcTempLibFile();
            try
            {
                using var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = $"TRUNCATE TABLE {tempTable}";
                var asd = await dropCommand.ExecuteNonQueryAsync();
                // Console.WriteLine($"affected truncated rows: {asd}");
                if (asd < 0)
                {
                    _logger.LogInformation($"Tabla truncada con exito");
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (OverflowException es)
            {
                Console.WriteLine($"Error msg: {es.Message}");
                Console.WriteLine($"asd {es} \n {es.Message.Split(" ")[1].Split()}");
                throw;
            }
            catch (OdbcException ea)
            {
                if (ea.Message.Contains("does not exist"))
                {
                    return 0;
                }
                // Console.WriteLine($"asd {ea.ErrorCode} \n {ea.Message.Split(" ")[1].Split()}");
                return 1;
            }
            catch (Exception ex)
            {
                // Ignore error if table doesn't exist
                _logger.LogInformation($"Drop table attempted: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Creates a new ODBC connection using the configured connection string.
        /// </summary>
        /// <returns>The newly created ODBC connection.</returns>
        /// <exception cref="OverflowException">When the connection string is too long and the executable is not compiled as 32-bit.</exception>
        /// <exception cref="OdbcException">When an ODBC error occurs while creating the connection.</exception>
        /// <exception cref="Exception">When an unexpected error occurs while creating the connection.</exception>
        public async Task<OdbcConnection> CreateOdbcConnection()
        {
            _logger.LogInformation(
                "Creating OBDC Connection with connection string: {ConnectionString}",
                _connectionString
            );

            try
            {
                var connection = new OdbcConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("CreateOdbcConnection: Connection opened successfully");
                return connection;
            }
            catch (OverflowException ex)
            {
                _logger.LogError(
                    ex,
                    "Overflow error while creating connection-> {ErrorMessage}\n\n Es necesario compilar en 32bits",
                    ex.Message
                );
                throw; // Rethrow to maintain the error
            }
            catch (OdbcException ex)
            {
                _logger.LogError(ex, "ODBC error while creating connection");
                throw; // Rethrow to maintain the error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating connection");
                throw;
            }
        }
    }
}
