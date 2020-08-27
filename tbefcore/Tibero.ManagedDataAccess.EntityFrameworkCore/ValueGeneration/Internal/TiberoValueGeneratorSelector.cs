using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
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
    public class TiberoValueGeneratorSelector : RelationalValueGeneratorSelector
    {
        private readonly ITiberoSequenceValueGeneratorFactory _sequenceFactory;

        private readonly ITiberoRelationalConnection _connection;
        public TiberoValueGeneratorSelector(
            [NotNull] ValueGeneratorSelectorDependencies dependencies,
            [NotNull] ITiberoSequenceValueGeneratorFactory sequenceFactory,
            [NotNull] ITiberoRelationalConnection connection)
            : base(dependencies)
        {
            Check.NotNull(sequenceFactory, nameof(sequenceFactory));
            Check.NotNull(connection, nameof(connection));

            _sequenceFactory = sequenceFactory;
            _connection = connection;
        }
        public new virtual ITiberoValueGeneratorCache Cache => (ITiberoValueGeneratorCache)base.Cache;
        public override ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.GetValueGeneratorFactory() == null
                   && property.Tibero().ValueGenerationStrategy == TiberoValueGenerationStrategy.SequenceHiLo
                ? _sequenceFactory.Create(property, Cache.GetOrAddSequenceState(property), _connection)
                : base.Select(property, entityType);
        }
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType() == typeof(Guid)
                ? property.ValueGenerated == ValueGenerated.Never
                  || property.Tibero().DefaultValueSql != null
                    ? (ValueGenerator)new TemporaryGuidValueGenerator()
                    : new SequentialGuidValueGenerator()
                : base.Create(property, entityType);
        }
    }
}
