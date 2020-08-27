
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
         * 여기서 실제 사용자가 호출할 option setting method를 작성하고 TiberoOptionsExtension에 그 extension option들에 대한 내용을 작업하면 된다. 예시로 하나만 작업해두도록 한다.
         */
        public virtual void Version(string version)
        {
            this.WithOption(e => e.withVersion(version));
        }
        public void LogPath(string logpath)
        {
            
            this.WithOption(e => e.withLogPath(logpath));
            /* 여기서 optionbuilder에 loggerFatcory 등록해 주면, Entity Framework에서 등록된 loggerFactory 를 service에 넘겨줄수 있음. */
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
