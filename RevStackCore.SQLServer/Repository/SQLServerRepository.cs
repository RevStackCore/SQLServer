using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.SqlClient;
using RevStackCore.Pattern;
using RevStackCore.SQL.Client;
using RevStackCore.Extensions.SQL;

namespace RevStackCore.SQLServer
{
    public class SQLServerRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly TypedClient<TEntity,SqlConnection,TKey> _typedClient;
        public SQLServerRepository(SQLServerDbContext context)
        {
            _typedClient = new TypedClient<TEntity, SqlConnection,TKey>(context.ConnectionString,SQLLanguageType.SQLServer);
        }

        public TEntity Add(TEntity entity)
        {
            return _typedClient.Insert(entity);
        }

        public void Delete(TEntity entity)
        {
            _typedClient.Delete(entity);
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _typedClient.Find(predicate);
        }

        public IEnumerable<TEntity> Get()
        {
            return _typedClient.GetAll();
        }

        public TEntity GetById(TKey id)
        {
            return _typedClient.GetById(id);
        }

        public TEntity Update(TEntity entity)
        {
            return _typedClient.Update(entity);
        }
    }
}
