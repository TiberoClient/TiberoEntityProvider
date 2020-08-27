using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tibero.DataAccess.Types;
using Tibero.DataAccess.Client;
using Tibero.EntityFrameworkCore.Storage.Internal.ValueConversion;
namespace Tibero.EntityFrameworkCore.Extensions
{
    public static class EFExtensions
    {
        /// <summary>
        /// Creates an initial DbCommand object based on a stored procedure name
        /// </summary>
        /// <param name="context">target database context</param>
        /// <param name="storedProcName">target procedure name</param>
        /// <param name="prependDefaultSchema">Prepend the default schema name to <paramref name="storedProcName"/> if explicitly defined in <paramref name="context"/></param>
        /// <param name="commandTimeout">Command timeout in seconds. Default is 30.</param>
        /// <returns></returns>
        public static DbCommand LoadStoredProc(this DbContext context, string storedProcName,
            bool prependDefaultSchema = true, short commandTimeout = 30)
        {
            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandTimeout = commandTimeout;

            if (prependDefaultSchema)
            {
                var schemaName = context.Model["DefaultSchema"];
                if (schemaName != null)
                {
                    storedProcName = $"{schemaName}.{storedProcName}";
                }
            }

            cmd.CommandText = storedProcName;
            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        /// <summary>
        /// Creates a DbParameter object and adds it to a DbCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <param name="configureParam"></param>
        /// <returns></returns>
        public static DbCommand AddParam(this DbCommand cmd, string paramName, object paramValue,
            Action<TiberoParameter> configureParam = null)
        {
            if (string.IsNullOrEmpty(cmd.CommandText) && cmd.CommandType != System.Data.CommandType.StoredProcedure)
                throw new InvalidOperationException("Call LoadStoredProc before using this method");

            var param = ((TiberoCommand)cmd).CreateParameter();
            param.ParameterName = paramName;
            param.Value = paramValue ?? DBNull.Value;
            configureParam?.Invoke(param);
            cmd.Parameters.Add(param);
            return cmd;
        }

        /// <summary>
        /// Creates a DbParameter object and adds it to a DbCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="paramName"></param>
        /// <param name="configureParam"></param>
        /// <returns></returns>
        public static DbCommand AddParam(this DbCommand cmd, string paramName,
            Action<TiberoParameter> configureParam = null)
        {
            if (string.IsNullOrEmpty(cmd.CommandText) && cmd.CommandType != CommandType.StoredProcedure)
                throw new InvalidOperationException("Call LoadStoredProc before using this method");

            var param = ((TiberoCommand)cmd).CreateParameter();
            param.ParameterName = paramName;
            configureParam?.Invoke(param);
            cmd.Parameters.Add(param);
            return cmd;
        }

        /// <summary>
        /// Adds a SqlParameter to a DbCommand.
        /// This enabled the ability to provide custom types for SQL-parameters.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        
        public static DbCommand AddParam(this DbCommand cmd, IDbDataParameter parameter)
        {
            if (string.IsNullOrEmpty(cmd.CommandText) && cmd.CommandType != System.Data.CommandType.StoredProcedure)
                throw new InvalidOperationException("Call LoadStoredProc before using this method");

            cmd.Parameters.Add(parameter);

            return cmd;
        }

        /// <summary>
        /// Adds an array of SqlParameters to a DbCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DbCommand AddParams(this DbCommand cmd, IDbDataParameter[] parameters)
        {
            if (string.IsNullOrEmpty(cmd.CommandText) && cmd.CommandType != System.Data.CommandType.StoredProcedure)
                throw new InvalidOperationException("Call LoadStoredProc before using this method");

            cmd.Parameters.AddRange(parameters);

            return cmd;
        }/*
        public static DbCommand AddParam(this DbCommand cmd, DbParameter parameters)
        {
            if (string.IsNullOrEmpty(cmd.CommandText) && cmd.CommandType != System.Data.CommandType.StoredProcedure)
                throw new InvalidOperationException("Call LoadStoredProc before using this method");

            cmd.Parameters.Add(parameters);

            return cmd;
        }*/
        public class SprocResults
        {
            private readonly DbDataReader _reader;

            public SprocResults(DbDataReader reader)
            {
                _reader = reader;

            }

            public IList<T> MapValue<T>() where T : new()
            {
                return MapToList<T>(_reader);
            }

            public T? ReadToValue<T>() where T : struct
            {
                return MapToValue<T>(_reader);
            }

            public Task<bool> NextResultAsync()
            {
                return _reader.NextResultAsync();
            }

            public Task<bool> NextResultAsync(CancellationToken ct)
            {
                return _reader.NextResultAsync(ct);
            }

            public bool NextResult()
            {
                return _reader.NextResult();
            }

            /// <summary>
            /// Retrieves the column values from the stored procedure and maps them to <typeparamref name="T"/>'s properties
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="dr"></param>
            /// <returns>IList&lt;<typeparam name="T">&gt;</typeparam></returns>
            private static IList<T> MapToList<T>(DbDataReader dr) where T : new()
            {
                var objList = new List<T>();
                var props = typeof(T).GetRuntimeProperties().ToList();

                Dictionary<string, int> colMapping = new Dictionary<string, int>();

                var colCount = dr.FieldCount;
                for (int i = 0; i < colCount; i++)
                {
                    colMapping.Add(dr.GetName(i).ToUpper(), i);
                }

                while (dr.Read())
                {
                    var obj = new T();
                    int cnt = 0;
                    foreach (var prop in props)
                    {
                        var upperName = prop.Name.ToUpper();
                        if (!colMapping.ContainsKey(upperName))
                            continue;

                        var idx = colMapping[upperName];

                        var val = dr.GetValue(idx);

                        prop.SetValue(obj, val == DBNull.Value ? null : converterToEntity(prop.PropertyType, val));
                        cnt++;
                    }

                    objList.Add(obj);
                }

                return objList;
            }

            /// <summary>
            /// Attempts to read the first value of the first row of the result set.
            /// </summary>
            private static T? MapToValue<T>(DbDataReader dr) where T : struct
            {
                if (!dr.HasRows)
                    return new T?();

                if (dr.Read())
                {
                    return dr.IsDBNull(0) ? new T?() : dr.GetFieldValue<T>(0);
                }

                return new T?();
            }
        }

        /// <summary>
        /// Executes a DbDataReader and passes the results to <paramref name="handleResults"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handleResults"></param>
        /// <param name="commandBehaviour"></param>
        /// <param name="manageConnection"></param>
        /// <returns></returns>
        public static SprocResults ExecuteStoredProc(this DbCommand command, Action<SprocResults> handleResults)
        {
            SprocResults sprocResults;
            if (handleResults == null)
            {
                throw new ArgumentNullException(nameof(handleResults));
            }

            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        sprocResults = new SprocResults(reader);
                        handleResults(sprocResults);
                    }
                }
                finally
                {
                    //if (manageConnection)
                    //{
                        command.Connection.Close();
                    //}
                }
            }
            return sprocResults;
        }
        public static void ExecuteStoredProc(this DbCommand command,
           params Action<SprocResults>[] handleResults)
        {
            if (handleResults == null)
            {
                throw new ArgumentNullException(nameof(handleResults));
            }

            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var sprocResults = new SprocResults(reader);
                        foreach (var result in handleResults)
                            result(sprocResults);
                    }
                }
                finally
                {
                    //if (manageConnection)
                    //{
                    command.Connection.Close();
                    //}
                }
            }
        }
        /*
        public static SprocResults GetNextResult(this SprocResults sprocResults, CommandBehavior commandBehaviour = CommandBehavior.Default,
            bool manageConnection = true)
        {
            sprocResults.NextResult();
            return sprocResults;
        }
        public static SprocResults ReadStoredProc(this SprocResults sprocResults, Action<SprocResults> handleResults)
        {

            if (handleResults == null)
            {
                throw new ArgumentNullException(nameof(handleResults));
            }


            try
            {
                handleResults(sprocResults);

            }
            finally
            {

            }


            return sprocResults;
        }
       
    */
    /*
        /// <summary>
        /// Executes a DbDataReader asynchronously and passes the results to <paramref name="handleResults"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handleResults"></param>
        /// <param name="commandBehaviour"></param>
        /// <param name="ct"></param>
        /// <param name="manageConnection"></param>
        /// <returns></returns>
        public static async Task ExecuteStoredProcAsync(this DbCommand command, Action<SprocResults> handleResults,
            System.Data.CommandBehavior commandBehaviour = System.Data.CommandBehavior.Default,
            CancellationToken ct = default, bool manageConnection = true)
        {
            if (handleResults == null)
            {
                throw new ArgumentNullException(nameof(handleResults));
            }

            using (command)
            {
                
                if (manageConnection && command.Connection.State == System.Data.ConnectionState.Closed)
                    await command.Connection.OpenAsync(ct).ConfigureAwait(false);
              
                //var task = await Task.Run(() => getString());
                
                try
                {
                    using (var reader = await command.ExecuteReaderAsync(commandBehaviour, ct)
                        .ConfigureAwait(false))
                    {
                        var sprocResults = new SprocResults(reader);
                        
                        handleResults(sprocResults);
                    }
                }
                finally
                {
                    if (manageConnection)
                    {
                        command.Connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Executes a DbDataReader asynchronously and passes the results thru all <paramref name="resultActions"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandBehaviour"></param>
        /// <param name="ct"></param>
        /// <param name="manageConnection"></param>
        /// <param name="resultActions"></param>
        /// <returns></returns>
        public static async Task ExecuteStoredProcAsync(this DbCommand command,
            CommandBehavior commandBehaviour = CommandBehavior.Default,
            CancellationToken ct = default, bool manageConnection = true, params Action<SprocResults>[] resultActions)
        {
            if (resultActions == null)
            {
                throw new ArgumentNullException(nameof(resultActions));
            }

            using (command)
            {
                if (manageConnection && command.Connection.State == ConnectionState.Closed)
                    await command.Connection.OpenAsync(ct).ConfigureAwait(false);
                try
                {
                    using (var reader = await command.ExecuteReaderAsync(commandBehaviour, ct)
                        .ConfigureAwait(false))
                    {
                        var sprocResults = new SprocResults(reader);
                        
                        foreach (var t in resultActions)
                            t(sprocResults);
                    }
                }
                finally
                {
                    if (manageConnection)
                    {
                        command.Connection.Close();
                    }
                }
            }
        }*/

        /// <summary>
        /// Executes a non-query.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="manageConnection"></param>
        /// <returns></returns>
        public static int ExecuteStoredNonQuery(this DbCommand command, bool manageConnection = true)
        {
            var numberOfRecordsAffected = -1;

            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                {
                    command.Connection.Open();
                }

                try
                {
                    numberOfRecordsAffected = command.ExecuteNonQuery();
                }
                finally
                {
                    if (manageConnection)
                    {
                        command.Connection.Close();
                    }
                }
            }

            return numberOfRecordsAffected;
        }
        /*
        /// <summary>
        /// Executes a non-query asynchronously.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <param name="manageConnection"></param>
        /// <returns></returns>
        public static async Task<int> ExecuteStoredNonQueryAsync(this DbCommand command, CancellationToken ct = default,
            bool manageConnection = true)
        {
            var numberOfRecordsAffected = -1;

            using (command)
            {
                if (command.Connection.State == ConnectionState.Closed)
                {
                    await command.Connection.OpenAsync(ct).ConfigureAwait(false);
                }

                try
                {
                    numberOfRecordsAffected = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                }
                finally
                {
                    if (manageConnection)
                    {
                        command.Connection.Close();
                    }
                }
            }

            return numberOfRecordsAffected;
        }
        */
        private static object converterToEntity(Type entityType, object val)
        {
            object convertedVal = val;

            if (entityType.Equals(typeof(int)) && val.GetType().Equals(typeof(long)))
                convertedVal = Convert.ToInt32(val);
            if (entityType.Equals(typeof(uint)) && val.GetType().Equals(typeof(long)))
                convertedVal = Convert.ToUInt32(val);
            else if (entityType.Equals(typeof(char)) && val.GetType().Equals(typeof(string)))
                convertedVal = Convert.ToChar(val);
            else if (entityType.Equals(typeof(short)) && val.GetType().Equals(typeof(int)))
                convertedVal = Convert.ToInt16(val);
            else if (entityType.Equals(typeof(ushort)) && val.GetType().Equals(typeof(long)))
                convertedVal = Convert.ToUInt16(val);
            else if (entityType.Equals(typeof(ulong)) && val.GetType().Equals(typeof(decimal)))
                convertedVal = Convert.ToUInt64(val);

            else if (entityType.Equals(typeof(byte)) && val.GetType().Equals(typeof(sbyte[])))
                convertedVal = Convert.ToByte(((sbyte[])val)[0]);
            else if (entityType.Equals(typeof(sbyte)) && val.GetType().Equals(typeof(int)))
                convertedVal = Convert.ToSByte(val);

            else if (entityType.Equals(typeof(DateTimeOffset)) && val.GetType().Equals(typeof(TiberoTimeStampTZ)))
            {
                var converter = new TiberoDateTimeOffsetConverter();
                convertedVal = converter.ConvertFromProvider(val);
            }
            else if (entityType.Equals(typeof(DateTimeOffset)) && val.GetType().Equals(typeof(TiberoTimeStampLTZ)))
            {
                var converter = new TiberoDateTimeOffsetLocalConverter();
                convertedVal = converter.ConvertFromProvider(val);
            }

            else if (entityType.Equals(typeof(Guid)) && val.GetType().Equals(typeof(string)))
                convertedVal = new Guid(val.ToString());
            else if (entityType.Equals(typeof(bool)) && val.GetType().Equals(typeof(short)))
                convertedVal = ((short)val == 1) ? true : false;

            return convertedVal;
        }
        static string getString()
        {
            //Thread.Sleep(2000);
            return "success";
        }
    }
}
