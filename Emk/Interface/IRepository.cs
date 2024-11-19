using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emk.Models;
using Emk.Models.Dto;

namespace Emk.Interface
{
    public interface IDbRepository
    {
        T Scalar<T>(string commandText, params object[] args);

        IEnumerable<T> Query<T>(string commandText, Func<IDataReader, T> map, params object[] args);

        IEnumerable<T> Query<T>(string commandText, Func<IDataReader, T> map, object @object, CommandType commandType);

        int ExecuteNonQuery(string commandText, params object[] args);
    }
}
