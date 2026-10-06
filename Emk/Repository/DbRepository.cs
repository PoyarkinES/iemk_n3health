using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Reflection;
using System.Threading.Tasks;
using Emk.Repository.Interface;
using System.Linq;
using System.Text.RegularExpressions;

namespace Emk.Repository
{
    public abstract class DbRepository: IDbRepository
    {
        private readonly string m_ConnectionString;

        protected DbRepository(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public async Task<T> Scalar<T>(string commandText, params OdbcParameter[] args)
        {
            try
            {
                using var connection = new OdbcConnection(m_ConnectionString);
                using var command = connection.CreateCommand();
                BindParameters(command, commandText, args);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                    return default;

                return (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception("Ошибка при получении данных из БД.", e);
            }
        }

        public async Task<List<T>> Query<T>(string commandText, Func<IDataReader, T> map, params OdbcParameter[] args)
        {
            try
            {
                var result = new List<T>();

                using var connection = new OdbcConnection(m_ConnectionString);
                using var command = connection.CreateCommand();
                BindParameters(command, commandText, args);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(map(reader));
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception("Ошибка при получении данных из БД.", e);
            }
        }

        public async Task<List<T>> Query<T>(string commandText, Func<IDataReader, T> map, object @object, CommandType commandType)
        {
            try
            {
                var result = new List<T>();

                using var connection = new OdbcConnection(m_ConnectionString);
                using var command = (OdbcCommand)connection.CreateCommand(commandType);
                var parameters = new List<OdbcParameter>();
                foreach (var prop in @object.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    parameters.Add(Parameter(prop.Name, prop.GetValue(@object, null)));
                }
                BindParameters(command, commandText, parameters.ToArray());

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(map(reader));
                }
                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception("Ошибка при получении данных из БД.", e);
            }
        }

        public async Task<int> ExecuteNonQuery(string commandText, params OdbcParameter[] args)
        {
            try
            {
                using var connection = new OdbcConnection(m_ConnectionString);
                using var command = connection.CreateCommand();
                BindParameters(command, commandText, args);

                await connection.OpenAsync();
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception("Ошибка при выполнении команды в БД.", e);
            }
        }

        private static void BindParameters(OdbcCommand command, string commandText, OdbcParameter[] parameters)
        {
            var lookup = parameters.ToDictionary(
                parameter => parameter.ParameterName.TrimStart('@'),
                parameter => parameter,
                System.StringComparer.OrdinalIgnoreCase);

            command.CommandText = Regex.Replace(commandText, @"@([A-Za-z_][A-Za-z0-9_]*)", match =>
            {
                if (parameters.Length == 0)
                    return match.Value;

                var name = match.Groups[1].Value;
                if (!lookup.TryGetValue(name, out var value))
                    throw new System.InvalidOperationException($"No value supplied for SQL parameter '{name}'.");

                command.Parameters.Add(new OdbcParameter
                {
                    Value = value.Value ?? System.DBNull.Value
                });
                return "?";
            });
        }

        protected static OdbcParameter Parameter(string name, object value)
        {
            return new OdbcParameter
            {
                ParameterName = name,
                Value = value ?? System.DBNull.Value
            };
        }
    }
}
