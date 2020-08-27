using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Tibero.EntityFrameworkCore.Metadata.Internal;
using Tibero.EntityFrameworkCore.Utilities;
using Tibero.EntityFrameworkCore.Metadata;
namespace Tibero.EntityFrameworkCore.Design.Internal
{
    public class TiberoAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public TiberoAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema)
            {
                return string.Equals("SYSTEM", (string)annotation.Value);
            }
            if (annotation.Name == TiberoAnnotationNames.ValueGenerationStrategy)
            {
                return (TiberoValueGenerationStrategy)annotation.Value == TiberoValueGenerationStrategy.IdentityColumn;
            }

            return false;
        }

        

        
    }
}
