using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
//using Tibero.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Tibero.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Tibero.EntityFrameworkCore.Query.Sql.Internal
{
    public class TiberoQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        readonly IDiagnosticsLogger<DbLoggerCategory.Query> _logger;
        private static readonly HashSet<string> _builtInFunctions = new HashSet<string>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
    {
      "MAX",
      "MIN",
      "SUM",
      "SUBSTR",
      "INSTR",
      "LENGTH",
      "COUNT",
      "TRIM",
      "RTRIM",
      "LTRIM",
      "REPLACE",
      "UPPER",
      "LOWER",
      "SYS_GUID",
      "LOG",
      "ABS",
      "CEIL",
      "FLOOR",
      "POWER",
      "EXP",
      "SQRT",
      "ACOS",
      "ASIN",
      "ATAN",
      "ATAN2",
      "COS",
      "SIN",
      "TAN",
      "SIGN",
      "ROUND"
    };

        protected override string TypedTrueLiteral { get; } = "TRUE::bool";

        protected override string TypedFalseLiteral { get; } = "FALSE::bool";

        public TiberoQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger = null)
            : base(dependencies, selectExpression)
        {
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::Constructor");
            }
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::GenerateTop");
                _logger.Logger.LogDebug("TiberoQuerySqlGenerator::GenerateTop : [" + Sql.ToString() + "]");
            }
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::GenerateLimitOffset");
            }
            /* 아래는 Tibero 차용 */
            if (selectExpression.Limit != null && selectExpression.Offset == null)
            {
                Sql.AppendLine().Append((object)"FETCH FIRST ");
                Visit(selectExpression.Limit);
                Sql.Append((object)" ROWS ONLY");
            }
            else
                base.GenerateLimitOffset(selectExpression);
        }
        protected override void GenerateOrderBy(IReadOnlyList<Ordering> orderings)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::GenerateOrderBy");
            }
            /* ordering할 content를 확인해서 base.GenerateOrderBy 에 넘긴다. */
            orderings = (IReadOnlyList<Ordering>)orderings.Where<Ordering>((Func<Ordering, bool>)(o => o.Expression.NodeType != ExpressionType.Constant && o.Expression.NodeType != ExpressionType.Parameter)).ToList<Ordering>();
            if (orderings.Count <= 0)
                return;
            base.GenerateOrderBy(orderings);
        }

        protected override void GenerateOrdering(Ordering ordering)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::GenereateOrdering");
            }
            /* 그리고 여기는 ordering 할 content 가 한개 씩 들어오는데,
             * NodeType을 비교해서 OrderBy 할지 말지 결정.
             * 결국 base를 타고, ASC 에서만 NULL FIRST 를 박아준다. -> 알아봐야함.
             */
            Check.NotNull<Ordering>(ordering, nameof(ordering));

            Expression expression = ordering.Expression;

            if (expression.NodeType == ExpressionType.Constant || expression.NodeType == ExpressionType.Parameter)
                return;

            base.GenerateOrdering(ordering);

            if (ordering.OrderingDirection != OrderingDirection.Asc)
                return;

            this.Sql.Append((object)" NULLS FIRST");
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull<BinaryExpression>(binaryExpression, nameof(binaryExpression));
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::VisitBinary");
            }

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.And:
                    this.Sql.Append((object)"BITAND(");
                    this.Visit(binaryExpression.Left);
                    this.Sql.Append((object)", ");
                    this.Visit(binaryExpression.Right);
                    this.Sql.Append((object)")");
                    return (Expression)binaryExpression;

                case ExpressionType.Modulo:
                    this.Sql.Append((object)"MOD(");
                    this.Visit(binaryExpression.Left);
                    this.Sql.Append((object)", ");
                    this.Visit(binaryExpression.Right);
                    this.Sql.Append((object)")");
                    return (Expression)binaryExpression;

                case ExpressionType.Or:
                    this.Visit(binaryExpression.Left);
                    this.Sql.Append((object)" - BITAND(");
                    this.Visit(binaryExpression.Left);
                    this.Sql.Append((object)", ");
                    this.Visit(binaryExpression.Right);
                    this.Sql.Append((object)") + ");
                    this.Visit(binaryExpression.Right);
                    return (Expression)binaryExpression;

                default:
                    if (binaryExpression.Right is ConstantExpression && (binaryExpression.Right as ConstantExpression).Value is string && string.IsNullOrEmpty((binaryExpression.Right as ConstantExpression).Value as string) && (binaryExpression.NodeType == ExpressionType.Equal || binaryExpression.NodeType == ExpressionType.NotEqual))
                    {
                        this.Visit(binaryExpression.Left);
                        if (binaryExpression.NodeType == ExpressionType.Equal)
                            this.Sql.Append((object)" IS NULL ");
                        else if (binaryExpression.NodeType == ExpressionType.NotEqual)
                            this.Sql.Append((object)" IS NOT NULL ");

                        return (Expression)binaryExpression;
                    }
                    Expression expression = base.VisitBinary(binaryExpression);

                    return expression;
            }
        }

        public override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::VisitFromSql");
            }
            /* From으로 감싸져야할 SQL 이 들어오는 경우 타는 visit 함수이다. 
             * sql이 적혀야할 indent 를 증가시키고 () 로 묶은 뒤 alias 를 걸어준다.
             */
            Check.NotNull<FromSqlExpression>(fromSqlExpression, nameof(fromSqlExpression));

            this.Sql.AppendLine((object)"(");

            using (this.Sql.Indent())
                this.GenerateFromSql(fromSqlExpression.Sql, fromSqlExpression.Arguments, this.ParameterValues);

            this.Sql.Append((object)") ").Append((object)this.SqlGenerator.DelimitIdentifier(fromSqlExpression.Alias));

            return (Expression)fromSqlExpression;
        }
        protected override void GeneratePseudoFromClause()
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::GeneratePseudoFromClause");
            }
            /* Generates a pseudo FROM clause.Required by some providers when a query has no
             * actual FROM clause.
             */

            this.Sql.Append((object)" FROM DUAL");
        }
        public override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::VisitSqlFragment");
            }
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));
            //sqlFragmentExpression.Type.
            base.VisitSqlFragment(sqlFragmentExpression);

            return sqlFragmentExpression;
        }
        protected override void GeneratePredicate([NotNull] Expression predicate)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::GeneratePredicate");
            }
            base.GeneratePredicate(predicate);
        }

        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoQuerySqlGenerator::VisitSqlFunction");
            }
            switch (sqlFunctionExpression.FunctionName)
            {
                case "EXTRACT":
                    this.Sql.Append((object)sqlFunctionExpression.FunctionName);
                    this.Sql.Append((object)"(");
                    this.Visit(sqlFunctionExpression.Arguments[0]);
                    this.Sql.Append((object)" FROM ");
                    this.Visit(sqlFunctionExpression.Arguments[1]);
                    this.Sql.Append((object)")");
                    return (Expression)sqlFunctionExpression;
                case "CAST":
                    this.Sql.Append((object)sqlFunctionExpression.FunctionName);
                    this.Sql.Append((object)"(");
                    this.Visit(sqlFunctionExpression.Arguments[0]);
                    this.Sql.Append((object)" AS ");
                    this.Visit(sqlFunctionExpression.Arguments[1]);
                    this.Sql.Append((object)")");
                    return (Expression)sqlFunctionExpression;
                case "AVG":
                    if (sqlFunctionExpression.Type == typeof(Decimal))
                        break;
                    goto default;
                case "SUM":
                    if (!(sqlFunctionExpression.Type == typeof(Decimal)))
                        goto default;
                    else
                        break;
                case "INSTR":
                    object obj;
                    if (sqlFunctionExpression.Arguments[1] is ParameterExpression parameterExpression && this.ParameterValues.TryGetValue(parameterExpression.Name, out obj))
                    {
                        string str = (string)obj;
                        if ((str != null ? (str.Length == 0 ? 1 : 0) : 0) != 0)
                            return this.Visit((Expression)Expression.Constant((object)1));
                        goto default;
                    }
                    else
                        goto default;
                case "ADD_MONTHS":
                    this.Sql.Append((object)"CAST(");
                    base.VisitSqlFunction(sqlFunctionExpression);
                    this.Sql.Append((object)" AS TIMESTAMP)");
                    return (Expression)sqlFunctionExpression;
                case "ADD_DATETIME":
                    this.Sql.Append((object)"CAST(");
                    Visit(sqlFunctionExpression.Arguments[0]);
                    this.Sql.Append(" + ");
                    this.Sql.Append(((SqlFragmentExpression)(sqlFunctionExpression.Arguments[1])).Sql);
                    this.Sql.Append((object)" AS TIMESTAMP)");
                    return (Expression)sqlFunctionExpression;
                case "NUMTODSINTERVAL":
                    this.Sql.Append("CAST(");
                    Visit(sqlFunctionExpression.Arguments[0]);
                    this.Sql.Append(" + ");
                    base.VisitSqlFunction((SqlFunctionExpression)sqlFunctionExpression.Arguments[1]);
                    this.Sql.Append((object)" AS TIMESTAMP)");

                    return (Expression)sqlFunctionExpression;
                case "TRUNC":
                    this.Sql.Append((object)sqlFunctionExpression.FunctionName);
                    this.Sql.Append((object)"(");
                    this.Visit(sqlFunctionExpression.Arguments[0]);

                    this.Sql.Append((object)")");
                    return (Expression)sqlFunctionExpression;
                default:
                    return base.VisitSqlFunction(TiberoQuerySqlGenerator._builtInFunctions.Contains(sqlFunctionExpression.FunctionName) || sqlFunctionExpression.Instance != null ?
                        sqlFunctionExpression :
                        new SqlFunctionExpression(this.SqlGenerator.DelimitIdentifier(sqlFunctionExpression.FunctionName), sqlFunctionExpression.Type, (string)null, (IEnumerable<Expression>)sqlFunctionExpression.Arguments));
            }

            this.Sql.Append((object)"CAST(");
            base.VisitSqlFunction(sqlFunctionExpression);
            this.Sql.Append((object)" AS NUMBER(29,4))");
            return (Expression)sqlFunctionExpression;

        }
    }
}
