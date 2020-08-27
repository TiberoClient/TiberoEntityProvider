using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tibero.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Update.Internal;
using Tibero.EntityFrameworkCore.Extensions;

namespace Tibero.EntityFrameworkCore.ValueGeneration.Internal
{
    public class TiberoSequenceHiLoValueGenerator<TValue> : HiLoValueGenerator<TValue>
    {
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly ITiberoUpdateSqlGenerator _sqlGenerator;
        private readonly ITiberoRelationalConnection _connection;
        private readonly ISequence _sequence;
        public TiberoSequenceHiLoValueGenerator(
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] ITiberoUpdateSqlGenerator sqlGenerator,
            [NotNull] TiberoSequenceValueGeneratorState generatorState,
            [NotNull] ITiberoRelationalConnection connection)
            : base(generatorState)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));
            Check.NotNull(sqlGenerator, nameof(sqlGenerator));
            Check.NotNull(connection, nameof(connection));

            _sequence = generatorState.Sequence;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
            _sqlGenerator = sqlGenerator;
            _connection = connection;
        }
        /*
         * UpdateSqlGenerator 의 GenerateNextSequenceValueOperation 내에서는 AppendNextSequenceValueOperation을 호출하고 이 함수는 TiberoUpdateSqlGenerator에 구현되어 있음. 
         */
        protected override long GetNewLowValue()
            => (long)Convert.ChangeType(
                _rawSqlCommandBuilder
                    .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                    .ExecuteScalar(_connection),
                typeof(long),
                CultureInfo.InvariantCulture);
        protected override async Task<long> GetNewLowValueAsync(CancellationToken cancellationToken = default)
           => (long)Convert.ChangeType(
               await _rawSqlCommandBuilder
                   .Build(_sqlGenerator.GenerateNextSequenceValueOperation(_sequence.Name, _sequence.Schema))
                   .ExecuteScalarAsync(_connection, cancellationToken: cancellationToken),
               typeof(long),
               CultureInfo.InvariantCulture);
        public override bool GeneratesTemporaryValues => false;
    }
}
