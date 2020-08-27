using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoStringIndexOfTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.IndexOf), new[] { typeof(string) });

     
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var patternExpression = methodCallExpression.Arguments[0];

                var charIndexExpression = Expression.Subtract(
                    new SqlFunctionExpression(
                        "INSTR",
                        typeof(int),
                        new[] { methodCallExpression.Object, patternExpression }),
                    Expression.Constant(1));

                return patternExpression is ConstantExpression constantExpression
                    && !string.IsNullOrEmpty((string)constantExpression.Value)
                        ? (Expression)charIndexExpression
                        : Expression.Condition(
                            Expression.Equal(patternExpression, Expression.Constant(string.Empty)),
                            Expression.Constant(0),
                            charIndexExpression);
            }

            return null;
        }
    }
}
