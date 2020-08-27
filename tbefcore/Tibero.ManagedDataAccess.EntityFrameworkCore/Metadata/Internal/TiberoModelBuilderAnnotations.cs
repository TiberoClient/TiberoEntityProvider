using System;
using System.Collections.Generic;
using System.Text;
using Tibero.EntityFrameworkCore.Metadata;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Tibero.EntityFrameworkCore.Metadata.Internal
{
    public class TiberoModelBuilderAnnotations : TiberoModelAnnotations
    {
        public TiberoModelBuilderAnnotations(
            [NotNull] InternalModelBuilder internalBuilder,
            ConfigurationSource configurationSource)
            :base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {

        }
        public new virtual bool HiLoSequenceName([CanBeNull] string value) => SetHiLoSequenceName(value);
        public new virtual bool HiLoSequenceSchema([CanBeNull] string value) => SetHiLoSequenceSchema(value);
        public new virtual bool ValueGenerationStrategy(TiberoValueGenerationStrategy? value) => SetValueGenerationStrategy(value);
    }
}
