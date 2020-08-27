using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Metadata.Internal;

namespace Tibero.EntityFrameworkCore.Extensions
{
    public static class TiberoPropertyBuilderExtensions
    {
        /*
         * 이 static class 는 확장 메서드를 사용하기 위한 클래스이다. 이름에서 보면 알수 있듯이, Property Builder를 사용하는 class.
         * Property Builder는 model create 시, Property 라는 메서드로 특정 엔티티의 한 property 의 특성을 build 하는 클래스.
         */ 
         /*
          * 이제 MS 에서와 같이 순차적으로 확장 메서드를 추가해나갈 건데, 내용에서 보면, Tibero() 와 같은 확장 메서드가 많이 보일 것이다.
          * 이는 각 Annotation들을 생성하고, Annotation 안에 있는 property 에 해당 property (다른 것도 마찬가지일듯.)를 달아놓고 쓰는듯.
          * 이렇게 되면 매번 Annotation 관련 class 를 생성해서 사용할 수 있는 method를 쓰면, property 에 작업이 될거라서 나중에도 같은 방식으로 
          * 생성하여 까볼수 있음.
          */
        public static PropertyBuilder ForTiberoUseSequenceHiLo(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var property = propertyBuilder.Metadata;

            name = name ?? TiberoModelAnnotations.DefaultHiLoSequenceName;

            var model = property.DeclaringEntityType.Model;
            
            if (model.Tibero().FindSequence(name, schema) == null)
            {
                model.Tibero().GetOrAddSequence(name, schema).IncrementBy = 10;
            }

            GetTiberoInternalBuilder(propertyBuilder).ValueGenerationStrategy(TiberoValueGenerationStrategy.SequenceHiLo);

            property.Tibero().HiLoSequenceName = name;
            property.Tibero().HiLoSequenceSchema = schema;

            return propertyBuilder;
        }
        public static PropertyBuilder<TProperty> ForTiberoUseSequenceHiLo<TProperty>(
           [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
           [CanBeNull] string name = null,
           [CanBeNull] string schema = null)
           => (PropertyBuilder<TProperty>)ForTiberoUseSequenceHiLo((PropertyBuilder)propertyBuilder, name, schema);
        public static PropertyBuilder UseTiberoIdentityColumn(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            GetTiberoInternalBuilder(propertyBuilder).ValueGenerationStrategy(TiberoValueGenerationStrategy.IdentityColumn);

            return propertyBuilder;
        }

       
        public static PropertyBuilder<TProperty> UseTiberoIdentityColumn<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder)
            => (PropertyBuilder<TProperty>)UseTiberoIdentityColumn((PropertyBuilder)propertyBuilder);
        private static TiberoPropertyBuilderAnnotations GetTiberoInternalBuilder(PropertyBuilder propertyBuilder)
           => propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Tibero(ConfigurationSource.Explicit);
    }
}
