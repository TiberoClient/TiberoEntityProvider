using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tibero.EntityFrameworkCore.Infrastructure;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Tibero.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class TiberoDbContextOptionsExtensions
    {
        
        public static DbContextOptionsBuilder UseTibero(
             [NotNull] this DbContextOptionsBuilder optionsBuilder,
             [NotNull] string connectionString,
             [CanBeNull] Action<TiberoDbContextOptionsBuilder> tiberoOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));
            
            var extension = (TiberoOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            tiberoOptionsAction?.Invoke(new TiberoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }
        public static DbContextOptionsBuilder UseTibero(
           [NotNull] this DbContextOptionsBuilder optionsBuilder,
           [NotNull] string connectionString,
           [CanBeNull] params Action<TiberoDbContextOptionsBuilder> [] tiberoOptionsActions )
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = (TiberoOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            foreach (Action<TiberoDbContextOptionsBuilder> action in tiberoOptionsActions)
            {
                action?.Invoke(new TiberoDbContextOptionsBuilder(optionsBuilder));
            }

            return optionsBuilder;
        }
       
        public static DbContextOptionsBuilder UseTibero(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<TiberoDbContextOptionsBuilder> tiberoOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = (TiberoOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            tiberoOptionsAction?.Invoke(new TiberoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        
        public static DbContextOptionsBuilder<TContext> UseTibero<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<TiberoDbContextOptionsBuilder> tiberoOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseTibero(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, tiberoOptionsAction);

        
        public static DbContextOptionsBuilder<TContext> UseTibero<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<TiberoDbContextOptionsBuilder> tiberoOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseTibero(
                (DbContextOptionsBuilder)optionsBuilder, connection, tiberoOptionsAction);

        private static TiberoOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<TiberoOptionsExtension>();
            
            return existing != null
                ? new TiberoOptionsExtension(existing)
                : new TiberoOptionsExtension();
        }
    }
}
