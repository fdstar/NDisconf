using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// 文件更新规则
    /// </summary>
    public class FileRule : Rule, IFileRule
    {
        /// <summary>
        /// 配置文件更新规则
        /// </summary>
        /// <param name="configName"></param>
        public FileRule(string configName) :
            base(configName)
        {
#if !NETSTANDARD2_0
            this.MapTo(configName);
#endif
        }
        /// <summary>
        /// 文件变更通知
        /// </summary>
        internal event FileChanged Changed;
        /// <summary>
        /// 当文件下载完成并且替换本地对应文件后回调，注意此处将采用委托链的方式，即多次调用均会被执行
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IFileRule OnChanged(FileChanged action)
        {
            this.Changed += action;
            return this;
        }
        /// <summary>
        /// 当配置的值发生变化时，通知值变更
        /// </summary>
        /// <param name="changedValue"></param>
        public override void ConfigChanged(string changedValue)
        {
#if !NETSTANDARD2_0
            if (this.AutoRefresh)
            {
                System.Configuration.ConfigurationManager.RefreshSection(this.SectionName);
            }
#endif
            this.Changed?.Invoke(
#if NETSTANDARD2_0
                this.ConfigName
#else
                this.SectionName
#endif
                );
        }
#if !NETSTANDARD2_0
        /// <summary>
        /// 是否自动刷新，默认自动刷新
        /// </summary>
        public bool AutoRefresh { get; set; } = true;
        /// <summary>
        /// 要刷新的节点名称
        /// </summary>
        public string SectionName { get; private set; }
        /// <summary>
        /// 不自动调用ConfigurationManager.RefreshSection方法更新配置
        /// </summary>
        /// <returns></returns>
        public IFileRule RefreshIgnores()
        {
            this.AutoRefresh = false;
            return this;
        }
        /// <summary>
        /// 注册Rule规则，设置默认的文件配置映射
        /// </summary>
        /// <param name="refreshSectionName">更新回调时ConfigurationManager.RefreshSection要刷新的节点名称，默认采用远程配置的configName</param>
        /// <returns></returns>
        public IFileRule MapTo(string refreshSectionName)
        {
            if (!string.IsNullOrWhiteSpace(refreshSectionName))
            {
                this.SectionName = Path.GetFileNameWithoutExtension(refreshSectionName);
            }
            return this;
        }
#endif
    }
}
