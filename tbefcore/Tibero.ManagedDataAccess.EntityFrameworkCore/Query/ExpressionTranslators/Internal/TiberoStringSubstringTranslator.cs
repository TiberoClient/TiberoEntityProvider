using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoStringSubstringTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoSubStringMapping = new Dictionary<MethodInfo, string>
        {
            { typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int) }), "substring(int)" },
            { typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) }), "substring(int, int)" }

        };
        private static readonly MethodInfo _methodInfo
           = typeof(string).GetRuntimeMethod(nameof(string.Substring), new[] { typeof(int), typeof(int) });
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (_methodInfoSubStringMapping.TryGetValue(methodCallExpression.Method, out var subStringPart))
            {
                if (subStringPart.Equals("substring(int, int)"))
                {
                    return new SqlFunctionExpression(
                         "SUBSTR",
                         methodCallExpression.Type,
                         new[]
                         {
                        methodCallExpression.Object,

                        methodCallExpression.Arguments[0] is ConstantExpression constantExpression1
                            && constantExpression1.Value is int value1
                                ? (Expression)Expression.Constant(value1 + 1)
                                : Expression.Add(
                                    methodCallExpression.Arguments[0],
                                    Expression.Constant(1)),
                        methodCallExpression.Arguments[1]
                         });
                }


                return new SqlFunctionExpression(
                     "SUBSTR",
                     methodCallExpression.Type,
                     new[]
                     {
                        methodCallExpression.Object,

                        methodCallExpression.Arguments[0] is ConstantExpression constantExpression
                            && constantExpression.Value is int value
                                ? (Expression)Expression.Constant(value + 1)
                                : Expression.Add(
                                    methodCallExpression.Arguments[0],
                                    Expression.Constant(1))
                        
                     });


            }
            else
            {
                return null;
            }
        }

    }
}
