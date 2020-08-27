using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;
using Tibero.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
namespace Tibero.EntityFrameworkCore.Update.Internal
{
   

    public class TiberoModificationCommandBatch : ReaderModificationCommandBatch
    {
        private int _parameterCount = 1;
        private const int MaxParameterCount = 1000;
        private readonly int _maxBatchSize;
        private readonly StringBuilder _varInPSM;
        private int _cursorIndex = 0;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerateHelper;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Update> _logger;
        public TiberoModificationCommandBatch(
          [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
          [NotNull] ISqlGenerationHelper sqlGenerationHelper,
          [NotNull] IUpdateSqlGenerator updateSqlGenerator,
          [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
          int? maxBatchSize,
          IDiagnosticsLogger<DbLoggerCategory.Update> logger = null)
          : base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
        {
            this._commandBuilderFactory = commandBuilderFactory;
            this._varInPSM = new StringBuilder();
            this._maxBatchSize = Math.Min(maxBatchSize ?? int.MaxValue, 200);
            this._sqlGenerateHelper = sqlGenerationHelper;
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::Constructor");
            }
        }
        
        

      
        protected override RawSqlCommand CreateStoreCommand()
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::CreateStoreCommand");
            }
            var commandBuilder = _commandBuilderFactory
                .Create()
                .Append(GetCommandText());

            var parameterValues = new Dictionary<string, object>(GetParameterCount());
            
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var commandIndex = 0; commandIndex < ModificationCommands.Count; commandIndex++)
            {
                var command = ModificationCommands[commandIndex];
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var columnIndex = 0; columnIndex < command.ColumnModifications.Count; columnIndex++)
                {
                    var columnModification = command.ColumnModifications[columnIndex];
                    if (columnModification.UseCurrentValueParameter)
                    {
                        commandBuilder.AddParameter(
                            columnModification.ParameterName,
                            _sqlGenerateHelper.GenerateParameterName(columnModification.ParameterName),
                            columnModification.Property);

                        parameterValues.Add(columnModification.ParameterName, columnModification.Value);
                    }

                    if (columnModification.UseOriginalValueParameter)
                    {
                         commandBuilder.AddParameter(
                          columnModification.OriginalParameterName,
                        _sqlGenerateHelper.GenerateParameterName(columnModification.OriginalParameterName),
                        columnModification.Property);

                        parameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);
                    }
                }
            }
            for (var generateCursor = 0; generateCursor < _cursorIndex ; generateCursor++)
            {
                string cursor = string.Format("C{0}", generateCursor);
                commandBuilder.AddRawParameter(cursor, (DbParameter)new TiberoParameter(cursor, TiberoDbType.RefCursor, (object)DBNull.Value, ParameterDirection.Output));
            }
            
            return new RawSqlCommand(commandBuilder.Build(), parameterValues);
        }

        protected new virtual ITiberoUpdateSqlGenerator UpdateSqlGenerator => (ITiberoUpdateSqlGenerator)base.UpdateSqlGenerator;

        protected override string GetCommandText()
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::GetCommandText");
            }
            

            StringBuilder BuilderForPSM = new StringBuilder();
            var c = base.GetCommandText();

            BuilderForPSM.AppendLine("DECLARE");
            _varInPSM.AppendLine(" v_RowCount INTEGER;");
            BuilderForPSM.Append(_varInPSM.ToString());
            BuilderForPSM.AppendLine("BEGIN");
            BuilderForPSM.AppendLine(c);
            BuilderForPSM.AppendLine("END;");
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogDebug("TiberoModificationCommandBatch::GetCommandText : [" + BuilderForPSM.ToString() +"]");
            }
            return BuilderForPSM.ToString();
        }

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::CanAddCommand");
            }
            if (this.ModificationCommands.Count >= this._maxBatchSize)
                return false;

            var additionalParameterCount = CountParameters(modificationCommand);

            if (_parameterCount + additionalParameterCount >= MaxParameterCount)
                return false;
            _parameterCount += additionalParameterCount;
                       
            return true;
        }
        
        protected override void UpdateCachedCommandText(int commandPosition)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::UpdateCachedCommandText");
            }
            var newModificationCommand = ModificationCommands[commandPosition];

            switch(newModificationCommand.EntityState)
            {
                case EntityState.Added:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendInsertOperationWithCursor(CachedCommandText, newModificationCommand, commandPosition, _varInPSM, ref _cursorIndex);
                    break;
                case EntityState.Modified:
                    CommandResultSet[commandPosition] =
                      UpdateSqlGenerator.AppendUpdateOperationWithCursor(CachedCommandText, newModificationCommand, commandPosition, _varInPSM, ref _cursorIndex);
                    break;
                case EntityState.Deleted:
                    CommandResultSet[commandPosition] =
                     UpdateSqlGenerator.AppendDeleteOperationWithCursor(CachedCommandText, newModificationCommand, commandPosition, _varInPSM, ref _cursorIndex);
                    break;
            }
            LastCachedCommandIndex = commandPosition;
        }
        

        protected override void ResetCommandText()
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::ResetCommandText");
            }
            /* 아래는 base.ResetCommandText()
             * 새로운 CachedCommandText 를 만들고 SqlGenerator 의 AppendBatchHeader 를 호출함. 
             * 그곳에서는 batch를 위해 들어가야할 내용들이 적힘.
             protected virtual void ResetCommandText()
             {
                CachedCommandText = new StringBuilder();
                UpdateSqlGenerator.AppendBatchHeader(CachedCommandText);
                LastCachedCommandIndex = -1;
             }*/
            base.ResetCommandText();
            this._cursorIndex = 0;
            this._varInPSM.Clear();
        }
        
        protected override bool IsCommandTextValid()
        {
            return true;
        }
        
        protected override int GetParameterCount()
        {
            return this._parameterCount;
        }

        private static int CountParameters(ModificationCommand modificationCommand)
        {

            int parametrCount = 0;
            foreach (ColumnModification columnModification in (IEnumerable<ColumnModification>)modificationCommand.ColumnModifications)
            {
                if (columnModification.UseCurrentValueParameter)
                    ++parametrCount;
                if (columnModification.UseOriginalValueParameter)
                    ++parametrCount;
            }
            return parametrCount;
        }

        
        protected override void Consume(RelationalDataReader relationalReader)
        {
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::Consume");
            }
            int commandIndex;
            /*
             *  INSERT : outparam 이 있으면 cursor open 하고 resultsetmapping = LASTRESULTSET 
             *           없으면, NORESULT 이고 cursor 없음.
             *  DELETE, UPDATE : outparam 있던 없던 cursor open 하고 LASTRESULTSET. 
             *  
             *  여기서 NEXTRESULTSET 은 NORESULTSET 이 아닌 경우에만 for loop 안에서 한번 증가시켜줘야한다. 그 이유는 NORESULTSET 인 경우 result set이 해당 modification command 의 것이 아님.
             *  
             *  propagate 가 필요한 경우는, out param 이 있는 경우라고 보면된다. 따라서 noresult set 이면 안되고, 읽을 데이터가 있어야한다. 
             *  없으면 throw.
             *  
             *  propagate 가 필요없으면, delete, update 에 대한 row 반영수만 체크하는 로직이다. 따라서 NORESULTSET 이 아닌 경우에 대해서만(INSERT WITHOUT OUT 제외) affected row count 가 1인지 체크
             *  
             *  나중에라도, 한번에 table 로 insert batch 에 대한 결과를 받아오게 된다면, NOTLASTRESULTSET 도 존재하게 될 것 같다. 
             *  commandresultset state 는 command와 1대1 대응이 되므로, NOTLASTRESULTSET 인 경우에는 data reader 의 read를 한번씩 하면서 propagate를 하면 될 것 같다. 
             *  
             *  
             */ 
            for (commandIndex=0 ; commandIndex < this.CommandResultSet.Count; commandIndex++)
            {
                if (this.ModificationCommands[commandIndex].RequiresResultPropagation)
                {
                    if(CommandResultSet[commandIndex] != ResultSetMapping.NoResultSet && relationalReader.Read())
                    {
                        IRelationalValueBufferFactory valueBufferFactory = this.CreateValueBufferFactory(ModificationCommands[commandIndex].ColumnModifications);
                        ModificationCommands[commandIndex].PropagateResults(valueBufferFactory.Create(relationalReader.DbDataReader));
                    }
                    else
                        throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0), this.ModificationCommands[commandIndex].Entries);

                }
                else
                {
                    if(CommandResultSet[commandIndex] != ResultSetMapping.NoResultSet)
                    {
                        if(!relationalReader.Read())
                            throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0), this.ModificationCommands[commandIndex].Entries);
                        else
                        {
                            int num = relationalReader.DbDataReader.GetInt32(0);
                            if(num != 1)
                                throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, (object)num), this.ModificationCommands[commandIndex].Entries);

                        }

                    }
                }
                if(CommandResultSet[commandIndex] != ResultSetMapping.NoResultSet)
                    relationalReader.DbDataReader.NextResult();
            }
            Debug.Assert(!relationalReader.DbDataReader.NextResult(), "Too Many returend resultset in modification");
            

        }

        protected override async Task ConsumeAsync(
          RelationalDataReader relationalReader,
          CancellationToken cancellationToken = default(CancellationToken))
        {
            /* await & async 구문은 비동기 작업에 사용되는 방식이다. 
             * async Task (Task<T>) 를 반환 타입으로 하며 함수이름이 Async 로 끝나면 비동기 task 를 만들수 있다. 
             * 비동기 task를 생성하기로 호출하는 Thread에서는 두가지 일수 있다.
             * 이 task 를 await 로 생성하게 되면 task를 생성하려고하는 thread가 아래 소스중 await 까지 직접 돌고 await 를 만나면서 하위 task 에 대한 생성까지만 전담하여 
             * task를 반환한다. 이 경우, async 긴 하지만 task 생성시까지는 해당 thread 가 책임진다. 근데 이 task 생성하는 곳이 해당하는 함수 또한 task 라면 await 으로 기다리고 있을 것이라서 
             * 더 상위로 반환.
             * task 를 생성하는 곳이 task 함수가 아니라면 그냥 바로 async 하게 도는 것이라서 이 함수부터 기다리지 않고 task 반환받자마자 해당 함수 하위 로직 시작. 
             */
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoModificationCommandBatch::ConsumeAsync");
            }
            int commandIndex;
            
            for (commandIndex = 0; commandIndex < this.CommandResultSet.Count; commandIndex++)
            {
                if (this.ModificationCommands[commandIndex].RequiresResultPropagation)
                {
                    if (CommandResultSet[commandIndex] != ResultSetMapping.NoResultSet && await relationalReader.ReadAsync())
                    {
                        IRelationalValueBufferFactory valueBufferFactory = this.CreateValueBufferFactory(ModificationCommands[commandIndex].ColumnModifications);
                        ModificationCommands[commandIndex].PropagateResults(valueBufferFactory.Create(relationalReader.DbDataReader));
                    }
                    else
                        throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0), this.ModificationCommands[commandIndex].Entries);

                }
                else
                {
                    if (CommandResultSet[commandIndex] != ResultSetMapping.NoResultSet)
                    {
                        if (!await relationalReader.ReadAsync())
                            throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, 0), this.ModificationCommands[commandIndex].Entries);
                        else
                        {
                            int num = relationalReader.DbDataReader.GetInt32(0);
                            if (num != 1)
                                throw new DbUpdateConcurrencyException(RelationalStrings.UpdateConcurrencyException(1, (object)num), this.ModificationCommands[commandIndex].Entries);

                        }

                    }
                }
                if (CommandResultSet[commandIndex] != ResultSetMapping.NoResultSet)
                    await relationalReader.DbDataReader.NextResultAsync();
            }
            Debug.Assert(!await relationalReader.DbDataReader.NextResultAsync(), "Too Many returend resultset in modification");
            
        }
    }
}
