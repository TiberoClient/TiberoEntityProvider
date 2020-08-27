using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Tibero.DataAccess.Client;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace Tibero.EntityFrameworkCore.Storage.Internal.Mapping
{
    // TODO: See #357. We should be able to simply use StringTypeMapping but DbParameter.Size isn't managed properly.
    public class TiberoStringTypeMapping : StringTypeMapping
    {
        
        private readonly int _maxSpecificSize;
        private readonly StoreTypePostfix? _storeTypePostfix;

        /* StringTypeMapping 에서 relationalTypeMappingParameter 를 들고 있음. 
         * 이 때 param 정보를 수정해서 달아놓기 위해서 새로 생성하고 가공하여 base 생성자 호출.
         * 가공하려는 내용은 storeTypePostfix with unicode, size 
         */ 
        public TiberoStringTypeMapping(
          [NotNull] string storeType,
          [CanBeNull] DbType? dbType,
          bool unicode = false,
          int? size = null,
          bool fixedLength = false,
          StoreTypePostfix? storeTypePostfix = null)
          : this(new RelationalTypeMappingParameters(
                     new CoreTypeMappingParameters(typeof(string)), 
                     storeType, 
                     GetStoreTypePostfix(storeTypePostfix, unicode, size), 
                     dbType, 
                     unicode, 
                     size, 
                     fixedLength))
        {
            
            this._storeTypePostfix = storeTypePostfix;
        }

        protected TiberoStringTypeMapping(
          RelationalTypeMapping.RelationalTypeMappingParameters parameters
          )
          : base(parameters)
        {
           
            this._maxSpecificSize = TiberoStringTypeMapping.CalculateSize(parameters.Unicode, parameters.Size);
        }

        private static StoreTypePostfix GetStoreTypePostfix(
          StoreTypePostfix? storeTypePostfix,
          bool unicode,
          int? size)
        {
            int criteria = unicode == true ? 2000 : 4000;

            return storeTypePostfix ?? (size <= criteria ? StoreTypePostfix.Size : StoreTypePostfix.None);
            
        }

        public override DbParameter CreateParameter(
          [NotNull] DbCommand command,
          [NotNull] string name,
          [CanBeNull] object value,
          bool? nullable = null)
        {
            /*
             * stringTypeMapping 에 CreateParameter 가 있다는 건 EF 단에서 mapping source 에서 dictionary 얻고 해당 mapping class 얻어서 parameter를 만들겠다?
             */
            Check.NotNull<DbCommand>(command, nameof(command));
            /*
             * base.CreateParamter -> command.CreateParameter() (abstract 함수로 tiberoCommand 에서 있는 CreateParameter 를 부르게 됨.) 
             * -> CreateParameter() : 여기서 빈 클래스를 생성을 하지만 자동으로 TiberoDbType이 varchar2 로 dbType 이 String 으로 설정됨. 
             * 아래 내용에서 DbType 이 바뀔수 있는 여지로는 StringMapping class 에 dbtype이 설정되어 있는 경우 base.CreateParameter 에서 tiberoParameter 의 dbtype에 세팅됨. 
             * 말고는 tiberoDbType을 아래와 같이 설정해주는 경우임. 
             */ 
            
            TiberoParameter parameter = (TiberoParameter)base.CreateParameter(command, name, value, nullable);

            /* Tibero 에서는 CLOB 의 경우 short.MaxValue 이상일 때 그리고 value 가 string 과 char[] 가 맞는지 여부에 따라 
             * XML 은 4000 자 이하(tibero 11 기준 )
             * 우리는 그냥 바로 CLOB XMLTYPE으로 박자잇.
             * 이외의 경우에는 그냥 dbType string TiberoDbType varchar2 로 자동생성된 type을 따른다. 
             */
            if (this.StoreType == "CLOB")
            {
                    parameter.TiberoDbType = TiberoDbType.Clob;
            }
            if (this.StoreType == "XMLTYPE")
            {
                parameter.TiberoDbType = TiberoDbType.XmlType;
            }
            else if (this.StoreType == "NCLOB")
            {
                parameter.TiberoDbType = TiberoDbType.NClob;
            }
            
            return (DbParameter)parameter;
        }

        private static int CalculateSize(bool unicode, int? size)
        {
            int criteria = unicode == true ?  2000 : 4000;

            if (size != null)
                return size <= criteria ? size.Value : criteria;
            else
                return criteria;
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
        {
            return new TiberoStringTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size, new StoreTypePostfix?(GetStoreTypePostfix(_storeTypePostfix, IsUnicode, size))));
        }

        public override CoreTypeMapping Clone(ValueConverter converter)
        {
            return new TiberoStringTypeMapping(Parameters.WithComposedConverter(converter));
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var value = parameter.Value;
            var length = (value as string)?.Length;
            try
            {
                parameter.Size = value == null || value == DBNull.Value || length != null && length <= _maxSpecificSize
                ? _maxSpecificSize
                : 0;
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            if (string.IsNullOrEmpty((string)value))
                return "NULL";
            return !this.IsUnicode ? "'" + this.EscapeSqlLiteral((string)value) + "'" : "N'" + this.EscapeSqlLiteral((string)value) + "'";
        }
    }
}
