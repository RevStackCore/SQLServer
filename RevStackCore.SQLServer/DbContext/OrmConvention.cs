using System;
using System.Collections.Generic;
using System.Text;

namespace RevStackCore.SQLServer.DbContext
{
    public class OrmConvention : Dapper.FastCrud.Configuration.OrmConventions
    {
        public OrmConvention()
        {
            this.ClearEntityToTableNameConversionRules();
        }
    }
}
