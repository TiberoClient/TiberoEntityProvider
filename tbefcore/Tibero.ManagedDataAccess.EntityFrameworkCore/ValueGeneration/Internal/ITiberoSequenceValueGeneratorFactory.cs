
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
namespace Tibero.EntityFrameworkCore.ValueGeneration.Internal
{
    public interface ITiberoSequenceValueGeneratorFactory 
    {
        ValueGenerator Create(
            [NotNull] IProperty property,
            [NotNull] TiberoSequenceValueGeneratorState generatorState,
            [NotNull] ITiberoRelationalConnection connection);
    }
}
