using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoDateAddTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new Dictionary<MethodInfo, string>
        {
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), new[] { typeof(int) }), "year" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), new[] { typeof(int) }), "month" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), new[] { typeof(double) }), "day" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), new[] { typeof(double) }), "hour" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), new[] { typeof(double) }), "minute" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), new[] { typeof(double) }), "second" },
            { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), new[] { typeof(double) }), "millisecond" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), new[] { typeof(int) }), "year" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), new[] { typeof(int) }), "month" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), new[] { typeof(double) }), "day" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), new[] { typeof(double) }), "hour" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), new[] { typeof(double) }), "minute" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), new[] { typeof(double) }), "second" },
            { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), new[] { typeof(double) }), "millisecond" }
        };
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (_methodInfoDatePartMapping.TryGetValue(methodCallExpression.Method, out var datePart))
            {
                var amountToAdd = methodCallExpression.Arguments.First();
                /*
                if (!datePart.Equals("year")
                    && !datePart.Equals("month")
                    && amountToAdd is ConstantExpression constantExpression
                    && ((double)constantExpression.Value >= int.MaxValue
                        || (double)constantExpression.Value <= int.MinValue))
                {
                    return null;
                }
                */
                if (datePart.Equals("year"))
                {
                    return new SqlFunctionExpression(
                        functionName: "ADD_MONTHS",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                        methodCallExpression.Object,
                        Expression.Multiply(amountToAdd, Expression.Constant(12))
                        });
                }
                else if (datePart.Equals("month"))
                {
                    return new SqlFunctionExpression(
                        functionName: "ADD_MONTHS",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                        methodCallExpression.Object,
                        amountToAdd
                        });
                }
                else if (datePart.Equals("day"))
                {
                    return new SqlFunctionExpression(
                        functionName: "ADD_DATETIME",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                         methodCallExpression.Object,
                         new SqlFragmentExpression(amountToAdd.ToString())
                        });
                }
                else if (datePart.Equals("hour"))
                {
                    return new SqlFunctionExpression(
                        functionName: "ADD_DATETIME",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                        methodCallExpression.Object,
                         new SqlFragmentExpression(amountToAdd.ToString() + "/24")
                        });
                }
                else if (datePart.Equals("minute"))
                {
                    return new SqlFunctionExpression(
                        functionName: "ADD_DATETIME",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                        methodCallExpression.Object,
                         new SqlFragmentExpression(amountToAdd.ToString() + "/1440")
                        });
                }
                else if (datePart.Equals("second"))
                {
                    return new SqlFunctionExpression(
                        functionName: "ADD_DATETIME",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                        methodCallExpression.Object,
                         new SqlFragmentExpression(amountToAdd.ToString() + "/86400")
                        });
                }
                /* 위에 까지는 그냥 datetime + 숫자 이렇게 해도정상동작함. 근데 millisecond 부터는 안되는듯.. */
                else if (datePart.Equals("millisecond"))
                {
                    if(typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), new[] { typeof(double) }) == methodCallExpression.Method)
                        throw new NotImplementedException("DateTimeOffset.AddMilliSeconds Cannot be used ");
                    return new SqlFunctionExpression(
                        functionName: "NUMTODSINTERVAL",
                        returnType: methodCallExpression.Type,
                        arguments: new[]
                        {
                        methodCallExpression.Object,
                        new SqlFunctionExpression("NUMTODSINTERVAL",methodCallExpression.Type, new[]{
                            new SqlFragmentExpression(amountToAdd.ToString() + "/1000"),
                            new SqlFragmentExpression("'SECOND'")
                        })
                        });
                }
            }

            return null;
        }
    }
}
