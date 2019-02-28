using System;
using System.Data;
using RevStackCore.Extensions.SQL;

namespace RevStackCore.SQLServer
{
    public static class SqlServerTypeMapping
    {
        public static MetadataSqlSchema Map(MetadataSqlSchema schema)
        {
            foreach (var column in schema.Columns)
            {
                column.DbTypeName = column.ToDbTypeName();
            }

            return schema;
        }

        private static string ToDbTypeName(this SqlDataColumn column)
        {
            if (column.DbType != default(SqlDbType))
            {
                return column.ToDbTypeNameFromSqlType();
            }
            else
            {
                return column.Type.ToDbTypeNameFromType();
            }
        }

        private static string ToDbTypeNameFromSqlType(this SqlDataColumn column)
        {
            var type = column.DbType;
            var size = column.Size;
            var precision = column.Precision;
            if (type == SqlDbType.BigInt)
            {
                return "bigint";
            }
            else if (type == SqlDbType.Bit)
            {
                return "bit";
            }
            else if (type == SqlDbType.Binary)
            {
                return "binary";
            }
            else if (type == SqlDbType.Char)
            {
                if (size != default(int))
                {
                    return $"char({size})";
                }
                return "char";
            }
            else if (type == SqlDbType.Date)
            {
                return "date";
            }
            else if (type == SqlDbType.DateTime)
            {
                return "datetime";
            }
            else if (type == SqlDbType.DateTime2)
            {
                return "datetime";
            }
            else if (type == SqlDbType.DateTimeOffset)
            {
                return "datetimeoffset";
            }
            else if (type == SqlDbType.Decimal)
            {
                if (size != default(int) && precision != default(int))
                {
                    return $"decimal({size},{precision})";
                }
                else if (size != default(int))
                {
                    return $"decimal({size})";
                }
                else
                {
                    return "decimal";
                }
            }
            else if (type == SqlDbType.Float)
            {
                if (size != default(int) && precision != default(int))
                {
                    return $"float({size},{precision})";
                }
                else if (size != default(int))
                {
                    return $"float({size})";
                }
                else
                {
                    return "float";
                }
            }
            else if (type == SqlDbType.Image)
            {
                return "binary";
            }
            else if (type == SqlDbType.Int)
            {
                return "int";
            }
            else if (type == SqlDbType.BigInt)
            {
                if (size != default(int))
                {
                    return $"bigint({size})";
                }
                else
                {
                    return "bigint";
                }
            }
            else if (type == SqlDbType.Money)
            {
                return "money";
            }
            else if (type == SqlDbType.NChar)
            {
                if (size != default(int))
                {
                    return $"nchar({size})";
                }
                else
                {
                    return "nchar";
                }
            }
            else if (type == SqlDbType.NVarChar)
            {
                if (size != default(int))
                {
                    return $"nvarchar({size})";
                }
                else
                {
                    return "nvarchar(100)";
                }
            }
            else if (type == SqlDbType.Real)
            {
                if (size != default(int) && precision != default(int))
                {
                    return $"real({size},{precision})";
                }
                else if (size != default(int))
                {
                    return $"real({size})";
                }
                else
                {
                    return "real";
                }
            }
            else if (type == SqlDbType.SmallInt)
            {
                return "smallint";
            }
            else if (type == SqlDbType.SmallMoney)
            {
                return "smallmoney";
            }
            else if (type == SqlDbType.SmallDateTime)
            {
                return "smalldatetime";
            }
            else if (type == SqlDbType.Text)
            {
                return "text";
            }
            else if (type == SqlDbType.Time)
            {
                return "time";
            }
            else if (type == SqlDbType.TinyInt)
            {
                return "tinyint";
            }
            else if (type == SqlDbType.UniqueIdentifier)
            {
                return "timestamp";
            }
            else if (type == SqlDbType.UniqueIdentifier)
            {
                return "uniqueidentifier";
            }
            else if (type == SqlDbType.VarBinary)
            {
                return "varbinary";
            }
            else if (type == SqlDbType.VarChar)
            {
                if (size != default(int))
                {
                    return $"varchar({size})";
                }
                else
                {
                    return "varchar(100)";
                }
            }

            return type.ToString();
        }

        private static string ToDbTypeNameFromType(this Type type)
        {
            if (type == typeof(string))
            {
                return "varchar(100)";
            }
            else if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(short))
            {
                return "int";
            }
            else if (type == typeof(long))
            {
                return "bigint";
            }
            else if (type == typeof(decimal))
            {
                return "decimal";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(double))
            {
                return "float";
            }
            else if (type == typeof(DateTime))
            {
                return "datetime";
            }
            else if (type == typeof(bool))
            {
                return "bit";
            }
            else if (type == typeof(DateTimeOffset))
            {
                return "datetimeoffset";
            }
            else if (type == typeof(Byte[]))
            {
                return "binary";
            }
            else if (type == typeof(TimeSpan))
            {
                return "time";
            }
            else if (type == typeof(char[]))
            {
                return "char";
            }
            else if (type == typeof(Guid))
            {
                return "uniqueidentifier";
            }

            return "varchar(100)";

        }
    }
}
