using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NexusStack.EFCore.DbContexts;
using NexusStack.EFCore.Repository.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NexusStack.EFCore.Repository.Base
{
    public abstract class RepositoryBase<TEntity, TKey>(
        MainContext dbContext,
        ISpecificationEvaluator specificationEvaluator) : QueryRepository<TEntity, TKey>(dbContext, specificationEvaluator), IRepositoryBase<TEntity, TKey> where TEntity : class
    {
        public RepositoryBase(MainContext dbContext)
            : this(dbContext, SpecificationEvaluator.Default)
        {
        }

        public virtual async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
        {
            IDbContextTransaction dbContextTransaction = await DbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return dbContextTransaction;
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(entity));

            await Entities.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return entity;
        }

        public virtual async Task InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(entities));

            await DbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(entity));

            Entities.Update(entity);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(entities));

            Entities.UpdateRange(entities);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(entity));

            DbContext.Set<TEntity>().Remove(entity);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(nameof(entities));

            DbContext.Set<TEntity>().RemoveRange(entities);
            return DbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken = default)
        {
            return DbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        public virtual Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return DbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public virtual Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default)
        {
            return DbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        public void ResetContextState()
        {
            DbContext.ChangeTracker.Clear();
        }

        public virtual async Task RollbackAsync(IDbContextTransaction transaction = null)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            else
            {
                // 如果没有提供事务，尝试回滚当前事务
                var currentTransaction = DbContext.Database.CurrentTransaction;
                if (currentTransaction != null)
                {
                    await currentTransaction.RollbackAsync();
                }
            }

            // 清除跟踪的实体状态，确保后续的 SaveChanges 不会重新应用已回滚的更改
            ResetContextState();
        }
    }
}
