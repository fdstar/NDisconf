using System;
using System.Collections.Generic;
using System.Text;

namespace NDisconf.Client.Rules
{
    /// <summary>
    /// 文件变更委托
    /// </summary>
    /// <param name="configName"></param>
    public delegate void FileChanged(string configName);
    /// <summary>
    /// 文件更新规则
    /// </summary>
    public interface IFileRule : IRule
    {
        /// <summary>
        /// 当文件下载完成并且替换本地对应文件后回调，注意此处将采用委托链的方式，即多次调用均会被执行
        /// </summary>
#if NETSTANDARD2_0
        /// <param name="action">Action参数为对应的配置文件名</param>
#else
        /// <param name="action">Action参数为对应要刷新的配置节点名</param>
#endif
        /// <returns></returns>
        IFileRule OnChanged(FileChanged action);

#if !NETSTANDARD2_0
        /// <summary>
        /// 注册Rule规则，设置默认的文件配置映射
        /// </summary>
        /// <param name="refreshSectionName">更新回调时ConfigurationManager.RefreshSection要刷新的节点名称，默认采用远程配置的configName</param>
        /// <returns></returns>
        IFileRule MapTo(string refreshSectionName);
        /// <summary>
        /// 不自动调用ConfigurationManager.RefreshSection方法更新配置
        /// </summary>
        /// <returns></returns>
        IFileRule RefreshIgnores();
#endif
    }
}
