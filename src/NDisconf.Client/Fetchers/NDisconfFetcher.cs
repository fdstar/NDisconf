using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NDisconf.Core.Entities;
using Polly;
using RestSharp;

namespace NDisconf.Client.Fetchers
{
    /// <summary>
    /// NDisconf下的Fetcher实现类
    /// </summary>
    public class NDisconfFetcher : BaseFetcher, IFetcher
    {
        const string GetConfigResource = "/Api/GetConfig";
        const string GetAllConfigsResource = "/Api/GetConfigs";
        const string GetZkHostsResource = "/Api/GetZookeeperHost";
        const string GetLastChangedTimeResource = "/Api/GetLastChangedTime";
        /// <summary>
        /// 根据配置进行实例化
        /// </summary>
        /// <param name="setting"></param>
        public NDisconfFetcher(NDisconfSetting setting)
            : base(setting)
        {
        }
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
            return this.CallApi<DateTime>(GetLastChangedTimeResource, async r =>
            {
                var response = await this._client.ExecuteTaskAsync(r).ConfigureAwait(false);
                return DateTime.Parse(response.Content);
            }, filter);
        }
        /// <summary>
        /// 获取Zookeeper服务路径
        /// </summary>
        /// <returns></returns>
        public Task<string> GetZkHosts()
        {
            throw new NotImplementedException();
        }
        private async Task<T> CallApi<T>(string resource, Func<RestRequest, Task<T>> func, object param = null)
        {
            RestRequest request = new RestRequest(resource, Method.POST);
            request.AddJsonBody(param);
            return await this._policy.Execute(() => func(request)).ConfigureAwait(false);
        }
    }
}
