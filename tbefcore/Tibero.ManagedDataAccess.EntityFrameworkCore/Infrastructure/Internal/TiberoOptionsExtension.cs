#region License

// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Tibero.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
// ReSharper disable once CheckNamespace
namespace Tibero.EntityFrameworkCore.Infrastructure.Internal
{
    public class TiberoOptionsExtension : RelationalOptionsExtension
    {
        private long? _serviceProviderHash;
        private string _version;
        private string _logpath;
        private string _logFragment;
        readonly IDiagnosticsLogger<DbLoggerCategory.Infrastructure> _logger;
        public TiberoOptionsExtension()
        {
            
        }
       

        public TiberoOptionsExtension([NotNull] TiberoOptionsExtension copyFrom,
            IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger = null)
            : base(copyFrom)
        {
            _version = copyFrom._version;
            _logpath = copyFrom._logpath;
            _logger = logger;
            if (_logger != null && _logger.Logger != null)
            {
                _logger.Logger.LogInformation("TiberoOptionsExtension::Constructor");
            }

        }

        protected override RelationalOptionsExtension Clone() => new TiberoOptionsExtension(this);
        public override long GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                _serviceProviderHash = (base.GetServiceProviderHashCode() * 397);
            }

            return _serviceProviderHash.Value;
        }
        public override bool ApplyServices(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));
            
            services.AddEntityFrameworkTibero();

            return true;
        }
        /*
         * 추가할 옵션이 있다면 아래와 같이 처리하면된다. 
        public virtual SqlServerOptionsExtension WithRowNumberPaging(bool rowNumberPaging)
        {
            var clone = (SqlServerOptionsExtension)Clone();

            clone._rowNumberPaging = rowNumberPaging;

            return clone;
        }*/
        public virtual string Version
        {
            get
            {
                return _version;
            }
        }
        public virtual string LogPath
        {
            get
            {
                return _logpath;
            }
        }
        public virtual TiberoOptionsExtension withVersion(string version)
        {
            var clone = (TiberoOptionsExtension)Clone();
            clone._version = version;
            return clone;
        }
        public virtual TiberoOptionsExtension withLogPath(string logpath)
        {
            var clone = (TiberoOptionsExtension)Clone();
            clone._logpath = logpath;
            return clone;
        }
        public override string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    builder.Append(base.LogFragment);

                    builder.Append("TiberoProviderCore2.1");

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }

    }
}
