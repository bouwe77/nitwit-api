using Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Data.Repositories
{
    public class SqliteRepository<TEntity> : IDisposable where TEntity : ISqliteEntity, new()
    {
        protected readonly SQLiteConnection _sqliteConnection;

        public SqliteRepository(string databaseFilePath)
        {
            _sqliteConnection = new SQLiteConnection(databaseFilePath);
            new TableCreator().CreateTablesIfNecessary(_sqliteConnection);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _sqliteConnection.Table<TEntity>().ToList();
        }

        public TEntity GetById(string id)
        {
            return Find(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, bool resolveForeignKeys = true)
        {
            return _sqliteConnection.Table<TEntity>().AsQueryable().Where(predicate.Compile()).ToList();
        }

        public void Insert(TEntity entity)
        {
            _sqliteConnection.Insert(entity);
        }

        public void Update(TEntity entity)
        {
            _sqliteConnection.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _sqliteConnection.Delete(entity);
        }

        public void DeleteAll()
        {
            _sqliteConnection.DeleteAll<TEntity>();
        }

        public void Dispose()
        {
            _sqliteConnection?.Dispose();
        }
    }
}
