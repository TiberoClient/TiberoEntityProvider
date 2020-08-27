using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoStringLengthTranslator : IMemberTranslator
    {
        public virtual Expression Translate(MemberExpression memberExpression)
        {
          

            return memberExpression.Expression != null
              && memberExpression.Expression.Type == typeof(string)
              && memberExpression.Member.Name == nameof(string.Length)
               ? new ExplicitCastExpression(
                   new SqlFunctionExpression("LENGTH", memberExpression.Type, new[] { memberExpression.Expression }),
                   typeof(int))
               : null;
        }
    }
}
