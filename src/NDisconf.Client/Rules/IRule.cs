using System;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// 映射规则
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// 注册Rule规则，设置默认的属性映射
        /// </summary>
        /// <param name="configName">默认采用远程的configName</param>
        /// <returns></returns>
        IRule MapTo(string configName);
        /// <summary>
        /// 当配置的值发生变化时，通知值变更
        /// </summary>
        /// <param name="changedValue"></param>
        void ConfigChanged(string changedValue);
    }
}
