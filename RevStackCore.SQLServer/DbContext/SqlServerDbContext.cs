using System;
namespace RevStackCore.SQLServer
{
    public class SQLServerDbContext
    {
        public string ConnectionString { get;  }
        public SQLServerDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
