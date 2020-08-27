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
using System.Globalization;
using System.Text;

namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    public class TiberoByteArrayTypeMapping : ByteArrayTypeMapping
    {
        private const int MaxSize = 2147483647;
        private readonly int _maxSpecificSize;
        private readonly StoreTypePostfix? _storeTypePostfix;

        public TiberoByteArrayTypeMapping(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType = System.Data.DbType.Binary,
          int? size = null,
          bool fixedLength = false,
          ValueComparer comparer = null,
          StoreTypePostfix? storeTypePostfix = null)
          : this(new RelationalTypeMappingParameters(
                     new CoreTypeMappingParameters(typeof(byte[]), null, comparer), 
                     storeType, 
                     GetStoreTypePostfix(storeTypePostfix, size), 
                     dbType, 
                     size: size, 
                     fixedLength: fixedLength))
        {
            _storeTypePostfix = storeTypePostfix;
        }

        protected TiberoByteArrayTypeMapping(
          RelationalTypeMapping.RelationalTypeMappingParameters parameters)
          : base(parameters)
        {
            this._maxSpecificSize = TiberoByteArrayTypeMapping.CalculateSize(parameters.Size);
        }

        private static StoreTypePostfix GetStoreTypePostfix(
          StoreTypePostfix? storeTypePostfix,
          int? size)
        {
            /* 이미 postfix 가 있으면 그냥 그대로 쓰고 없으면 maxsize가 넘어가지 않는 선에서 Size 로 */
            if (storeTypePostfix.HasValue)
                return storeTypePostfix.GetValueOrDefault(); 
            else
            {
                StoreTypePostfix replace_storeTypePostfix = (size != null && size <= MaxSize) ? StoreTypePostfix.Size : StoreTypePostfix.None;
                return replace_storeTypePostfix;
            }
        }

        private static int CalculateSize(int? size)
        {
            return size.HasValue && size < MaxSize ? size.Value : MaxSize;
          
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            
            return new TiberoByteArrayTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?(GetStoreTypePostfix(this._storeTypePostfix, size))));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoByteArrayTypeMapping(Parameters.WithComposedConverter(converter));
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            Check.NotNull<DbCommand>(command, nameof(command));
            var c = base.CreateParameter(command, name, value, nullable);
            TiberoParameter parameter = (TiberoParameter)c;
            if (this.StoreType == "BLOB")
                parameter.TiberoDbType = TiberoDbType.Blob;
            else if (this.StoreType == "BFILE")
                parameter.TiberoDbType = TiberoDbType.BFile;
            if (((TiberoCommand)command).BindByName == false)
            {               
                ((TiberoCommand)command).BindByName = true;
            }
            return (DbParameter)parameter;
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as byte[])?.Length;
            var maxSpecificSize = CalculateSize(Size);
            var size = parameter.Size;

            parameter.Size = value == null || value == DBNull.Value || length != null && length <= maxSpecificSize
                ? maxSpecificSize
                : size;
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("'");
            foreach (byte num in (byte[])value)
                stringBuilder.Append(num.ToString("X2", CultureInfo.InvariantCulture));
            stringBuilder.Append("'");
            return stringBuilder.ToString();
        }
    }
}
