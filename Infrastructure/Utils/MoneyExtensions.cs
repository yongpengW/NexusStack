using System;
using System.Collections.Generic;
using System.Text;

namespace NexusStack.Infrastructure.Utils
{
    /// <summary>
    /// 处理金额扩展方法
    /// </summary>
    public static class MoneyExtensions
    {
        /// <summary>
        /// 金额四舍五入 向上取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ToRoundUp(this decimal value)
        {
            return Math.Ceiling(value * 100) / 100;
        }

        /// <summary>
        /// 金额四舍五入 向下取整
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ToRoundDown(this decimal value)
        {
            return Math.Floor(value * 100) / 100;
        }
    }
}
