using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using System;
using Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            /*
            new NpgsqlArraySequenceEqualTranslator(),*/
            new TiberoConvertTranslator(),
            new TiberoDateAddTranslator(),
            new TiberoContainsOptimizedTranslator(),
            new TiberoEndsWithOptimizedTranslator(),
            new TiberoStartsWithOptimizedTranslator(),
            new TiberoObjectToStringTranslator(),
            new TiberoStringIndexOfTranslator(),
            new TiberoStringIsNullOrWhiteSpaceTranslator(),
            new TiberoStringReplaceTranslator(),
            new TiberoStringSubstringTranslator(),
            new TiberoStringToLowerTranslator(),
            new TiberoStringToUpperTranslator(),
            new TiberoStringTrimEndTranslator(),
            new TiberoStringTrimStartTranslator(),
            new TiberoStringTrimTranslator(),
            new TiberoMathTranslator()
            //new TiberoNewGuidTranslator()

        };

        public TiberoCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies,
            [NotNull] ITiberoOptions npgsqlOptions)
            : base(dependencies)
        {
           
            /*
            var instanceTranslators =
                new IMethodCallTranslator[]
                {
                    new NpgsqlDateAddTranslator(npgsqlOptions.PostgresVersion)
                };
                */
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            //AddTranslators(instanceTranslators);

            /*
            foreach (var plugin in npgsqlOptions.Plugins)
                plugin.AddMethodCallTranslators(this);
                */
        }

        /// <summary>
        /// Adds additional dispatches to the translators list.
        /// </summary>
        /// <param name="translators">The translators.</param>
        public new virtual void AddTranslators([NotNull] [ItemNotNull] IEnumerable<IMethodCallTranslator> translators)
            => base.AddTranslators(translators);
    }
}
