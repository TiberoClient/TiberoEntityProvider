using System;
using System.Collections.Generic;
using System.Text;

namespace Tibero.EntityFrameworkCore.Metadata.Internal
{
    public static class TiberoAnnotationNames
    {
        public const string Prefix = "Tibero:";
        public const string Identity = Prefix + "Identity";
        public const string ValueGenerationStrategy = Prefix + "ValueGenerationStrategy";
        public const string HiLoSequenceName = Prefix + "HiLoSequenceName";
        public const string HiLoSequenceSchema = Prefix + "HiLoSequenceSchema";
    }
}
