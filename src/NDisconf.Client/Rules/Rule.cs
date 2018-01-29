using System;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// 基础规则
    /// </summary>
    public abstract class Rule : IRule
    {
        /// <summary>
        /// 配置映射规则
        /// </summary>
        /// <param name="configName"></param>
        public Rule(string configName)
        {
            if (string.IsNullOrWhiteSpace(configName))
            {
                throw new ArgumentNullException("configName can not be null.");
            }
            this.ConfigName = configName;
        }
        /// <summary>
        /// 当前对应的配置节点名称
        /// </summary>
        public string ConfigName { get; private set; }
        /// <summary>
        /// 当配置的值发生变化时，通知值变更
        /// </summary>
        /// <param name="changedValue"></param>
        public abstract void ConfigChanged(string changedValue);
    }
}
