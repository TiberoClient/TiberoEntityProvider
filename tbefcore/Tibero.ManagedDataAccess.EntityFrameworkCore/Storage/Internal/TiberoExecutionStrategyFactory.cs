using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public class TiberoExecutionStrategyFactory : RelationalExecutionStrategyFactory
    {
        public TiberoExecutionStrategyFactory(
            [NotNull] ExecutionStrategyDependencies dependencies)
            : base(dependencies)
        {
        }

    
        protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
            => new TiberoExecutionStrategy(dependencies);
    }
}
