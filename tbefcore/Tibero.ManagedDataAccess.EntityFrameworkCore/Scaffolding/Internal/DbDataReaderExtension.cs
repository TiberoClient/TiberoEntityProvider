using System.Data.Common;
using JetBrains.Annotations;

namespace Tibero.EntityFrameworkCore.Scaffolding.Internal
{
    public static class DbDataReaderExtension
    {
        public static T GetValueOrDefault<T>([NotNull] this DbDataReader reader, [NotNull] string name)
        {
            var idx = reader.GetOrdinal(name);
            return reader.IsDBNull(idx)
                ? default(T)
                : reader.GetFieldValue<T>(idx);
        }

        public static T GetValueOrDefault<T>([NotNull] this DbDataRecord record, [NotNull] string name)
        {
            var idx = record.GetOrdinal(name);
            return record.IsDBNull(idx)
                ? default
                : (T)record.GetValue(idx);
        }
    }
}
