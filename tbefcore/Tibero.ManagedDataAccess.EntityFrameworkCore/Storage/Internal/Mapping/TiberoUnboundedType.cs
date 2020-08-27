using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.DataAccess.Client;
using System;
using System.Data;
using System.Data.Common;

namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoUnboundedTypeMapping : StringTypeMapping
    {
        private const int MaxSize = 2147483647;
        private readonly int _maxSpecificSize;
        private readonly StoreTypePostfix? _storeTypePostfix;

        public TiberoUnboundedTypeMapping(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType,
          bool unicode = false,
          int? size = null,
          bool fixedLength = false,
          StoreTypePostfix? storeTypePostfix = null)
          : this(new RelationalTypeMapping.RelationalTypeMappingParameters(new CoreTypeMapping.CoreTypeMappingParameters(typeof(string), 
              (ValueConverter)null, (ValueComparer)null, (ValueComparer)null, (Func<IProperty, IEntityType, ValueGenerator>)null), 
              storeType, TiberoUnboundedTypeMapping.GetStoreTypePostfix(storeTypePostfix, unicode, size), dbType, unicode, new int?(), fixedLength, new int?(), new int?()))
        {
            size = new int?();
            this._storeTypePostfix = storeTypePostfix;
        }

        protected TiberoUnboundedTypeMapping(
          RelationalTypeMapping.RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
            this._maxSpecificSize = TiberoUnboundedTypeMapping.CalculateSize(parameters.Unicode, parameters.Size);
        }

        private static StoreTypePostfix GetStoreTypePostfix(
          StoreTypePostfix? storeTypePostfix,
          bool unicode,
          int? size)
        {
            return StoreTypePostfix.None;
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            Check.NotNull<DbCommand>(command, nameof(command));
            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);
            if (this.StoreType == "CLOB")
                parameter.TiberoDbType = TiberoDbType.Clob;
            else if (this.StoreType == "NCLOB")
                parameter.TiberoDbType = TiberoDbType.NClob;
            return (DbParameter)parameter;
        }

        private static int CalculateSize(bool unicode, int? size)
        {
            return 0;
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return (RelationalTypeMapping)new TiberoUnboundedTypeMapping(this.Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?(TiberoUnboundedTypeMapping.GetStoreTypePostfix(this._storeTypePostfix, this.IsUnicode, size))));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return (CoreTypeMapping)new TiberoUnboundedTypeMapping(this.Parameters.WithComposedConverter(converter));
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            if (parameter.Value is string str)
            {
                int length = str.Length;
            }
            try
            {
                parameter.Size = 0;
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            return !this.IsUnicode ? "'" + this.EscapeSqlLiteral((string)value) + "'" : "N'" + this.EscapeSqlLiteral((string)value) + "'";
        }
    }
}