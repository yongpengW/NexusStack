using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure
{
    /// <summary>
    /// 所有的EFCore 的Repository Service 都需要继承此接口,便于通过反射进行注入，以及Redis接口和服务
    /// </summary>
    public interface IScopedDependency
    {

    }
}
