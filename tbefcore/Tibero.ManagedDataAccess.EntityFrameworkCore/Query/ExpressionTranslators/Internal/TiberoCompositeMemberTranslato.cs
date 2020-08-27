using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using System;
using Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal;

namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        public TiberoCompositeMemberTranslator(
            [NotNull] RelationalCompositeMemberTranslatorDependencies dependencies,
            [NotNull] ITiberoOptions TiberoOptions)
            : base(dependencies)
        {
           
            
            AddTranslators(new List<IMemberTranslator>
            {
                new TiberoStringLengthTranslator(),
                new TiberoDateTimeMemberTranslator()
            });

            
        }

        public new virtual void AddTranslators([NotNull] IEnumerable<IMemberTranslator> translators)
            => base.AddTranslators(translators);
    }
}