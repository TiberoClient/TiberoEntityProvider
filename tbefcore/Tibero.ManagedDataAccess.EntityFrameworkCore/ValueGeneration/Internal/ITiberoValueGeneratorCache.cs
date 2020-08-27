using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Tibero.EntityFrameworkCore.ValueGeneration.Internal
{
    public interface ITiberoValueGeneratorCache : IValueGeneratorCache
    {
        TiberoSequenceValueGeneratorState GetOrAddSequenceState([NotNull] IProperty property);
    }
}
