using System;
using System.Reflection;
using System.Resources;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
namespace Tibero.EntityFrameworkCore.Internal
{
    public static class TiberoStrings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Tibero.EntityFrameworkCore.Properties.TiberoStrings", typeof(TiberoStrings).GetTypeInfo().Assembly);
        public static string IdentityBadType([CanBeNull] object property, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("IdentityBadType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

        
        public static string UnqualifiedDataType([CanBeNull] object dataType)
            => string.Format(
                GetString("UnqualifiedDataType", nameof(dataType)),
                dataType);

        
        public static string UnqualifiedDataTypeOnProperty([CanBeNull] object dataType, [CanBeNull] object property)
            => string.Format(
                GetString("UnqualifiedDataTypeOnProperty", nameof(dataType), nameof(property)),
                dataType, property);

       
        public static string SequenceBadType([CanBeNull] object property, [CanBeNull] object entityType, [CanBeNull] object propertyType)
            => string.Format(
                GetString("SequenceBadType", nameof(property), nameof(entityType), nameof(propertyType)),
                property, entityType, propertyType);

       
        public static string IndexTableRequired
            => GetString("IndexTableRequired");

       
        public static string AlterMemoryOptimizedTable
            => GetString("AlterMemoryOptimizedTable");

        
        public static string AlterIdentityColumn
            => GetString("AlterIdentityColumn");

        
        public static string TransientExceptionDetected
            => GetString("TransientExceptionDetected");

               
        public static string NonKeyValueGeneration([CanBeNull] object property, [CanBeNull] object entityType)
            => string.Format(
                GetString("NonKeyValueGeneration", nameof(property), nameof(entityType)),
                property, entityType);

        
        public static string MultipleIdentityColumns([CanBeNull] object properties, [CanBeNull] object table)
            => string.Format(
                GetString("MultipleIdentityColumns", nameof(properties), nameof(table)),
                properties, table);

        
        public static string IncompatibleTableMemoryOptimizedMismatch([CanBeNull] object table, [CanBeNull] object entityType, [CanBeNull] object otherEntityType, [CanBeNull] object memoryOptimizedEntityType, [CanBeNull] object nonMemoryOptimizedEntityType)
            => string.Format(
                GetString("IncompatibleTableMemoryOptimizedMismatch", nameof(table), nameof(entityType), nameof(otherEntityType), nameof(memoryOptimizedEntityType), nameof(nonMemoryOptimizedEntityType)),
                table, entityType, otherEntityType, memoryOptimizedEntityType, nonMemoryOptimizedEntityType);

        public static string NoInitialCatalog
            => GetString("NoInitialCatalog");

        
        public static string DuplicateColumnNameValueGenerationStrategyMismatch([CanBeNull] object entityType1, [CanBeNull] object property1, [CanBeNull] object entityType2, [CanBeNull] object property2, [CanBeNull] object columnName, [CanBeNull] object table)
            => string.Format(
                GetString("DuplicateColumnNameValueGenerationStrategyMismatch", nameof(entityType1), nameof(property1), nameof(entityType2), nameof(property2), nameof(columnName), nameof(table)),
                entityType1, property1, entityType2, property2, columnName, table);

        public static string InvalidTableToIncludeInScaffolding([CanBeNull] object table)
            => string.Format(
                GetString("InvalidTableToIncludeInScaffolding", nameof(table)),
                table);

        
        public static string FreeTextFunctionOnClient
            => GetString("FreeTextFunctionOnClient");

        
        public static string InvalidColumnNameForFreeText
            => GetString("InvalidColumnNameForFreeText");

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = _resourceManager.GetString(name);
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}
