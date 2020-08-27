using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoEndsWithOptimizedTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
           = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) });

       
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var patternExpression = methodCallExpression.Arguments[0];

                var endsWithExpression = new NullCompensatedExpression(
                    Expression.Equal(
                        new SqlFunctionExpression(
                            "SUBSTR",
                       
                            methodCallExpression.Object.Type,
                            new[]
                            {
                                methodCallExpression.Object,
                                Expression.Negate(new SqlFunctionExpression("LENGTH", typeof(int), new[] { patternExpression }))
                            }),
                        patternExpression));

                return patternExpression is ConstantExpression patternConstantExpression
                    ? ((string)patternConstantExpression.Value)?.Length == 0
                        ? (Expression)Expression.Constant(true)
                        : endsWithExpression
                    : Expression.OrElse(
                        endsWithExpression,
                        Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
            }

            return null;
        }
    }
}
