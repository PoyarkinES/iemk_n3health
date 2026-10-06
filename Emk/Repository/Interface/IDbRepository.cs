using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;

namespace Emk.Repository.Interface
{
    public interface IDbRepository
    {
        Task<T> Scalar<T>(string commandText, params OdbcParameter[] args);

        Task<List<T>> Query<T>(string commandText, Func<IDataReader, T> map, params OdbcParameter[] args);

        Task<List<T>> Query<T>(string commandText, Func<IDataReader, T> map, object @object, CommandType commandType);

        Task<int> ExecuteNonQuery(string commandText, params OdbcParameter[] args);
    }
}
