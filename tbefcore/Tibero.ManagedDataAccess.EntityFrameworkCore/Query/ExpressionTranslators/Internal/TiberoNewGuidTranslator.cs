using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
namespace Tibero.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class TiberoNewGuidTranslator : SingleOverloadStaticMethodCallTranslator
    {
        /*SYS_GUID 쓰면... where 절같은데 들어가면 서버에서 답이 안옴 ;; 일단 등록을 안해놓았따. */
        public TiberoNewGuidTranslator()
            : base(typeof(Guid), nameof(Guid.NewGuid), "SYS_GUID")
        {
        }
    }
}
