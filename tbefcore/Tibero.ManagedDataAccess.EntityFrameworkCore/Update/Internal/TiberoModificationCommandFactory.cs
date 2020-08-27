using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tibero.EntityFrameworkCore.Update.Internal
{
    public class TiberoModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        readonly ISqlGenerationHelper _sqlGenerationHelper;
        readonly IUpdateSqlGenerator _updateSqlGenerator;
        readonly IRelationalValueBufferFactoryFactory _valueBufferFactoryFactory;
        readonly IDbContextOptions _options;
        readonly IDiagnosticsLogger<DbLoggerCategory.Update> _logger;

        public TiberoModificationCommandBatchFactory(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IDbContextOptions options,
            IDiagnosticsLogger<DbLoggerCategory.Update> logger = null)
        {
           
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(updateSqlGenerator, nameof(updateSqlGenerator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(options, nameof(options));

            _commandBuilderFactory = commandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
            _updateSqlGenerator = updateSqlGenerator;
            _valueBufferFactoryFactory = valueBufferFactoryFactory;
            _options = options;
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatchFatcory::Constructor");
            }
            
        }

        public virtual ModificationCommandBatch Create()
        {
            var optionsExtension = _options.Extensions.OfType<TiberoOptionsExtension>().FirstOrDefault();
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatchFatcory::Create");
            }
            return new TiberoModificationCommandBatch(
                _commandBuilderFactory,
                _sqlGenerationHelper,
                _updateSqlGenerator,
                _valueBufferFactoryFactory,
                optionsExtension?.MaxBatchSize,
                _logger);
        }
    }
}