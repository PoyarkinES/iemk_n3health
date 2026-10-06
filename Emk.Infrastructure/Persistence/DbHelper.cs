using System;
using System.Data;
using System.Data.Common;

namespace Emk.Infrastructure.Persistence
{
    public static class DbHelper
    {
        public static DbCommand CreateCommand(this DbConnection connection, CommandType commandType)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            var command = connection.CreateCommand();
            command.CommandType = commandType;
            return command;
        }

        public static T Get<T>(this IDataReader reader, string name)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var value = reader[name];
            if (value == null || value == DBNull.Value || string.IsNullOrEmpty(value.ToString()))
                return default(T);

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return targetType.IsEnum
                ? (T)Enum.Parse(targetType, Convert.ToString(value), true)
                : (T)Convert.ChangeType(value, targetType);
        }
    }
}
