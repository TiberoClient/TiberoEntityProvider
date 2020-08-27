using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoConvertTranslator : IMethodCallTranslator
    {
        private static readonly Dictionary<string, string> _typeMapping = new Dictionary<string, string>
        {
            [nameof(Convert.ToByte)] = "NUMBER(3)",
            [nameof(Convert.ToDecimal)] = "NUMBER",
            [nameof(Convert.ToDouble)] = "NUMBER",
            [nameof(Convert.ToInt16)] = "NUMBER(6)",
            [nameof(Convert.ToInt32)] = "NUMBER(10)",
            [nameof(Convert.ToInt64)] = "NUMBER(19)",
            [nameof(Convert.ToString)] = "NVARCHAR(2000)"
        };

        private static readonly List<Type> _supportedTypes = new List<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(string)
        };

        private static readonly IEnumerable<MethodInfo> _supportedMethods
            = _typeMapping.Keys
                .SelectMany(
                    t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                        .Where(
                            m => m.GetParameters().Length == 1
                                 && _supportedTypes.Contains(m.GetParameters().First().ParameterType)));

    
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            /*
             * Method call 에서 어떤 method 인지 나온다. 마치 함수처럼 반환타입 함수명(인자) 처럼 표현되는데 그 표현식이 _supportedMethods 에 있으면 유효성 통과.
             */ 
            return _supportedMethods.Contains(methodCallExpression.Method)
               ? new SqlFunctionExpression(
                   "CAST",
                   methodCallExpression.Type,
                   new[]
                   { methodCallExpression.Arguments[0],
                        new SqlFragmentExpression(
                            _typeMapping[methodCallExpression.Method.Name])
                   })
               : null;
        }
    }
}
