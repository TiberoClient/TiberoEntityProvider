using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Tibero.EntityFrameworkCore.Query.Internal
{
    public class TiberoCompileQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        public TiberoCompileQueryCacheKeyGenerator(
            [NotNull] CompiledQueryCacheKeyGeneratorDependencies dependencies,
            [NotNull] RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }
        public override object GenerateCacheKey(Expression query, bool async)
           => new TiberoCompiledQueryCacheKey(
               GenerateCacheKeyCore(query, async),
                false);

        private readonly struct TiberoCompiledQueryCacheKey
        {
            private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
            private readonly bool _useRowNumberOffset;

            public TiberoCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey, bool useRowNumberOffset)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
                _useRowNumberOffset = useRowNumberOffset;
            }

            public override bool Equals(object obj)
                => !(obj is null)
                   && obj is TiberoCompiledQueryCacheKey
                   && Equals((TiberoCompiledQueryCacheKey)obj);

            private bool Equals(TiberoCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                   && _useRowNumberOffset == other._useRowNumberOffset;

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_relationalCompiledQueryCacheKey.GetHashCode() * 397) ^ _useRowNumberOffset.GetHashCode();
                }
            }
        }
    }
}
