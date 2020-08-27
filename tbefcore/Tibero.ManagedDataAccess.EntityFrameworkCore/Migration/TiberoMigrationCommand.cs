using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Utilities;

namespace Tibero.EntityFrameworkCore.Migration
{
    public class TiberoMigrationCommand : MigrationCommand
    {
        private readonly IRelationalCommand _relationalCommand;

        public TiberoMigrationCommand(
           [NotNull] IRelationalCommand relationalCommand,
           bool transactionSuppressed = false): base(relationalCommand, transactionSuppressed)
        {
            Check.NotNull(relationalCommand, nameof(relationalCommand));

            _relationalCommand = relationalCommand;
            TransactionSuppressed = transactionSuppressed;
        }
        public override bool TransactionSuppressed { get; }
             
    }
}
