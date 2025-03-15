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
            string builderBasisConnectionString;
            // var odbcBasisConnectionString = configuration.GetConnectionString("ODBC_BASIS_DEV");
            var settings = configuration.GetSection("Settings");
            int maxRetries = settings.GetValue<int>("maxRetries");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                Console.WriteLine(
                    $"Enviroment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
                );

                builderBasisConnectionString =
                    configuration.GetConnectionString("BASIS_DEV") ?? string.Empty;
            }
            else
            {
                builderBasisConnectionString =
                    configuration.GetConnectionString("BASIS_PROD") ?? string.Empty;
            }

            if (string.IsNullOrEmpty(builderBasisConnectionString))
            {
                throw new ArgumentNullException("BASIS connection strings are missing");
            }

            if (int.IsNegative(maxRetries))
            {
                throw new ArgumentException("Max Retries no puede ser negativo");
            }

            _logger = logger;
            _connectionString = builderBasisConnectionString;
            _configuration = configuration;
        }

        public string GetBasisLibrary(string negocio)
        {
            string basisLibrary;
            if (negocio == "ARCA")
            {
                basisLibrary = "BASFLEBC";
            }
            else if (negocio == "TONI")
            {
                basisLibrary = "BASFLDIP";
            }
            else if (negocio == "INALECSA")
            {
                basisLibrary = "BASFLCDT";
            }
            else
            {
                throw new ArgumentException("Invalid negocio value");
            }

            return basisLibrary;
        }

        public string GetBasisTempLibFile()
        {
            var settings = _configuration.GetSection("Settings");
            var basisTempFile = settings.GetValue<string>("BasisTmpFile");
            var basistempLib = settings.GetValue<string>("BasisTmpLibrary");
            if (string.IsNullOrEmpty(basisTempFile))
            {
                throw new ArgumentNullException("BasisTempFile connection strings are missing");
            }
            if (string.IsNullOrEmpty(basistempLib))
            {
                throw new ArgumentNullException("BasisTmpLibrary connection strings are missing");
            }
            return basistempLib + "." + basisTempFile;
        }

        public async Task<int> CreateBasisTempDb(string tableStructure, string tempTable)
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

        private async Task<int> TruncateTable(string tempTable)
        {
            using var connection = await CreateOdbcConnection();
            // var tempTable = GetBasisTempLibFile();
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
        /// Crea una conexión ODBC asíncrona a DB2 BASIS utilizando una cadena de conexión especificada en app settings de acuerdo al entorno en que se encuentre, puede ser BASIS_DEV o BASIS_PROD
        /// </summary>
        /// <remarks>
        /// Este método utiliza un pool de conexiones para obtener una conexión existente o crear una nueva si es necesario.
        /// La conexión devuelta ya tiene la comunicación abierta y está lista para ejecutar comandos.
        /// </remarks>
        /// <returns>
        /// Una instancia de <see cref="OdbcConnection"/> con la comunicación abierta.
        /// </returns>
        /// <exception cref="OdbcException">
        /// Se lanza si ocurre un error al intentar abrir la conexión.
        /// </exception>
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
