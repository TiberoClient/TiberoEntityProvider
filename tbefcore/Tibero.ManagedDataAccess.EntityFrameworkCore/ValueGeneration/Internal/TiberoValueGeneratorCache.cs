using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tibero.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Extensions;


namespace Tibero.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TiberoValueGeneratorCache : ValueGeneratorCache, ITiberoValueGeneratorCache
    {
        private readonly ConcurrentDictionary<string, TiberoSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ConcurrentDictionary<string, TiberoSequenceValueGeneratorState>();
        public TiberoValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies)
           : base(dependencies)
        {
        }
        public virtual TiberoSequenceValueGeneratorState GetOrAddSequenceState(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var sequence = property.Tibero().FindHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence),
                sequenceName => new TiberoSequenceValueGeneratorState(sequence));
        }

        private static string GetSequenceName(ISequence sequence)
            => (sequence.Schema == null ? "" : sequence.Schema + ".") + sequence.Name;
    }
}
