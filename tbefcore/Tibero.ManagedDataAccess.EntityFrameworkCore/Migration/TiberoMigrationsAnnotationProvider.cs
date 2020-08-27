using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using JetBrains.Annotations;
using Tibero.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Extensions;
using Tibero.EntityFrameworkCore.Metadata;
namespace Tibero.EntityFrameworkCore.Migration
{
    public class TiberoMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        public TiberoMigrationsAnnotationProvider([NotNull] MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<IAnnotation> For(IModel model) => ForRemove(model);
        public override IEnumerable<IAnnotation> For(IEntityType entityType) => ForRemove(entityType);
        /*
        public override IEnumerable<IAnnotation> For(IKey key)
        {
            var isClustered = key.SqlServer().IsClustered;
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Clustered,
                    isClustered.Value);
            }
        }
        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            var isClustered = index.SqlServer().IsClustered;
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Clustered,
                    isClustered.Value);
            }
        }*/
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.Tibero().ValueGenerationStrategy == TiberoValueGenerationStrategy.IdentityColumn)
            {
                yield return new Annotation(
                    TiberoAnnotationNames.ValueGenerationStrategy,
                    TiberoValueGenerationStrategy.IdentityColumn);
            }
        }
        /*
        public override IEnumerable<IAnnotation> ForRemove(IModel model)
        {
            if (model.GetEntityTypes().Any(e => e.BaseType == null && e.SqlServer().IsMemoryOptimized))
            {
                yield return new Annotation(
                    TiberoAnnotationNames.MemoryOptimized,
                    true);
            }
        }
        public override IEnumerable<IAnnotation> ForRemove(IEntityType entityType)
        {
            if (IsMemoryOptimized(entityType))
            {
                yield return new Annotation(
                    TiberoAnnotationNames.MemoryOptimized,
                    true);
            }
        }

        private static bool IsMemoryOptimized(IEntityType entityType)
            => entityType.GetAllBaseTypesInclusive().Any(t => t.SqlServer().IsMemoryOptimized);
            */

    }

}
