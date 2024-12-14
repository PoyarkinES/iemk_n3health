using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Emk.Repository
{
	public static class DbHelper
	{
		public static DbCommand CreateCommand(this DbConnection connection, CommandType commandType)
		{
			var command = connection.CreateCommand();
			command.CommandType = commandType;
			return command;
		}
		public static T Scalar<T>(this DbConnection connection, string commandText, params object[] args)
		{
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;

                    foreach (var value in args)
                    {
                        var param = command.CreateParameter();
                        param.Value = value;
                        command.Parameters.Add(param);
                    }

                    return (T)Convert.ChangeType(command.ExecuteScalar(), typeof(T));
                }
            }
            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

        }

        public static DbDataReader Query(this DbConnection connection, string commandText, params object[] args)
		{
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;

                    foreach (var value in args)
                    {
                        var param = command.CreateParameter();
                        param.Value = value;
                        command.Parameters.Add(param);
                    }

                    return command.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

		public static DbDataReader Query(this DbConnection connection, string commandText, object @object, CommandType commandType)
		{
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var command = connection.CreateCommand(commandType)) {
                    command.CommandText = commandText;

                    foreach (var prop in @object.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                        var param = command.CreateParameter();
                        param.ParameterName = prop.Name;
                        param.Value = prop.GetValue(@object, null);
                        command.Parameters.Add(param);
                    }

                    return command.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'Query'. Ошибка при получении данных из БД" + e.ToString());
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public static  int ExecuteNonQuery(this DbConnection connection, string commandText, params object[] args)
		{
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                using (var command = connection.CreateCommand()) {
                    command.CommandText = commandText;

                    foreach (var value in args) {
                        var param = command.CreateParameter();
                        param.Value = value;
                        command.Parameters.Add(param);
                    }

                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                Log.Error(e.ToString());
                throw new Exception($"MethodName: 'ExecuteNonQuery'. Ошибка при получении данных из БД" + e.ToString());
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public static T Get<T>(this IDataReader reader, string name) => reader[name].to<T>(default(T));

        private static T to<T>(this object value, T defaultValue)
        {
            if (value == DBNull.Value || value == null || string.IsNullOrEmpty(value.ToString()))
                return defaultValue;
            Type nullableType = typeof(T);
            Type type1 = Nullable.GetUnderlyingType(nullableType);
            if ((object)type1 == null)
                type1 = nullableType;
            Type type2 = type1;
            return (T)(!type2.IsEnum
                ? Convert.ChangeType(value, type2)
                : Enum.Parse(type2, Convert.ToString(value), true));
        }

    }
}
