
using System;
using System.Collections.Generic;
using System.Net.Security;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Tibero.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.EntityFrameworkCore.Diagnostics;
// ReSharper disable once CheckNamespace
namespace Tibero.EntityFrameworkCore.Infrastructure
{

    public class TiberoDbContextOptionsBuilder
        : RelationalDbContextOptionsBuilder<TiberoDbContextOptionsBuilder, TiberoOptionsExtension>
    {
        DbContextOptionsBuilder _optionsBuilder;
        public static ILoggerFactory _loggerFactory = new LoggerFactory();
        public static bool hasLogger = false;
        readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;

        public TiberoDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder,
            IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger = null)
            : base(optionsBuilder)
        {
            _optionsBuilder = optionsBuilder;
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoDbContextOptionsBuilder::Constructor");
            }
        }
        /*
         * ���⼭ ���� ����ڰ� ȣ���� option setting method�� �ۼ��ϰ� TiberoOptionsExtension�� �� extension option�鿡 ���� ������ �۾��ϸ� �ȴ�. ���÷� �ϳ��� �۾��صε��� �Ѵ�.
         */
        public virtual void Version(string version)
        {
            this.WithOption(e => e.withVersion(version));
        }
        public void LogPath(string logpath)
        {
            
            this.WithOption(e => e.withLogPath(logpath));
            /* ���⼭ optionbuilder�� loggerFatcory ����� �ָ�, Entity Framework���� ��ϵ� loggerFactory �� service�� �Ѱ��ټ� ����. */
            if (hasLogger == false)
            {
                _loggerFactory.AddSerilog(new LoggerConfiguration().MinimumLevel.Information().WriteTo.File(logpath).CreateLogger());
                hasLogger = true;
            }
            _optionsBuilder.UseLoggerFactory(_loggerFactory).EnableSensitiveDataLogging();

        }
        public void LogPath(string path, string logName)
        {

            string logpath = path + "\\" + logName;
            this.WithOption(e => e.withLogPath(logpath));

            if (hasLogger == false)
            {
                _loggerFactory.AddSerilog(new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(logpath).CreateLogger());
                hasLogger = true;
            }
            _optionsBuilder.UseLoggerFactory(_loggerFactory).EnableSensitiveDataLogging();

        }

    }
}
