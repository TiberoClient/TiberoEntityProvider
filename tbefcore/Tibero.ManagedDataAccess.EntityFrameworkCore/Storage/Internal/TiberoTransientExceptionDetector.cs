using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Tibero.DataAccess.Client;
namespace Tibero.EntityFrameworkCore.Storage.Internal
{
    public static class TiberoTransientExceptionDetector
    {
        public static bool ShouldRetryOn([NotNull] Exception ex)
        {
            if (ex is TiberoException tiberoException)
            {
                /* 서버의 transient 에러에 대해 retry 하라는데 일단 나중에 알게 되면 에러 detect 목록에 추가하자. 
                 */
                return false;
            }
            if(ex is TimeoutException)
            {
                return true;
            }
            return false;
        }
    }
}
