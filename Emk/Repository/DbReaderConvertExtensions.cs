using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emk.Repository
{
    public static class DbReaderConvertExtensions
    {
        public static T Get<T>(this IDataReader reader, string name) => reader[name].To<T>(default(T));

        private static T To<T>(this object value, T defaultValue)
        {
            if (value == DBNull.Value || value == null)
                return defaultValue;
            Type nullableType = typeof(T);
            Type type1 = Nullable.GetUnderlyingType(nullableType);
            if ((object) type1 == null)
                type1 = nullableType;
            Type type2 = type1;
            return (T) (!type2.IsEnum
                ? Convert.ChangeType(value, type2)
                : Enum.Parse(type2, Convert.ToString(value), true));
        }
    }
}
