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
        /// 配置节点名称
        /// </summary>
        string ConfigName { get; }
        /// <summary>
        /// 当配置的值发生变化时，通知值变更
        /// </summary>
        /// <param name="changedValue"></param>
        void ConfigChanged(string changedValue);
    }
}
