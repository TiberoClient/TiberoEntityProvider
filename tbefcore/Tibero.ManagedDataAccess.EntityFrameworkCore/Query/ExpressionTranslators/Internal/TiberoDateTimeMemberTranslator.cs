using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Tibero.DataAccess.Types;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TiberoDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new Dictionary<string, string>
            {
                { nameof(DateTime.Year), "YEAR" },
                { nameof(DateTime.Month), "MONTH" },
                { nameof(DateTime.Day), "DAY" },
                { nameof(DateTime.Hour), "HOUR" },
                { nameof(DateTime.Minute), "MINUTES" },
                { nameof(DateTime.Second), "SECOND" },
                { nameof(DateTime.Millisecond), "millisecond" },
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression Translate(MemberExpression memberExpression)
        {
            /*
             * linq query 등에서 등장하는 datetime의 특정 구문에 대한 translator이다. 
             */ 
            var declaringType = memberExpression.Member.DeclaringType;
            if (declaringType == typeof(DateTime)
                || declaringType == typeof(DateTimeOffset))
            {
                var memberName = memberExpression.Member.Name;
                /* datetime 의 특정부분을 나타내려면 extract function 사용해야한다. */
                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    
                    return new SqlFunctionExpression(
                        "EXTRACT",
                        memberExpression.Type,
                        arguments: new[] { new SqlFragmentExpression(datePart), memberExpression.Expression });
                }

                switch (memberName)
                {
                   
                    case nameof(DateTime.Now):
                
                        //throw new InvalidOperationException("");
                        return declaringType == typeof(DateTimeOffset) ?
                                    new ExplicitCastExpression(new SqlFragmentExpression("SYSDATE"), typeof(DateTimeOffset)) :
                                    new ExplicitCastExpression(new SqlFragmentExpression("SYSDATE"), typeof(DateTime));
                            

                    case nameof(DateTime.UtcNow):
                        return declaringType == typeof(DateTimeOffset) ?
                                   new ExplicitCastExpression(new SqlFragmentExpression("SYSTIMESTAMP"), typeof(DateTimeOffset)) :
                                   new ExplicitCastExpression(new SqlFragmentExpression("SYSTIMESTAMP"), typeof(DateTime));

                    case nameof(DateTime.Date):
                        return new SqlFunctionExpression(
                            "TRUNC",
                            memberExpression.Type,
                            new[] { memberExpression.Expression });

                    case nameof(DateTime.Today):
                        return new SqlFunctionExpression(
                            "TRUNC",
                            memberExpression.Type,
                            new Expression[]
                            {
                                new SqlFragmentExpression("SYSDATE")
                            });
                }
            }

            return null;
        }
    }
}