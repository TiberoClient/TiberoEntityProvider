using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Tibero.EntityFrameworkCore.Utilities;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tibero.EntityFrameworkCore.Query.Sql.Internal
{
    public class TiberoQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        readonly ITiberoOptions _TiberoOptions;
        readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        public TiberoQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] ITiberoOptions TiberoOptions, 
            IDiagnosticsLogger<DbLoggerCategory.Query> logger = null)
            : base(dependencies)
        {
            _TiberoOptions = TiberoOptions;
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGeneratorFactory::Constructor");
            }
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new TiberoQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)),
                _logger);
    }
}
