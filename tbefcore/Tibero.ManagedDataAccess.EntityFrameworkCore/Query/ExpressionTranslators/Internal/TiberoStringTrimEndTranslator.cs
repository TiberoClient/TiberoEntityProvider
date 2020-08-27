using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoStringTrimEndTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), Array.Empty<Type>());

        
        private static readonly MethodInfo _methodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char[]) });
        private static readonly MethodInfo _methodInfoWithCharacterArg
            = typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), new[] { typeof(char) });

        
        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
                        
            if (_methodInfoWithoutArgs?.Equals(methodCallExpression.Method) == true)
            {
                var sqlArguments = new[] { methodCallExpression.Object };

                return new SqlFunctionExpression("RTRIM", methodCallExpression.Type, sqlArguments);
            }
            else if (_methodInfoWithCharacterArg.Equals(methodCallExpression.Method))
            {
                var sqlArguments = new[] { methodCallExpression.Object, methodCallExpression.Arguments[0] };
               
                return new SqlFunctionExpression("RTRIM", methodCallExpression.Type, sqlArguments);
            }
            /* 아래는 char array 를 인자로 받느 trim에 대해 처리하려했는데, methodCallExpression에서 char array 인 경우 parameter값이 안넘어옴.
             * 애초에 MS에서는 해당 경우를 고려하지 않음. 일단 주석처리.
            else if (_methodInfoWithCharArrayArg.Equals(methodCallExpression.Method))
             {
                var c = (methodCallExpression.Arguments[0] as ConstantExpression)?.Value as Array;
                Console.WriteLine("cccccccccccc"+ c);
                if (methodCallExpression.Arguments[0]is ConstantExpression constantExpression)
                {
                    var characters = constantExpression.Value.ToString();
                    var expression = Expression.Constant(characters);
                    var sqlArguments = new[] { methodCallExpression.Object, expression };
                    return new SqlFunctionExpression("RTRIM", methodCallExpression.Type, sqlArguments);
                }
                
            }*/

            return null;
        }
    }
}
