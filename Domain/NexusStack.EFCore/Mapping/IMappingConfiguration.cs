using Microsoft.EntityFrameworkCore;
using NexusStack.EFCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.EFCore.Mapping
{
    public interface IMappingConfiguration
    {
        void ApplyConfiguration(ModelBuilder modelBuilder);
    }

    public interface IMappingConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>, IMappingConfiguration where TEntity : class, IEntity
    {

    }
}
