using Polly;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
