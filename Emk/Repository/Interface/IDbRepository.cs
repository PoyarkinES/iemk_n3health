using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Emk.Repository.Interface
{
    public interface IDbRepository
    {
        T Scalar<T>(string commandText, params object[] args);

        IEnumerable<T> Query<T>(string commandText, Func<IDataReader, T> map, params SqlParameter[] args);

        IEnumerable<T> Query<T>(string commandText, Func<IDataReader, T> map, object @object, CommandType commandType);

        int ExecuteNonQuery(string commandText, params object[] args);
    }
}
