using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.EntityFrameworkCore.Utilities;

namespace Tibero.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TiberoSequenceValueGeneratorState :HiLoValueGeneratorState
    {
        public TiberoSequenceValueGeneratorState([NotNull] ISequence sequence)
            : base(Check.NotNull(sequence, nameof(sequence)).IncrementBy)
        {
            Sequence = sequence;
        }
        public virtual ISequence Sequence { get; }
    }
}
