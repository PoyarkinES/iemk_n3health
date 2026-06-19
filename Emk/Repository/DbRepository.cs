using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using Emk.Repository.Interface;

namespace Emk.Repository
{
    public abstract class DbRepository: IDbRepository
    {
        private readonly string m_ConnectionString;

        protected DbRepository(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public async Task<T> Scalar<T>(string commandText, params SqlParameter[] args)
        {
            try
            {
                using var connection = new SqlConnection(m_ConnectionString);
                using var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var value in args)
                {
                    var param = command.CreateParameter();
                    param.Value = value;
                    command.Parameters.Add(param);
                }

                connection.Open();
                var result = (T) Convert.ChangeType(await command.ExecuteScalarAsync(), typeof(T));
                connection.Close();

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
        }

        public async Task<List<T>> Query<T>(string commandText, Func<IDataReader, T> map, params SqlParameter[] args)
        {
            try
            {
                var result = new List<T>();

                using var connection = new SqlConnection(m_ConnectionString);
                using var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var value in args)
                {
                    var param = command.CreateParameter();
                    param.Value = value;
                    command.Parameters.Add(param);
                }

                connection.Open();
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    result.Add(map(reader));
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
        }

        public async Task<List<T>> Query<T>(string commandText, Func<IDataReader, T> map, object @object, CommandType commandType)
        {
            try
            {
                var result = new List<T>();

                using var connection = new SqlConnection(m_ConnectionString);
                using var command = connection.CreateCommand(commandType);
                command.CommandText = commandText;

                foreach (var prop in @object.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var param = command.CreateParameter();
                    param.ParameterName = prop.Name;
                    param.Value = prop.GetValue(@object, null);
                    command.Parameters.Add(param);
                }

                connection.Open();
                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    result.Add(map(reader));
                }
                connection.Close();
                connection.Dispose();

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
        }

        public async Task<int> ExecuteNonQuery(string commandText, params object[] args)
        {
            try
            {
                var result = -1;

                using var connection = new SqlConnection(m_ConnectionString);
                using var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var value in args)
                {
                    var param = command.CreateParameter();
                    param.Value = value;
                    command.Parameters.Add(param);
                }

                connection.Open();
                result = await command.ExecuteNonQueryAsync();
                connection.Close();
                connection.Dispose();

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'ExecuteNonQuery'. Ошибка при получении данных из БД" + e.ToString());
            }
        }
    }
}
