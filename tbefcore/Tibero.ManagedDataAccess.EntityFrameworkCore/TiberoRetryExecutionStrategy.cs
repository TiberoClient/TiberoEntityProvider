using System;
using System.Collections.Generic;
using Tibero.DataAccess.Client;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
namespace Tibero.EntityFrameworkCore
{

    /* provider에서는 executionStrategy를 찾을 때, service 에 등록해놓은 애를 사용할 것임. 기본 retry 는 false
     * 여기 소스는, MS 에서 따로 사용자가 직접 strategy 를 받아서 excute 등에 사용할 수 있는 방법이 따로 있는 것 같고
     * 그 때 retry 가능한 형태로 반환하고자 하는 것 같음.
     * 일단 Tibero Provider에서는 따로 option 지원 및 retryingExcutionStrategy 는 지원 없음. 껍데기만 존재.
     */ 
    public class TiberoRetryingExecutionStrategy : ExecutionStrategy
    {
        public TiberoRetryingExecutionStrategy(
            [NotNull] DbContext context,
            int maxRetryCount,
            TimeSpan maxRetryDelay,
            [CanBeNull] ICollection<int> errorNumbersToAdd)
            : base(
                context,
                maxRetryCount,
                maxRetryDelay)
        { }
        protected override bool ShouldRetryOn(Exception exception)
        {


            return false;
        }
    }
}
