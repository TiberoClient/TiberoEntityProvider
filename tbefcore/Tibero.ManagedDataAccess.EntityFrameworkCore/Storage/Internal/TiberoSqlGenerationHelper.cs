
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using System;


namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public class TiberoSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        bool _isSetNoQuote;
        static TiberoSqlGenerationHelper()
        {
            

        }

        public TiberoSqlGenerationHelper([NotNull] RelationalSqlGenerationHelperDependencies dependencies)
            : base(dependencies) {
            _isSetNoQuote = GetQuoteEnv();
        }

        public bool GetQuoteEnv()
        {
            string env = Environment.GetEnvironmentVariable("EF_NOQUOTE");
           
            if (env == null)
                return false;
            else if (env == "YES")
                return true;

            return false;
        }
        public override string GenerateParameterName(string name)
        {
            /*
            Console.WriteLine("generateparamtername");
            //name.Append("abc");
            string after_name = base.GenerateParameterName(name);
            Console.WriteLine("after_name :" + after_name);
            return after_name;*/
            return ":" + name;
        }
        public override void GenerateParameterName(StringBuilder builder, string name)
        {
            
            builder.Append(":");
            builder.Append(name);
        }
        public override string DelimitIdentifier(string identifier)
        {
            if (RequiresQuoting(identifier) && _isSetNoQuote != true)
                return base.DelimitIdentifier(identifier);
            else
                return identifier;
        }
          

        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            
            if (RequiresQuoting(identifier) && _isSetNoQuote != true )
                base.DelimitIdentifier(builder, identifier);
            else
                builder.Append(identifier);
        }

        /// <summary>
        /// Returns whether the given string can be used as an unquoted identifier in PostgreSQL, without quotes.
        /// </summary>
        static bool RequiresQuoting(string identifier)
        {
            var first = identifier[0];
            if (!char.IsLower(first) && first != '_')
                return true;

            for (var i = 1; i < identifier.Length; i++)
            {
                var c = identifier[i];

                if (char.IsLower(c))
                    continue;

                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '_':
                    case '$':  // yes it's true
                        continue;
                }

                return true;
            }

            //if (ReservedWords.Contains(identifier.ToUpperInvariant()))
             //   return true;

            return false;
        }
    }
}