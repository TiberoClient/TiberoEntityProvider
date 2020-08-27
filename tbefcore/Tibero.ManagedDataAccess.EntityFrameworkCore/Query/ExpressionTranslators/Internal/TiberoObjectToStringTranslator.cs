using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoObjectToStringTranslator : IMethodCallTranslator
    {
        private const int DefaultLength = 100;

        private static readonly Dictionary<Type, string> _typeMapping
            = new Dictionary<Type, string>
            {
                { typeof(int), "VARCHAR(11)" },
                { typeof(long), "VARCHAR(20)" },
                { typeof(DateTime), $"VARCHAR({DefaultLength})" },
                { typeof(Guid), "VARCHAR(36)" },
                { typeof(bool), "VARCHAR(5)" },
                { typeof(byte), "VARCHAR(3)" },
                { typeof(byte[]), $"VARCHAR({DefaultLength})" },
                { typeof(double), $"VARCHAR({DefaultLength})" },
                { typeof(DateTimeOffset), $"VARCHAR({DefaultLength})" },
                { typeof(char), "VARCHAR(1)" },
                { typeof(short), "VARCHAR(6)" },
                { typeof(float), $"VARCHAR({DefaultLength})" },
                { typeof(decimal), $"VARCHAR({DefaultLength})" },
                { typeof(TimeSpan), $"VARCHAR({DefaultLength})" },
                { typeof(uint), "VARCHAR(10)" },
                { typeof(ushort), "VARCHAR(5)" },
                { typeof(ulong), "VARCHAR(19)" },
                { typeof(sbyte), "VARCHAR(4)" }
            };

       
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == nameof(ToString)
                && methodCallExpression.Arguments.Count == 0
                && methodCallExpression.Object != null
                && _typeMapping.TryGetValue(
                    methodCallExpression.Object.Type
                        .UnwrapNullableType(),
                    out var storeType))
            {
                 return new SqlFunctionExpression(
                     functionName: "CAST",
                     returnType: methodCallExpression.Type,
                     arguments: new[]
                     {
                         methodCallExpression.Object,
                         new SqlFragmentExpression(storeType)
                     });
               
            }

            return null;
        }
    }
}
