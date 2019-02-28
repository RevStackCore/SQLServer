using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using RevStackCore.Pattern;
using RevStackCore.Extensions.SQL;

namespace RevStackCore.SQLServer
{
    public static class Extensions
    {
        public static SQLServerDbContext RegisterTable<T,TKey>(this SQLServerDbContext context) where T : class, IEntity<TKey>
        {
            string cmdText;
            List<SqlDataColumn> indexes;
            Type type = typeof(T);
            var metadataSchema = type.ToMetadataSqlSchema();
            metadataSchema = SqlServerTypeMapping.Map(metadataSchema);
            string conn = context.ConnectionString;
            var utility = new SqlUtility<SqlConnection, SqlCommand>(conn);
            if (utility.TableExists(metadataSchema.TableName))
            {
                var existingColumns = getTableColumns(conn, metadataSchema.TableName);
                cmdText = alterTableCommandText(metadataSchema, existingColumns);
                if (!string.IsNullOrEmpty(cmdText))
                {
                    utility.ExecuteSqlCommand(cmdText);
                    indexes = getIndexes(metadataSchema, existingColumns);
                    foreach (var index in indexes)
                    {
                        cmdText = createIndexCommandText(index, metadataSchema.TableName);
                        utility.ExecuteSqlCommand(cmdText);
                    }
                }
            }
            else
            {
                cmdText = createTableCommandText(metadataSchema);
                utility.ExecuteSqlCommand(cmdText);
                indexes = getIndexes(metadataSchema);
                foreach (var index in indexes)
                {
                    cmdText = createIndexCommandText(index, metadataSchema.TableName);
                    utility.ExecuteSqlCommand(cmdText);
                }
            }

            return context;
        }

        private static List<string> getTableColumns(string connectionString, string tableName)
        {
            var columns = new List<string>();
            string query = $"SELECT * FROM {tableName} where 1=0";
            ReadOnlyCollection<DbColumn> columnSchema;
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = cnn.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cnn.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                    {
                        columnSchema = rdr.GetColumnSchema();
                    }
                    cnn.Close();
                }
            }
            foreach (var column in columnSchema)
            {
                columns.Add(column.ColumnName);
            }

            return columns;
        }

        private static string createTableCommandText(MetadataSqlSchema schema)
        {
            string cmd = $"CREATE TABLE {schema.TableName}( ";
            var columns = schema.Columns;
            foreach(var column in columns)
            {
                cmd += $"{column.Name} {column.DbTypeName}";
                if(column.IsAutoIncrementing)
                {
                    cmd += $" Identity(1,1)";
                }
                if (!column.AllowNulls)
                {
                    cmd += $" NOT NULL";
                }
                cmd += ",";
            }
            string primaryKey = getPrimaryKeyText(schema);
            cmd += $" {primaryKey})";
            return cmd;
        }

        private static string alterTableCommandText(MetadataSqlSchema schema, List<string> existingColumns)
        {
            string cmd = null;
            var schemaColumns = schema.Columns;
            var columns = schemaColumns.Where(x => !existingColumns.Any(y => y == x.Name));
            if(columns.Any())
            {
                cmd = $"ALTER TABLE {schema.TableName} ADD ";
                int count = columns.Count();
                int index = 0;
                foreach (var column in columns)
                {
                    index += 1;
                    cmd += $"{column.Name} {column.DbTypeName}";
                    if (column.IsAutoIncrementing)
                    {
                        cmd += $" Identity(1,1)";
                    }
                    if (!column.AllowNulls)
                    {
                        cmd += $" NOT NULL";
                    }
                    if (index < count)
                    {
                        cmd += ", ";
                    }
                }
            }
            return cmd;
        }

        private static string createIndexCommandText(SqlDataColumn column, string tableName)
        {
            string cmd = "CREATE";
            if(column.IsUniqueIndex)
            {
                cmd += " UNIQUE";
            }
            if (column.IsClusteredIndex)
            {
                cmd += " CLUSTERED";
            }
            cmd += $" INDEX idx_{column.Name} ON {tableName}({column.Name})";
            return cmd;
        }

        private static List<SqlDataColumn> getIndexes(MetadataSqlSchema schema)
        {
            var indexes = schema.Columns.Where(x => x.IsIndex == true);
            if(indexes.Any())
            {
                return indexes.ToList();
            }
            else
            {
                return new List<SqlDataColumn>();
            }
        }

        private static List<SqlDataColumn> getIndexes(MetadataSqlSchema schema, List<string> existingColumns)
        {
            var schemaColumns = schema.Columns;
            var columns = schemaColumns.Where(x => !existingColumns.Any(y => y == x.Name));
            if (columns.Any())
            {
                return columns.ToList();
            }
            else
            {
                return new List<SqlDataColumn>();
            }
        }

        private static string getPrimaryKeyText(MetadataSqlSchema schema)
        {
            string ids = "Id";
            string cmd= $"CONSTRAINT PK_{schema.TableName} PRIMARY KEY";
            var columns = schema.Columns.Where(x=>x.IsPrimaryKey==true);
            if(columns.Any())
            {
                int count = columns.Count();
                int index = 0;
                ids = "";
                foreach (var column in columns)
                {
                    index += 1;
                    ids += column.Name;
                    if (index < count)
                    {
                        ids += ",";
                    }
                }
            }
            cmd += $"({ids})";
            return cmd;
        }
    }
}
