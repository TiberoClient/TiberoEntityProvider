using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoContainsOptimizedTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), new[] { typeof(string) });

        
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var patternExpression = methodCallExpression.Arguments[0];

                var charIndexExpression = Expression.GreaterThan(
                    new SqlFunctionExpression(
                        "INSTR",
                        typeof(int),
                        new[] { methodCallExpression.Object, patternExpression }),
                    Expression.Constant(0));

                return patternExpression is ConstantExpression patternConstantExpression
                    ? ((string)patternConstantExpression.Value)?.Length == 0
                        ? (Expression)Expression.Constant(true)
                        : charIndexExpression
                    : Expression.OrElse(
                        charIndexExpression,
                        Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
            }

            return null;
        }
    }
}
