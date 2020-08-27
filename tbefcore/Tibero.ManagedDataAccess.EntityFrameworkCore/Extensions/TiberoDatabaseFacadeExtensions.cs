using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
namespace Tibero.EntityFrameworkCore.Extensions
{
    public static class TiberoDatabaseFacadeExtensions
    {

        public static bool IsTibero([NotNull] this DatabaseFacade database)
        {
            
            return database.ProviderName.Equals(
               typeof(TiberoOptionsExtension).GetTypeInfo().Assembly.GetName().Name,
               StringComparison.Ordinal);
        }
    }
}
