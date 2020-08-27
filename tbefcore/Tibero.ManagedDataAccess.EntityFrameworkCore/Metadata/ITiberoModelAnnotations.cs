using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace Tibero.EntityFrameworkCore.Metadata
{
   
    public interface ITiberoModelAnnotations : IRelationalModelAnnotations
    {
       
        TiberoValueGenerationStrategy? ValueGenerationStrategy { get; }

      
        string HiLoSequenceName { get; }

        string HiLoSequenceSchema { get; }
    }
}