using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Migrations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Tibero.EntityFrameworkCore.Migration
{
    public class TiberoMigrationCommandExecutor : MigrationCommandExecutor
    {
        public override void ExecuteNonQuery(
               IEnumerable<MigrationCommand> migrationCommands,
               IRelationalConnection connection)
        {
            Check.NotNull(migrationCommands, nameof(migrationCommands));
            Check.NotNull(connection, nameof(connection));

            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                connection.Open();

                try
                {
                    IDbContextTransaction transaction = null;

                    try
                    {
                        foreach (var command in migrationCommands)
                        {
                            if (transaction == null
                                && !command.TransactionSuppressed)
                            {
                                transaction = connection.BeginTransaction();
                            }

                            if (transaction != null
                                && command.TransactionSuppressed)
                            {
                                transaction.Commit();
                                transaction.Dispose();
                                transaction = null;
                            }
                            try
                            {
                                command.ExecuteNonQuery(connection);
                            }
                            catch (Exception e)
                            {
                                if (e.Message.Contains("ODP-7206"))
                                {
                                    continue;
                                    /* Already NOT NULL OR NULL  해당 에러는 Throw 하지 않음.*/
                                }
                                else throw;
                            }
                        }

                        transaction?.Commit();
                    }
                   
                    finally
                    {
                        transaction?.Dispose();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        public override async Task ExecuteNonQueryAsync(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(migrationCommands, nameof(migrationCommands));
            Check.NotNull(connection, nameof(connection));

            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                await connection.OpenAsync(cancellationToken);

                try
                {
                    IDbContextTransaction transaction = null;

                    try
                    {
                        foreach (var command in migrationCommands)
                        {
                            if (transaction == null
                                && !command.TransactionSuppressed)
                            {
                                transaction = await connection.BeginTransactionAsync(cancellationToken);
                            }

                            if (transaction != null
                                && command.TransactionSuppressed)
                            {
                                transaction.Commit();
                                transaction.Dispose();
                                transaction = null;
                            }
                            try
                            {
                                await command.ExecuteNonQueryAsync(connection, cancellationToken: cancellationToken);
                            }
                            catch (Exception e)
                            {
                                if (e.Message.Contains("ODP-7206"))
                                {
                                    continue;
                                    /* Already NOT NULL OR NULL  해당 에러는 Throw 하지 않음.*/
                                }
                                else throw;
                            }
                        }

                        transaction?.Commit();
                    }
                    
                    finally
                    {
                        transaction?.Dispose();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

    }
}
