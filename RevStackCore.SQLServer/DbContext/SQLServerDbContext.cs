using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data.Common;
using Dapper.FastCrud;
using System.Linq;
using System.Linq.Expressions;
using RevStackCore.SQLServer.Query;
using System.Data.SqlClient;

namespace RevStackCore.SQLServer.DbContext
{
    public class SQLServerDbContext
    {
        private readonly DbConnection _connection;

        public SQLServerDbContext(string connectionString)
        {
            OrmConfiguration.DefaultDialect = SqlDialect.MsSql;
            OrmConfiguration.Conventions = new OrmConvention();

            _connection = new SqlConnection(connectionString);
        }

        public TEntity Add<TEntity>(TEntity entity, bool useAsyncMethods)
        {
            var type = entity.GetType();
            var name = type.Name;
            var idProperty = type.GetProperty("Id");

            if (idProperty == null)
            {
                throw new Exception("Id is required.");
            }

            try
            {
                _connection.Open();

                if (useAsyncMethods)
                    _connection.InsertAsync<TEntity>((TEntity)entity).GetAwaiter().GetResult();
                else
                    _connection.Insert<TEntity>((TEntity)entity);

                return entity;
            }
            catch (Exception ex)
            {
                return default(TEntity);
            }
            finally
            {
                _connection.Close();
            }
        }

        public TEntity Update<TEntity>(TEntity entity, bool useAsyncMethods)
        {
            var type = entity.GetType();
            var name = type.Name;
            var idProperty = type.GetProperty("Id");

            if (idProperty == null)
            {
                throw new Exception("Id is required.");
            }

            try
            {
                _connection.Open();

                if (useAsyncMethods)
                    _connection.UpdateAsync<TEntity>((TEntity)entity).GetAwaiter().GetResult();
                else
                    _connection.Update<TEntity>((TEntity)entity);

                return entity;
            }
            catch (Exception ex)
            {
                return default(TEntity);
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete<TEntity>(TEntity entity, bool useAsyncMethods)
        {
            var type = entity.GetType();
            var idProperty = type.GetProperty("Id");

            if (idProperty == null)
            {
                throw new Exception("Id is required.");
            }

            try
            {
                _connection.Open();

                if (useAsyncMethods)
                    _connection.DeleteAsync<TEntity>((TEntity)entity).GetAwaiter().GetResult();
                else
                    _connection.Delete<TEntity>((TEntity)entity);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _connection.Close();
            }
        }

        public IEnumerable<IEntity> Get<IEntity>()
        {
            try
            {
                _connection.Open();

                return _connection.Find<IEntity>();
            }
            catch (Exception ex)
            {
                return default(IEnumerable<IEntity>);
            }
            finally
            {
                _connection.Close();
            }
        }

        public IEnumerable<IEntity> Find<IEntity>(Expression<Func<IEntity, bool>> predicate)
        {
            try
            {
                _connection.Open();

                DapperQueryProvider queryProvider = new DapperQueryProvider(_connection);
                return new Query.Query<IEntity>(queryProvider).Where(predicate);
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Dispose()
        {
            _connection.Close();
        }

        internal class DapperQueryProvider : QueryProvider
        {
            DbConnection _connection;

            public DapperQueryProvider(DbConnection connection)
            {
                _connection = connection;
            }

            public override object Execute(Expression expression)
            {
                string resultSql = this.Translate(expression);
                var results = Dapper.SqlMapper.Query(_connection, resultSql) as IEnumerable<IDictionary<string, object>>;
                return results;
            }

            public override IEnumerable<T> ExecuteQuery<T>(Expression expression)
            {
                string resultSql = this.Translate(expression);
                Type elementType = TypeSystem.GetElementType(expression.Type);
                return Dapper.SqlMapper.Query<T>(_connection, resultSql);
            }
        }
    }
}
