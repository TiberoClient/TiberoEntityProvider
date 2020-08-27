using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Storage.Internal;
namespace Tibero.EntityFrameworkCore.Migration
{
    public class TiberoMigrationCommandListBuilder : MigrationCommandListBuilder
    {
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;
        private readonly List<TiberoMigrationCommand> _commands = new List<TiberoMigrationCommand>();

        public IRelationalCommandBuilder _commandBuilder;
        public TiberoMigrationCommandListBuilder([NotNull] IRelationalCommandBuilderFactory commandBuilderFactory) :base(commandBuilderFactory)
        {
            Check.NotNull(commandBuilderFactory, nameof(commandBuilderFactory));

            _commandBuilderFactory = commandBuilderFactory;
            _commandBuilder = commandBuilderFactory.Create();
        }
        public override IReadOnlyList<MigrationCommand> GetCommandList() => _commands;
        public override MigrationCommandListBuilder EndCommand(bool suppressTransaction = false)
        {
            if (_commandBuilder.GetLength() != 0)
            {
                _commands.Add(new TiberoMigrationCommand(_commandBuilder.Build(), suppressTransaction));
                _commandBuilder = _commandBuilderFactory.Create();
            }

            return this;
        }
        


    }
}
