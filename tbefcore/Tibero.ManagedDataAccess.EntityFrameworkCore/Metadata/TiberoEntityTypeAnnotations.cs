using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Tibero.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
namespace Tibero.EntityFrameworkCore.Metadata
{
    public class TiberoEntityTypeAnnotations : RelationalEntityTypeAnnotations, ITiberoEntityTypeAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IEntityType" />.
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" /> to use. </param>
        public TiberoEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IEntityType" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IEntityType" /> to annotate.
        /// </param>
        public TiberoEntityTypeAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

    }
}
