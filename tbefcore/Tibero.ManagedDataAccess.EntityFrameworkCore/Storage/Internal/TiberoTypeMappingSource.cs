using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Tibero.EntityFrameworkCore.Storage.Internal.Mapping;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using Tibero.DataAccess.Types;
using Tibero.EntityFrameworkCore.Internal;


namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public class TiberoTypeMappingSource : RelationalTypeMappingSource
    {
        /* 아래 unicode 가 아닌 경우, CHAR, VARCHAR2 로 mapping 되는 애들에 대해서 DbType.AnsiString을 넣어줬는데 String으로 바꿈. TDP 에서 지원안함. 
         */

        public Dictionary<string, RelationalTypeMapping> _StoreTypeMappings { get; }
        public Dictionary<Type, RelationalTypeMapping> _ClrTypeMappings { get; }

        readonly HashSet<string> _userDefinedTypes = new HashSet<string>();

        #region Mappings

        // Numeric types
        readonly TiberoShortTypeMapping _short = new TiberoShortTypeMapping("NUMBER(5)", new DbType?(DbType.Int16));
        readonly TiberoUShortTypeMapping _ushort = new TiberoUShortTypeMapping("NUMBER(5)", new DbType?(DbType.Int16));
        readonly TiberoIntTypeMapping _int = new TiberoIntTypeMapping("NUMBER(10)", new DbType?(DbType.Int32));
        readonly TiberoUIntTypeMapping _uint = new TiberoUIntTypeMapping("NUMBER(10)", new DbType?(DbType.Int32));
        readonly TiberoLongTypeMapping _long = new TiberoLongTypeMapping("NUMBER(19)", new DbType?(DbType.Int64));
        readonly TiberoULongTypeMapping _ulong = new TiberoULongTypeMapping("NUMBER(20)", new DbType?(DbType.Int64));
        readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("NUMBER", new DbType?(DbType.Decimal));

        /* Floating Point */
        readonly TiberoFloatTypeMapping _real = new TiberoFloatTypeMapping("BINARY_FLOAT", new DbType?());
        readonly TiberoDoubleTypeMapping _double = new TiberoDoubleTypeMapping("BINARY_DOUBLE", new DbType?());

        // Character types
        /*
         * 아래 character mapping 들은 clr type string 인 경우에는 clr type을 다룰 때, string 으로 표현된 데이터를 다루겠다는 의미라서,
         * 길이와 unicode, fixed length 만 사용되어서 char varchar nchar nvarchar clob nclob 을 StoreType으로 가지는 TiberoStringMapping 이 생성됨. 
         * 그리고 StoreType으로 찾을 때 대부분은 DbType.String 으로 설정됨. 따라서 StoreDictionary 에 추가될 필요없을 것 같음. 
         * StoreTypeName 을 가지고 FindMapping 을 타는 경우는 여러경우가 있겠지만, 사용자가 property 에 특정 type 으로 설정을 해주었을 때,
         * 해당 Type에 대한 Mapping 정보를 얻기위함. 
         * 
         */
        readonly TiberoStringTypeMapping _xml = new TiberoStringTypeMapping("XML", new DbType?(), true);
        readonly TiberoStringTypeMapping _fixedLengthAnsiString = new TiberoStringTypeMapping("CHAR", new DbType?(DbType.String), false, new int?(), true);
        readonly TiberoStringTypeMapping _fixedLengthUnicodeString = new TiberoStringTypeMapping("NCHAR", new DbType?(DbType.String), true, new int?(), true);
        readonly TiberoStringTypeMapping _variableLengthAnsiString = new TiberoStringTypeMapping("VARCHAR2", new DbType?(DbType.String));
        readonly TiberoStringTypeMapping _variableLengthUnicodeString = new TiberoStringTypeMapping("NVARCHAR2", new DbType?(DbType.String), true);
        readonly TiberoStringTypeMapping _urowId = new TiberoStringTypeMapping("UROWID", new DbType?());
        //readonly TiberoStringTypeMapping _

        /* Raw Type */
        readonly TiberoByteTypeMapping _byte = new TiberoByteTypeMapping("RAW(1)", new DbType?(DbType.Byte));
        readonly TiberoSByteTypeMapping _sbyte = new TiberoSByteTypeMapping("NUMBER(3)", new DbType?());
        readonly TiberoByteArrayTypeMapping _variableLengthBinary = new TiberoByteArrayTypeMapping("BLOB", new DbType?(DbType.Binary));
        readonly TiberoByteArrayTypeMapping _rowversion = new TiberoByteArrayTypeMapping("RAW(8)", DbType.Binary, size: 8, comparer: new ValueComparer<byte[]>(
                    (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                    v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                    v => v == null ? null : v.ToArray()),
                    storeTypePostfix: StoreTypePostfix.None);

        // Other types
        readonly TiberoBoolTypeMapping _bool = new TiberoBoolTypeMapping("NUMBER(1)", new DbType?(DbType.Byte));
        readonly TiberoGuidTypeMapping _guid = new TiberoGuidTypeMapping("RAW(16)", new DbType?(DbType.String), size: 16);
        //readonly TiberoGuidTypeMapping2 _guid = new TiberoGuidTypeMapping2("RAW(16)", new DbType? (DbType.Guid), size: 16);
        //readonly TiberoByteArrayTypeMapping _guid = new TiberoByteArrayTypeMapping("RAW(16)", new DbType?(DbType.Guid), size: 16 );

        /* Data Time */
        readonly TiberoDateTimeTypeMapping _date = new TiberoDateTimeTypeMapping("DATE", new DbType?(DbType.Date));
        readonly TiberoDateTimeTypeMapping _datetime = new TiberoDateTimeTypeMapping("TIMESTAMP(7)", new DbType?(DbType.DateTime)); // 여기서 정밀도 7은 ms 프로바이더에도 정밀도를 7로 하더라. 
        readonly TiberoTimeSpanTypeMapping _time = new TiberoTimeSpanTypeMapping("INTERVAL DAY(8) TO SECOND(7)", new DbType?()); // timespan 의 날짜의 정밀도는 8, second 의 정밀도는 7임. 테스트 해보니 그럼.
        readonly TiberoDateTimeOffsetTypeMapping _datetimeoffset = new TiberoDateTimeOffsetTypeMapping("TIMESTAMP(3) WITH TIME ZONE", new DbType?());
        readonly TiberoDateTimeOffsetLocalTypeMapping _datetimeoffsetlocal = new TiberoDateTimeOffsetLocalTypeMapping("TIMESTAMP WITH LOCAL TIME ZONE", new DbType?());

        private readonly HashSet<string> _disallowedMappings
           = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
           {
                "binary",
                "binary varying",
                "varbinary",
                "char",
                "character",
                "char varying",
                "character varying",
                "varchar",
                "national char",
                "national character",
                "nchar",
                "national char varying",
                "national character varying",
                "nvarchar",
                "nvarchar2",
                "varchar2"
           };
        #endregion Mappings

        public TiberoTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
            [CanBeNull] ITiberoOptions TiberoOptions = null)
            : base(dependencies, relationalDependencies)
        {
          
            /* 
             * Dictionary 에 등록해놓은건 추가 처리 없이 바로 꺼내가려고 보통 등록해놓은거 같음. 
             */
            _StoreTypeMappings = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
            {
                /* binary type */
                {"blob", _variableLengthBinary },
                {"long raw", _variableLengthBinary },
                {"bfile", _variableLengthBinary },
                /* character type */
                {"xml", _xml },
                /* floating type */
                {"binary_float", _real},
                {"binary_double", _double },
                {"date", _date },
                {"timestamp with local time zone", _datetimeoffsetlocal}
                /* numberic type */
       
                //{"number(5)", this._int }
                //{"char", this._fixedlengthString }
            };

            _ClrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(short), this._short },
                { typeof(ushort), this._ushort },
                { typeof(int), this._int },
                { typeof(uint), this._uint },
                { typeof(long), this._long },
                { typeof(ulong), this._ulong },
                { typeof(decimal), this._decimal },
                { typeof(byte), this._byte },
                { typeof(sbyte), this._sbyte },
                { typeof(float), this._real },
                { typeof(double), this._double },
                { typeof(bool), this._bool },
                { typeof(Guid), this._guid },
                { typeof(DateTime), this._datetime },
                { typeof(TimeSpan), this._time },
                { typeof(DateTimeOffset), this._datetimeoffset },
                { typeof(TiberoTimeStampTZ), this._datetimeoffset },
                { typeof(TiberoTimeStampLTZ), this._datetimeoffsetlocal }

            };


            
        }

       
        
        void SetupEnumMappings()
        {
            /*
            foreach (var adoMapping in TiberoConnection.GlobalTypeMapper.Mappings.Where(m => m.TypeHandlerFactory is IEnumTypeHandlerFactory))
            {
                var storeType = adoMapping.PgTypeName;
                var clrType = adoMapping.ClrTypes.SingleOrDefault();
                if (clrType == null)
                {
                    // TODO: Log skipping the enum
                    continue;
                }

                var nameTranslator = ((IEnumTypeHandlerFactory)adoMapping.TypeHandlerFactory).NameTranslator;

                var mapping = new NpgsqlEnumTypeMapping(storeType, clrType, nameTranslator);
                ClrTypeMappings[clrType] = mapping;
                StoreTypeMappings[storeType] = new[] { mapping };
                _userDefinedTypes.Add(storeType);
            }*/
        }
        /*
        protected override RelationalTypeMapping FindMapping(
      in RelationalTypeMappingInfo mappingInfo)
        {

            RelationalTypeMapping relationalTypeMapping = (RelationalTypeMapping)null;

            /* 여기서 clone 을 때릴 때, 따로 mappingInfo 에 storeType이 없으면 가져온 TypeMapping 의 기본 값인 StoreType으로 복사 시작 .*/
        /* relationalTypeMapping = FindRawMapping(mappingInfo)?.Clone(in mappingInfo);

         return relationalTypeMapping;
     }*/

            /* relational type mapping 부모 클래스에서는 findmapping 하고 나서 char, varchar 계열의 애들의 storetypename에 길이정보를 직접 기술하지 않으면 disallow함. 
             * 그 정보로 column 이름을 생성하나봄..
             * 사실 char는 없어도 column 생성되는데 그냥 묶어놈.*/
        protected override void ValidateMapping(CoreTypeMapping mapping, IProperty property)
        {
            var relationalMapping = mapping as RelationalTypeMapping;
            
            if (_disallowedMappings.Contains(relationalMapping?.StoreType))
            {
                if (property == null)
                {
                    throw new ArgumentException(TiberoStrings.UnqualifiedDataType(relationalMapping.StoreType));
                }
               
                throw new ArgumentException(TiberoStrings.UnqualifiedDataTypeOnProperty(relationalMapping.StoreType, property.Name));
            }
        }

        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
           => FindRawMapping(mappingInfo)?.Clone(mappingInfo);
        private RelationalTypeMapping FindRawMapping(
          RelationalTypeMappingInfo mappingInfo)
        {
            
            Type clrType = mappingInfo.ClrType;
            string storeTypeName = mappingInfo.StoreTypeName;
            string storeTypeNameBase = mappingInfo.StoreTypeNameBase;
            
            /* FindMapping 에서 mappinginfo 에 storeType이 존재할 수 있음
             * 그 순서는 mappingInfo 를 만들면서 storeType을 생성자 호출시에 있으면 존재할 수 있음. 
             * 그리고 findmapping 에서는 clone 을 생성해서 넘겨주는 과정에서 storeType도 parameter 내에 내포되어 넘어가고 최종적으로 TypeMapping 의 storeType으로 존재할 수 있게됨. 
             *
             */
            if (storeTypeName != null)
            {
               
                /* StoreTypeName 이 타는 경우를 아직 찾을수 없음.. 일단 clr type 으로 찾는 경우만 고려해본다. */
                if (clrType == typeof(float)
                  && mappingInfo.Size != null
                  && mappingInfo.Size <= 24
                  && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                      || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
                {
                    return _real;
                }
                else if(clrType == typeof(TiberoTimeStampLTZ))
                {
                    if (storeTypeName.ToLowerInvariant().Contains("local"))
                        return _datetimeoffsetlocal;
                }

                if (!string.IsNullOrEmpty(storeTypeNameBase))
                {
                    switch (storeTypeNameBase.ToLowerInvariant())
                    {
                        /* character 로 취급할 수 있는 type으로 사용자가 명시적으로 column type을 지정하면
                         * 무조건 DbType.String 으로 DbType을 설정하고 storeTypeName 도 사용자가 지정한거로 넘기는데. 
                         * 이렇게 해도 무방한가 테스트 필요. 
                         * 이렇게 되면 storetype dictionary 에도 등록할 필요가 없음. 
                         */
                        case "char":
                        case "clob":
                        case "long":
                        case "nchar":
                        case "nclob":
                        case "nvarchar2":
                        case "rowid":
                        case "urowid":
                        case "varchar2":
                            string storeType = storeTypeNameBase;
                            DbType? dbType = new DbType?(DbType.String);
                            bool isUnicode = mappingInfo.IsUnicode == true;
                            bool isFixedLength = mappingInfo.IsFixedLength == true;
                            int? size = mappingInfo.Size;
                            StoreTypePostfix? storeTypePostfix = new StoreTypePostfix?();

                            /* 아래 내용 unicode 설정은 unit test 만들면서 추가. unicode type에 대해서는 설정해줘야할 것 같은데 기존에도 정상동작하고있었음.
                             * 혹시 문제생기면 다시 rollback */
                            if (storeTypeNameBase.ToLowerInvariant().StartsWith("n", StringComparison.OrdinalIgnoreCase))
                                isUnicode = true;

                            return new TiberoStringTypeMapping(storeType, dbType, isUnicode, size, isFixedLength, storeTypePostfix);

                        case "number":
                            int? precision = mappingInfo.Precision;
                            int? scale = mappingInfo.Scale;
                                                        
                            if (precision != null)
                            {
                                if (scale != null)
                                {
                                    return new DecimalTypeMapping(string.Format("NUMBER({0},{1})", precision, scale), new DbType?(DbType.Decimal));
                                }
                                else if (scale == null)
                                {
                                    /* 굳이 이렇게 세부적으로 나눌 필요가 있나 싶음 전부 DECIMAL 로 처리해도 될 것 같은데 */
                                    if (precision == 1)
                                        return new TiberoBoolTypeMapping("NUMBER(1)", new DbType?(DbType.Byte));
                                    else if (precision <= 4)
                                        return new ByteTypeMapping(string.Format("NUMBER({0})", precision), new DbType?(DbType.Byte));
                                    else if (precision <= 5)
                                        return new ShortTypeMapping(string.Format("NUMBER({0})", precision), new DbType?(DbType.Int16));
                                    else if (precision <= 9)
                                        return new TiberoIntTypeMapping(string.Format("NUMBER({0})", precision), new DbType?(DbType.Int32));
                                    else if (precision <= 19)
                                        return new LongTypeMapping(string.Format("NUMBER({0})", precision), new DbType?(DbType.Int64));
                                    else
                                        return new DecimalTypeMapping(string.Format("NUMBER({0}", precision), new DbType?(DbType.Decimal));
                                }
                            }
                            return new DecimalTypeMapping("NUMBER", new DbType?(DbType.Decimal));
                            

                            //case "raw":
                            //nullable1 = mappingInfo.Size;
                            //return 16 == nullable1.GetValueOrDefault() & nullable1.HasValue ? (RelationalTypeMapping)this._guid : (RelationalTypeMapping)new TiberoByteArrayTypeMapping(string.Format("RAW({0})", (object)mappingInfo.Size), new DbType?(DbType.Binary), mappingInfo.Size, false, (ValueComparer)null, new StoreTypePostfix?());


                    }
                }
                RelationalTypeMapping relationalTypeMapping;
                if (this._StoreTypeMappings.TryGetValue(storeTypeName, out relationalTypeMapping))
                {
                   
                    return clrType == (Type)null || relationalTypeMapping.ClrType == clrType ? relationalTypeMapping : (RelationalTypeMapping)null;
                }
                if (this._StoreTypeMappings.TryGetValue(storeTypeNameBase, out relationalTypeMapping))
                {
                    return clrType == (Type)null || relationalTypeMapping.ClrType == clrType ? relationalTypeMapping : (RelationalTypeMapping)null;
                }
            }

            if (clrType != (Type)null)
            {
                RelationalTypeMapping relationalTypeMapping;
                if (this._ClrTypeMappings.TryGetValue(clrType, out relationalTypeMapping))
                {
                   
                    return relationalTypeMapping;
                }

                if (clrType == typeof(string))
                {
                    /* clrType string 은 char, varchar2, clob, nclob 으로 storetype을 만들겠다.. */
                    var isAnsi = mappingInfo.IsUnicode == false;
                    var isFixedLength = mappingInfo.IsFixedLength == true;
                    var baseName = (isAnsi ? "" : "N") + (isFixedLength ? "CHAR" : "VARCHAR2");
                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)(isAnsi ? 900 : 450) : (int?)(isAnsi ? (isFixedLength ? 2000 : 4000) : (isFixedLength ? 1000 : 2000)));


                    var maxSize = isAnsi ? (isFixedLength ? 2000 : 4000) : (isFixedLength ? 1000 : 2000);

                    /* 아래에서 size 가 0일 수가 있나? */
                    if (size > maxSize || (size == 0 && isFixedLength))
                    {
                        string storeType = isAnsi ? "CLOB" : "NCLOB";

                        return new TiberoUnboundedTypeMapping(storeType, new DbType?(), !isAnsi, new int?(), false, new StoreTypePostfix?());
                    }

                    return new TiberoStringTypeMapping(baseName + "(" + size + ")", isAnsi ? new DbType?(DbType.String) : new DbType?(), !isAnsi, size, isFixedLength, new StoreTypePostfix?());

                }
                if (clrType == typeof(byte[]))
                {
                    /* rowversion 이면 rowversion mapping 을 반환
                     * 아니라면 길이에 따라 BLOB 아니면 RAW 
                     * 여기서 반환한 애가 BLOB 인 경우는 ODP TYPE을 무조건 BLOB 으로 아니라면 RAW 로 
                     * 즉 clr tpye binary -> RAW OR BLOB 으로. 
                     */
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _rowversion;
                    }

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);

                    StoreTypePostfix? storeTypePostfix = new StoreTypePostfix?();
                    string storeType;

                    if (size == null)
                        size = new int?(2000);

                    if (size.GetValueOrDefault() <= 2000)
                    {
                        storeTypePostfix = StoreTypePostfix.Size;
                        storeType = "RAW(" + size + ")";
                        return (RelationalTypeMapping)new TiberoByteArrayTypeMapping(storeType, new DbType?(DbType.Binary), size, false, (ValueComparer)null, storeTypePostfix);
                    }
                    else
                    {
                        storeTypePostfix = StoreTypePostfix.None;
                        storeType = "BLOB";
                        return (RelationalTypeMapping)new TiberoByteArrayTypeMapping(storeType, new DbType?(DbType.Binary), size, false, (ValueComparer)null, storeTypePostfix);
                    }

                }

            }

            return (RelationalTypeMapping)null;

        }
    }
}
