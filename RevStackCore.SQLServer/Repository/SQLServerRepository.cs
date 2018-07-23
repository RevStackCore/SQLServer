using System;
using System.Collections.Generic;
using RevStackCore.Pattern;
using RevStackCore.SQLServer.DbContext;
using System.Linq;
using System.Linq.Expressions;

namespace RevStackCore.SQLServer.Repository
{
    public class SQLServerRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
    {
        private readonly SQLServerDbContext _dbContext;
        public SQLServerRepository(SQLServerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual TEntity Add(TEntity entity)
        {
            _dbContext.Add(entity, false);
            return entity;
        }

        public virtual TEntity Update(TEntity entity)
        {
            _dbContext.Update(entity, false);
            return entity;
        }

        public virtual void Delete(TEntity entity)
        {
            _dbContext.Delete(entity, false);
        }

        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _dbContext.Find(predicate).AsQueryable();
        }

        public virtual IEnumerable<TEntity> Get()
        {
            return _dbContext.Get<TEntity>();
        }

        public virtual TEntity GetById(TKey id)
        {

            return Find(x => x.Id.Equals(id)).FirstOrDefault();
        }
    }
}
