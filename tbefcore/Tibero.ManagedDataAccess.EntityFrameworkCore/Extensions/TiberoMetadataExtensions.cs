using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Metadata;
namespace Tibero.EntityFrameworkCore.Extensions
{
    public static class TiberoMetadataExtensions
    {
        public static TiberoPropertyAnnotations Tibero([NotNull] this IMutableProperty property)
           => (TiberoPropertyAnnotations)Tibero((IProperty)property);
        public static ITiberoPropertyAnnotations Tibero([NotNull] this IProperty property)
            => new TiberoPropertyAnnotations(Check.NotNull(property, nameof(property)));
        
        public static ITiberoModelAnnotations Tibero([NotNull] this IModel model)
             => new TiberoModelAnnotations(Check.NotNull(model, nameof(model)));

        public static TiberoModelAnnotations Tibero([NotNull] this IMutableModel model)
            => (TiberoModelAnnotations)Tibero((IModel)model);
        public static TiberoEntityTypeAnnotations Tibero([NotNull] this IMutableEntityType entityType)
       => (TiberoEntityTypeAnnotations)Tibero((IEntityType)entityType);

       public static ITiberoEntityTypeAnnotations Tibero([NotNull] this IEntityType entityType)
            => new TiberoEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));
    }
}
