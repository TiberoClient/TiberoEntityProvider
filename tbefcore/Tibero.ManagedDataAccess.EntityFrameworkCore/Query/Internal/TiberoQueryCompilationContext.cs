using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
namespace Tibero.EntityFrameworkCore.Query.Internal
{
    public class TiberoQueryCompilationContext : RelationalQueryCompilationContext
    {
        public TiberoQueryCompilationContext(
               [NotNull] QueryCompilationContextDependencies dependencies,
               [NotNull] ILinqOperatorProvider linqOperatorProvider,
               [NotNull] IQueryMethodProvider queryMethodProvider,
               bool trackQueryResults)
               : base(
                   dependencies,
                   linqOperatorProvider,
                   queryMethodProvider,
                   trackQueryResults)
        {
        }

        
        public override bool IsLateralJoinSupported => true;
    }
}
