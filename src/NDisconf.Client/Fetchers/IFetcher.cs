using NDisconf.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NDisconf.Client.Fetchers
{
    /// <summary>
    /// 数据抓取接口，用于定义从Web服务器获取数据和文件
    /// </summary>
    public interface IFetcher
    {
        /// <summary>
        /// 根据键值获取对应的配置内容
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<string> GetConfig(ConfigFetchFilter filter);
        /// <summary>
        /// 批量获取所有的配置信息
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IDictionary<ConfigType, IDictionary<string, string>>> GetAllConfigs(FetchFilter filter);
        /// <summary>
        /// 获取Zookeeper服务路径
        /// </summary>
        /// <returns></returns>
        string GetZkHosts();
        /// <summary>
        /// 获取指定应用的最后一次更新时间
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<DateTime> GetLastChangedTime(FetchFilter filter);
    }
}
