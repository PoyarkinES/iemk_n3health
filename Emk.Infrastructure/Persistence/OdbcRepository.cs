using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Emk.Infrastructure.Persistence
{
    public abstract class OdbcRepository
    {
        private readonly string _connectionString;

        protected OdbcRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A database connection string is required.", nameof(connectionString));

            _connectionString = connectionString;
        }

        public async Task<T> Scalar<T>(string commandText, params OdbcParameter[] parameters)
        {
            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    BindParameters(command, commandText, parameters);
                    await connection.OpenAsync().ConfigureAwait(false);
                    var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
                    return result == null || result == DBNull.Value
                        ? default(T)
                        : (T)Convert.ChangeType(result, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
                }
            }
            catch (Exception exception)
            {
                Emk.Log.Error(exception.ToString());
                throw new Exception("Ошибка при получении данных из БД.", exception);
            }
        }

        public async Task<List<T>> Query<T>(
            string commandText,
            Func<IDataReader, T> map,
            params OdbcParameter[] parameters)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            try
            {
                var results = new List<T>();
                using (var connection = new OdbcConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    BindParameters(command, commandText, parameters);
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            results.Add(map(reader));
                    }
                }

                return results;
            }
            catch (Exception exception)
            {
                Emk.Log.Error(exception.ToString());
                throw new Exception("Ошибка при получении данных из БД.", exception);
            }
        }

        public async Task<List<T>> Query<T>(
            string commandText,
            Func<IDataReader, T> map,
            object values,
            CommandType commandType)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var parameters = values.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => Parameter(property.Name, property.GetValue(values, null)))
                .ToArray();

            try
            {
                var results = new List<T>();
                using (var connection = new OdbcConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = commandType;
                    BindParameters(command, commandText, parameters);
                    await connection.OpenAsync().ConfigureAwait(false);
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            results.Add(map(reader));
                    }
                }

                return results;
            }
            catch (Exception exception)
            {
                Emk.Log.Error(exception.ToString());
                throw new Exception("Ошибка при получении данных из БД.", exception);
            }
        }

        public async Task<int> ExecuteNonQuery(string commandText, params OdbcParameter[] parameters)
        {
            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                using (var command = connection.CreateCommand())
                {
                    BindParameters(command, commandText, parameters);
                    await connection.OpenAsync().ConfigureAwait(false);
                    return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                Emk.Log.Error(exception.ToString());
                throw new Exception("Ошибка при выполнении команды в БД.", exception);
            }
        }

        protected static OdbcParameter Parameter(string name, object value)
        {
            return new OdbcParameter
            {
                ParameterName = name,
                Value = value ?? DBNull.Value
            };
        }

        private static void BindParameters(
            OdbcCommand command,
            string commandText,
            OdbcParameter[] parameters)
        {
            parameters = parameters ?? new OdbcParameter[0];
            var lookup = parameters.ToDictionary(
                parameter => parameter.ParameterName.TrimStart('@'),
                parameter => parameter,
                StringComparer.OrdinalIgnoreCase);

            command.CommandText = Regex.Replace(commandText, @"@([A-Za-z_][A-Za-z0-9_]*)", match =>
            {
                if (parameters.Length == 0)
                    return match.Value;

                var name = match.Groups[1].Value;
                OdbcParameter parameter;
                if (!lookup.TryGetValue(name, out parameter))
                    throw new InvalidOperationException("No value supplied for SQL parameter '" + name + "'.");

                command.Parameters.Add(new OdbcParameter { Value = parameter.Value ?? DBNull.Value });
                return "?";
            });
        }
    }
}
