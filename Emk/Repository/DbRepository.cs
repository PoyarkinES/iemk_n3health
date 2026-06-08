using System.Data.Odbc;

namespace Emk.Repository
{
	public abstract class DbRepository
	{
		protected OdbcConnection Connection => Factory.GetDbConnection();
		
	}
}
