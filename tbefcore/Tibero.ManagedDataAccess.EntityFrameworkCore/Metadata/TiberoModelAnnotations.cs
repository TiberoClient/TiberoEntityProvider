using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Metadata.Internal;

namespace Tibero.EntityFrameworkCore.Metadata
{
    public class TiberoModelAnnotations : RelationalModelAnnotations, ITiberoModelAnnotations
    {

        public const string DefaultHiLoSequenceName = "EFCoreSequenceHiLo";
        /*
         * 가장 기본이 되는 RelationalAnnotations 에 MetaData를 들고 있는데, 이 metadata는 IAnnotable 을 상속받음. 
         * 따라서 model이던 property 던 해당 내용을 annotation에 달고 싶으면 아래 생성자에서 base에 넘길 때,
         * 알아서 RelationalAnnotation을 만들어서 달아놓게됨. 
         */ 
        public TiberoModelAnnotations([NotNull] IModel model)
            : base(model)
        {

        }
        public TiberoModelAnnotations([NotNull] RelationalAnnotations annotations)
           : base(annotations)
        {

        }
        public virtual string HiLoSequenceName
        {
            get => (string)Annotations.Metadata[TiberoAnnotationNames.HiLoSequenceName];
            [param: CanBeNull]
            set => SetHiLoSequenceName(value);
        }
        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                TiberoAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));
        public virtual string HiLoSequenceSchema
        {
            get => (string)Annotations.Metadata[TiberoAnnotationNames.HiLoSequenceSchema];
            [param: CanBeNull]
            set => SetHiLoSequenceSchema(value);
        }
        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                TiberoAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(value, nameof(value)));
        public virtual TiberoValueGenerationStrategy? ValueGenerationStrategy
        {
            get => (TiberoValueGenerationStrategy?)Annotations.Metadata[TiberoAnnotationNames.ValueGenerationStrategy];

            set => SetValueGenerationStrategy(value);
        }
        protected virtual bool SetValueGenerationStrategy(TiberoValueGenerationStrategy? value)
            => Annotations.SetAnnotation(TiberoAnnotationNames.ValueGenerationStrategy, value);

    }
}
