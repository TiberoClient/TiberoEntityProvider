using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Metadata.Internal;

namespace Tibero.EntityFrameworkCore.Metadata.Internal
{
    public static class TiberoInternalMetadataBuilderExtensions
    {
       /*
        * 1. Extension method 를 모아둔 static class. 
        * 2. Metadata라고 붙여진 이유는, annotation 관련하여 model, key, index, property 등의 meta를 생성하는 메서드를 모아두었기 때문이다. 
        * 3. 
        */ 
        
        public static TiberoModelBuilderAnnotations Tibero(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new TiberoModelBuilderAnnotations(builder, configurationSource);
            
        public static TiberoPropertyBuilderAnnotations Tibero(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new TiberoPropertyBuilderAnnotations(builder, configurationSource);
            
        public static RelationalEntityTypeBuilderAnnotations Tibero(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource);

        public static RelationalKeyBuilderAnnotations Tibero(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource);

        public static RelationalIndexBuilderAnnotations Tibero(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource);

        public static RelationalForeignKeyBuilderAnnotations Tibero(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource);
    }
}
