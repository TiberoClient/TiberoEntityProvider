using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Metadata;
namespace Tibero.EntityFrameworkCore.Metadata.Internal
{
   /*
    * propertybuilder는 property 에 대한 항목들을 build하는 class 이다. 
    */ 
    
    public class TiberoPropertyBuilderAnnotations : TiberoPropertyAnnotations
    {
      
        public TiberoPropertyBuilderAnnotations(
            [NotNull] InternalPropertyBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new RelationalAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        private InternalPropertyBuilder PropertyBuilder => ((Property)Property).Builder;

      
        protected new virtual RelationalAnnotationsBuilder Annotations => (RelationalAnnotationsBuilder)base.Annotations;

      
        protected override bool ShouldThrowOnConflict => false;

       
        protected override bool ShouldThrowOnInvalidConfiguration => Annotations.ConfigurationSource == ConfigurationSource.Explicit;

#pragma warning disable 109
     
        public new virtual bool ColumnName([CanBeNull] string value) => SetColumnName(value);

    
        public new virtual bool ColumnType([CanBeNull] string value) => SetColumnType(value);

        public new virtual bool DefaultValueSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            return SetDefaultValueSql(value);
        }

        public new virtual bool ComputedColumnSql([CanBeNull] string value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention);
            return SetComputedColumnSql(value);
        }

        public new virtual bool DefaultValue([CanBeNull] object value)
        {
            PropertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            return SetDefaultValue(value);
        }
        
        public new virtual bool HiLoSequenceName([CanBeNull] string value) => SetHiLoSequenceName(value);

    
        public new virtual bool HiLoSequenceSchema([CanBeNull] string value) => SetHiLoSequenceSchema(value);

       
        public new virtual bool ValueGenerationStrategy(TiberoValueGenerationStrategy? value)
        {
            if (!SetValueGenerationStrategy(value))
            {
                return false;
            }

            if (value == null)
            {
                HiLoSequenceName(null);
                HiLoSequenceSchema(null);
            }
            else if (value.Value == TiberoValueGenerationStrategy.IdentityColumn)
            {
                HiLoSequenceName(null);
                HiLoSequenceSchema(null);
            }

            return true;
        }
#pragma warning restore 109
    }
    
}
