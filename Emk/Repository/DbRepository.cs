using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Emk.Interface;

namespace Emk.Repository
{
	public abstract class DbRepository: IDbRepository
    {
        private readonly string m_ConnectionString;
		//protected OdbcConnection Connection => Factory.GetDbConnection();

        protected DbRepository(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public T Scalar<T>(string commandText, params object[] args)
        {
            try
            {
                T result; 

                using (var connection = new SqlConnection(m_ConnectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;

                        foreach (var value in args)
                        {
                            var param = command.CreateParameter();
                            param.Value = value;
                            command.Parameters.Add(param);
                        }
                        result = (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
        }

        public IEnumerable<T> Query<T>(string commandText, Func<IDataReader, T> map, params object[] args)
        {
            try
            {
                var result = new List<T>();

                using (var connection = new SqlConnection(m_ConnectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;

                        foreach (var value in args)
                        {
                            var param = command.CreateParameter();
                            param.Value = value;
                            command.Parameters.Add(param);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(map(reader));
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
        }

        public IEnumerable<T> Query<T>(string commandText, Func<IDataReader, T> map, object @object, CommandType commandType)
        {
            try
            {
                var result = new List<T>();

                using (var connection = new SqlConnection(m_ConnectionString))
                {
                    using (var command = connection.CreateCommand(commandType))
                    {
                        command.CommandText = commandText;

                        foreach (var prop in @object.GetType()
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var param = command.CreateParameter();
                            param.ParameterName = prop.Name;
                            param.Value = prop.GetValue(@object, null);
                            command.Parameters.Add(param);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(map(reader));
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
        }

        public int ExecuteNonQuery(string commandText, params object[] args)
        {
            try
            {
                using (var connection = new SqlConnection(m_ConnectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = commandText;

                        foreach (var value in args)
                        {
                            var param = command.CreateParameter();
                            param.Value = value;
                            command.Parameters.Add(param);
                        }

                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'ExecuteNonQuery'. Ошибка при получении данных из БД" + e.ToString());
            }
        }
    }
}
