using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore.Repository
{
    public static class SpecificationExtensions
    {
        public static Specifications<TEntity> Create<TEntity>(this Specification<TEntity> specification)
        {
            return new Specifications<TEntity>();
            //return null;
        }
    }
}
