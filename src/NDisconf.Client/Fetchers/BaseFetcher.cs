using Polly;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NDisconf.Client.Fetchers
{
    /// <summary>
    /// 读取配置基础类
    /// </summary>
    public abstract class BaseFetcher
    {
        /// <summary>
        /// 重试策略
        /// </summary>
        protected Policy _policy;
        /// <summary>
        /// 配置信息
        /// </summary>
        protected NDisconfSetting _setting;
        /// <summary>
        /// http请求客户端
        /// </summary>
        protected RestClient _client;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="setting"></param>
        public BaseFetcher(NDisconfSetting setting)
        {
            this._setting = setting;
            var strategy = this._setting.UpdateStrategy;
            this._policy = Policy.Handle<Exception>().WaitAndRetry(strategy.RetryTimes, i => TimeSpan.FromSeconds(strategy.RetryIntervalSeconds), (e, t) =>
            {
                //TODO:log
            });
            this._client = new RestClient(this._setting.WebApiHost);
        }
        /// <summary>
        /// 向服务端发起http请求，并返回响应的响应
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <param name="func"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        protected virtual async Task<T> CallApi<T>(string resource, Func<RestRequest, Task<T>> func, object param = null)
        {
            RestRequest request = new RestRequest(resource, Method.POST);
            request.AddJsonBody(param);
            return await this._policy.Execute(() => func(request)).ConfigureAwait(false);
        }
    }
}
