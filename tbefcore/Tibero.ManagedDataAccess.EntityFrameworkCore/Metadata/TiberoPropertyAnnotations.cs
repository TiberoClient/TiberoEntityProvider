using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Extensions;
using Tibero.EntityFrameworkCore.Internal;

namespace Tibero.EntityFrameworkCore.Metadata
{
    public class TiberoPropertyAnnotations : RelationalPropertyAnnotations, ITiberoPropertyAnnotations
    {

        public TiberoPropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {

        }
        public TiberoPropertyAnnotations([NotNull] RelationalAnnotations annotations)
           : base(annotations)
        {

        }
        public virtual TiberoValueGenerationStrategy? ValueGenerationStrategy
        {
            get => GetTiberoValueGenerationStrategy(fallbackToModel: true);
            set => SetValueGenerationStrategy(value);
        }


        public virtual string HiLoSequenceName
        {
            /*
             * RelationalPropertyAnnotatioins 에 RelationalAnnotations 를 들고있음. 그게 Annotations. 그리고 그 Annotations에는 annotatable 한 Metatdata를 들고있음.
             */
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
        public virtual ISequence FindHiLoSequence()
        {
            // 아래는 property 가 속한 model 을 찾고 그 model의 annotations을 탐색하기 위해 ITiberoModelAnnotations을 가져오는 과정. 그리고 그 후 인터페이스를 통해 sequenceName을 찾음.
            var modelExtensions = Property.DeclaringEntityType.Model.Tibero();

            if (ValueGenerationStrategy != TiberoValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = HiLoSequenceName
                               ?? modelExtensions.HiLoSequenceName
                               ?? TiberoModelAnnotations.DefaultHiLoSequenceName;

            var sequenceSchema = HiLoSequenceSchema
                                 ?? modelExtensions.HiLoSequenceSchema;

            return modelExtensions.FindSequence(sequenceName, sequenceSchema);
        }
        public virtual TiberoValueGenerationStrategy? GetTiberoValueGenerationStrategy(bool fallbackToModel)
        {
            var annotation = Annotations.Metadata.FindAnnotation(TiberoAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (TiberoValueGenerationStrategy?)annotation.Value;
            }
            /* 여기서 Property 는 Annotations의 Metdata를 꺼내오는 get임
             * Relational 확장메서드는 해당 property 를 다시 relationalPropertyAnnotations 로 생성해서 가져오는 메서드. 
             */
            var relationalProperty = Property.Relational();
            if (!fallbackToModel
                || relationalProperty.DefaultValue != null
                || relationalProperty.DefaultValueSql != null
                || relationalProperty.ComputedColumnSql != null)
            {
                return null;
            }

            if (Property.ValueGenerated != ValueGenerated.OnAdd)
            {
                var sharedTablePrincipalPrimaryKeyProperty = Property.FindSharedTableRootPrimaryKeyProperty();
                if (sharedTablePrincipalPrimaryKeyProperty != null
                    && sharedTablePrincipalPrimaryKeyProperty.Tibero().ValueGenerationStrategy == TiberoValueGenerationStrategy.IdentityColumn)
                {
                    return TiberoValueGenerationStrategy.IdentityColumn;
                }

                return null;
            }

            var modelStrategy = Property.DeclaringEntityType.Model.Tibero().ValueGenerationStrategy;

            if (modelStrategy == TiberoValueGenerationStrategy.SequenceHiLo
                && IsCompatibleSequenceHiLo(Property))
            {
                return TiberoValueGenerationStrategy.SequenceHiLo;
            }

            if (modelStrategy == TiberoValueGenerationStrategy.IdentityColumn
                && IsCompatibleIdentityColumn(Property))
            {
                return TiberoValueGenerationStrategy.IdentityColumn;
            }

            return null;
        }

        
        protected virtual bool SetValueGenerationStrategy(TiberoValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = Property.ClrType;

                if (value == TiberoValueGenerationStrategy.IdentityColumn
                    && !IsCompatibleIdentityColumn(Property))
                {
                    if (ShouldThrowOnInvalidConfiguration)
                    {
                        throw new ArgumentException(
                            TiberoStrings.IdentityBadType(
                                Property.Name, Property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                    }

                    return false;
                }

                if (value == TiberoValueGenerationStrategy.SequenceHiLo
                    && !IsCompatibleSequenceHiLo(Property))
                {
                    if (ShouldThrowOnInvalidConfiguration)
                    {
                        throw new ArgumentException(
                            TiberoStrings.SequenceBadType(
                                Property.Name, Property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                    }

                    return false;
                }
            }

            if (!CanSetValueGenerationStrategy(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && ValueGenerationStrategy != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }
            
            var setSuccessfully = Annotations.SetAnnotation(TiberoAnnotationNames.ValueGenerationStrategy, value);
           
            return setSuccessfully;
        }

        /// <summary>
        ///     Checks whether or not it is valid to set the given <see cref="SqlServerValueGenerationStrategy" />
        ///     for the property.
        /// </summary>
        /// <param name="value"> The strategy to check. </param>
        /// <returns> <c>True</c> if it is valid to set; <c>false</c> otherwise. </returns>
        protected virtual bool CanSetValueGenerationStrategy(TiberoValueGenerationStrategy? value)
        {
            if (GetTiberoValueGenerationStrategy(fallbackToModel: false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(TiberoAnnotationNames.ValueGenerationStrategy, value))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValue(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValue)));
                }

                if (GetDefaultValueSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValueSql)));
                }

                if (GetComputedColumnSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(ComputedColumnSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValue(null)
                         || !CanSetDefaultValueSql(null)
                         || !CanSetComputedColumnSql(null)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Gets the default value set for the property.
        /// </summary>
        /// <param name="fallback">
        ///     If <c>true</c>, and some SQL Server specific
        ///     <see cref="ValueGenerationStrategy" /> has been set, then this method will always
        ///     return <c>null</c> because these strategies do not use default values.
        /// </param>
        /// <returns> The default value, or <c>null</c> if none has been set. </returns>
        protected override object GetDefaultValue(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValue(fallback);
        }

        /// <summary>
        ///     Checks whether or not it is valid to set a default value for the property.
        /// </summary>
        /// <param name="value"> The value to check. </param>
        /// <returns> <c>True</c> if it is valid to set this value; <c>false</c> otherwise. </returns>
        protected override bool CanSetDefaultValue(object value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValue), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValue(value);
        }

        /// <summary>
        ///     Gets the default SQL expression set for the property.
        /// </summary>
        /// <param name="fallback">
        ///     If <c>true</c>, and some SQL Server specific
        ///     <see cref="ValueGenerationStrategy" /> has been set, then this method will always
        ///     return <c>null</c> because these strategies do not use default expressions.
        /// </param>
        /// <returns> The default expression, or <c>null</c> if none has been set. </returns>
        protected override string GetDefaultValueSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValueSql(fallback);
        }

        /// <summary>
        ///     Checks whether or not it is valid to set a default SQL expression for the property.
        /// </summary>
        /// <param name="value"> The expression to check. </param>
        /// <returns> <c>True</c> if it is valid to set this expression; <c>false</c> otherwise. </returns>
        protected override bool CanSetDefaultValueSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValueSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValueSql(value);
        }

        /// <summary>
        ///     Gets the computed SQL expression set for the property.
        /// </summary>
        /// <param name="fallback">
        ///     If <c>true</c>, and some SQL Server specific
        ///     <see cref="ValueGenerationStrategy" /> has been set, then this method will always
        ///     return <c>null</c> because these strategies do not use computed expressions.
        /// </param>
        /// <returns> The computed expression, or <c>null</c> if none has been set. </returns>
        protected override string GetComputedColumnSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetComputedColumnSql(fallback);
        }

        /// <summary>
        ///     Checks whether or not it is valid to set a computed SQL expression for the property.
        /// </summary>
        /// <param name="value"> The expression to check. </param>
        /// <returns> <c>True</c> if it is valid to set this expression; <c>false</c> otherwise. </returns>
        protected override bool CanSetComputedColumnSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ComputedColumnSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetComputedColumnSql(value);
        }

        /// <summary>
        ///     Resets value-generation for the property to defaults.
        /// </summary>
        protected override void ClearAllServerGeneratedValues()
        {
            SetValueGenerationStrategy(null);

            base.ClearAllServerGeneratedValues();
        }

        private static bool IsCompatibleIdentityColumn(IProperty property)
        {
            var type = property.ClrType;

            return (type.IsInteger() || type == typeof(decimal)) && !HasConverter(property);
        }

        private static bool IsCompatibleSequenceHiLo(IProperty property)
            => property.ClrType.IsInteger() && !HasConverter(property);

        private static bool HasConverter(IProperty property)
            => (property.FindMapping()?.Converter
                ?? property.GetValueConverter()) != null;
    }
}

