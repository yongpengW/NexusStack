using Ardalis.Specification;
using LinqKit;
using Microsoft.EntityFrameworkCore.Query;
using NexusStack.EFCore.Repository.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using X.PagedList;

namespace NexusStack.EFCore.Repository.AutoMapper
{
    public interface IAutoMapperRepository<TEntity, TKey> : IRepositoryBase<TEntity, TKey> where TEntity : class
    {
        Task<TProjectedType> GetAsync<TProjectedType>(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<TProjectedType> GetAsync<TProjectedType>(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<TProjectedType> GetByIdAsync<TProjectedType>(TKey id, CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<IPagedList<TProjectedType>> GetPagedListAsync<TProjectedType>(int pageIndex = 1,
                                                                               int pageSize = 10,
                                                                               Expression<Func<TEntity, bool>> condition = null,
                                                                               Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
                                                                               Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orders = null,
                                                                               bool asNoTracking = true,
                                                                               CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<IPagedList<TProjectedType>> GetPagedListAsync<TProjectedType>(ISpecification<TEntity> specification, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<List<TProjectedType>> GetListAsync<TProjectedType>(Expression<Func<TEntity, bool>> condition = null,
                                                                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
                                                                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orders = null,
                                                                bool asNoTracking = true,
                                                                CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<List<TProjectedType>> GetListAsync<TProjectedType>(ISpecification<TEntity> specification, CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<IPagedList<TProjectedType>> GetPagedListAsync<TProjectedType>(ExpressionStarter<TEntity> expression, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default) where TProjectedType : class;

        Task<List<TProjectedType>> GetListAsync<TProjectedType>(ExpressionStarter<TEntity> expression, CancellationToken cancellationToken = default) where TProjectedType : class;
    }
}
