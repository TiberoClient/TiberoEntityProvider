using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Tibero.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Utilities;

namespace Tibero.EntityFrameworkCore.Extensions
{
    public static class TiberoModelBuilderExtensions
    {
        /*
         * sequence 는 모델에, identitycolumn 은 property 에.
         */ 
        public static ModelBuilder ForTiberoUseSequenceHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var model = modelBuilder.Model;

            name = name ?? TiberoModelAnnotations.DefaultHiLoSequenceName;

            if (model.Tibero().FindSequence(name, schema) == null)
            {
                modelBuilder.HasSequence(name, schema).IncrementsBy(10);
            }

            model.Tibero().ValueGenerationStrategy = TiberoValueGenerationStrategy.SequenceHiLo;
            model.Tibero().HiLoSequenceName = name;
            model.Tibero().HiLoSequenceSchema = schema;

            return modelBuilder;
        }

        
        public static ModelBuilder ForTiberoUseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Tibero().ValueGenerationStrategy = TiberoValueGenerationStrategy.IdentityColumn;
            property.Tibero().HiLoSequenceName = null;
            property.Tibero().HiLoSequenceSchema = null;

            return modelBuilder;
        }
    }
}
