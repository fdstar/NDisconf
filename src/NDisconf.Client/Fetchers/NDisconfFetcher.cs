using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NDisconf.Core.Entities;

namespace NDisconf.Client.Fetchers
{
    /// <summary>
    /// NDisconf下的Fetcher实现类
    /// </summary>
    public class NDisconfFetcher : IFetcher
    {
        /// <summary>
        /// 批量获取所有的配置信息
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<IDictionary<ConfigType, IDictionary<string, string>>> GetAllConfigs(FetchFilter filter)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 根据键值获取对应的配置内容
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<string> GetConfig(ConfigFetchFilter filter)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取指定应用的最后一次更新时间
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Task<DateTime> GetLastChangedTime(FetchFilter filter)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取Zookeeper服务路径
        /// </summary>
        /// <returns></returns>
        public Task<string> GetZkHosts()
        {
            throw new NotImplementedException();
        }
    }
}
