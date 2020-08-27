using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoStringIsNullOrWhiteSpaceTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
           = typeof(string).GetRuntimeMethod(nameof(string.IsNullOrWhiteSpace), new[] { typeof(string) });

       
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Equals(_methodInfo))
            {
                var argument = methodCallExpression.Arguments[0];

                return 
                    Expression.OrElse(
                    new IsNullExpression(argument),
                    Expression.Equal(
                        new SqlFunctionExpression(
                            "TRIM",
                            typeof(string),
                            new[]
                            {
                              argument 
                            }),
                        Expression.Constant("", typeof(string))));
            }

            return null;
        }
    }
}
