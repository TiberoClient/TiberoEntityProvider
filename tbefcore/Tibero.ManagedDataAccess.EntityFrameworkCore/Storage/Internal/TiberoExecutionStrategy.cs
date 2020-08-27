using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Internal;
namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public class TiberoExecutionStrategy : IExecutionStrategy
    {
        private ExecutionStrategyDependencies Dependencies { get; }
        public TiberoExecutionStrategy([NotNull] ExecutionStrategyDependencies dependencies)
        {
            Dependencies = dependencies;
        }
        public virtual bool RetriesOnFailure => false;
        public virtual TResult Execute<TState, TResult>(
            TState state,
            Func<DbContext, TState, TResult> operation,
            Func<DbContext, TState, ExecutionResult<TResult>> verifySucceeded)
        {
            try
            {
                return operation(Dependencies.CurrentDbContext.Context, state);
            }
            catch (Exception ex) when (ExecutionStrategy.CallOnWrappedException(ex, TiberoTransientExceptionDetector.ShouldRetryOn))
            {
                throw new InvalidOperationException(TiberoStrings.TransientExceptionDetected, ex);
            }
        }
        public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
            TState state,
            Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
            Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>> verifySucceeded,
            CancellationToken cancellationToken)
        {
            try
            {
                return await operation(Dependencies.CurrentDbContext.Context, state, cancellationToken);
            }
            catch (Exception ex) when (ExecutionStrategy.CallOnWrappedException(ex, TiberoTransientExceptionDetector.ShouldRetryOn))
            {
                throw new InvalidOperationException(TiberoStrings.TransientExceptionDetected, ex);
            }
        }
    }
}
