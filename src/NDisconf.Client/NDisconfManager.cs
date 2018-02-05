using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Polly;
using NDisconf.Core.Zookeepers;
using NDisconf.Client.Fetchers;
using NDisconf.Core.Entities;
using NDisconf.Client.Preservations;
using System.Linq;
using NDisconf.Client.Rules;
#if NETSTANDARD2_0
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#endif

namespace NDisconf.Client
{
    /// <summary>
    /// NDisconf客户端配置管理类
    /// </summary>
    public class NDisconfManager
    {
        /// <summary>
        /// NDisconf的日志前缀
        /// </summary>
        public const string LOGPREFIX = "NDisconfLog";
        private NDisconfSetting _setting;
        private Policy _fallBackPolicy;
        private NodeWatcher _watcher;
        private IFetcher _fetcher;
        private IPreservation[] _preservations;
        /// <summary>
        /// 单例
        /// </summary>
        private static readonly NDisconfManager Instance = new NDisconfManager();
        /// <summary>
        /// 文件更新规则
        /// </summary>
        public readonly RuleCollection<FileRule> FileRules = new RuleCollection<FileRule>(c => new FileRule(c));
        /// <summary>
        /// 键值对更新规则
        /// </summary>
        public readonly RuleCollection<ItemRule> ItemRules = new RuleCollection<ItemRule>(c => new ItemRule(c));
        private NDisconfManager()
        {
            _fallBackPolicy = Policy.Handle<Exception>().Fallback(() => { }, e =>
            {
                //TODO:log
            });
        }
#if NETSTANDARD2_0
        /// <summary>
        /// NDisconf初始化注册
        /// </summary>
        /// <param name="configPath">json配置文件路径</param>
        /// <param name="act">注册执行的委托</param>
        public static Task Register(string configPath = "configs/ndisconf.json", Action<NDisconfManager> act = null)
        {
            act?.Invoke(Instance);
            return Instance.Init(configPath);
        }
#else
        /// <summary>
        /// NDisconf初始化注册
        /// </summary>
        /// <param name="sectionName">配置节点名称</param>
        /// <param name="act">注册执行的委托</param>
        public static Task Register(string sectionName = "ndisconf", Action<NDisconfManager> act = null)
        {
            act?.Invoke(Instance);
            return Instance.Init(sectionName);
        }
#endif
        /// <summary>
        /// NDisconf初始化
        /// 该方法理应在所有代码执行之前就被调用，否则可能会出现配置调用顺序错误
        /// </summary>
        /// <param name="path"></param>
        private async Task Init(string path)
        {
            this._setting = this.GetSetting(path);
            this._fetcher = FetcherFactory.GetFetcher(this._setting);
            this._preservations = new IPreservation[] {
                new FilePreservation(this._setting.Preservation),
                new ItemPreservation(this._setting.Preservation)
            };
            await this._fallBackPolicy.ExecuteAsync(async () =>
            {
                await RecoverAllConfigs();
            });
        }
        private NDisconfSetting GetSetting(string path)
        {
#if NETSTANDARD2_0
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(path, false, false)
                .Build();
            return new ServiceCollection()
                .AddOptions()
                .Configure<NDisconfSetting>(config)
                .BuildServiceProvider()
                .GetService<IOptions<NDisconfSetting>>()
                .Value;
#else
            return (NDisconfSetting)System.Configuration.ConfigurationManager.GetSection(path);
#endif
        }
        private async Task RecoverAllConfigs()
        {
            var zkHost = await this._fetcher.GetZkHosts();
            var filter = this.GetFilter<FetchFilter>();
            var ltimeFromServer = await this._fetcher.GetLastChangedTime(filter);
            IDictionary<ConfigType, IDictionary<string, string>> configs;
            if (this.NeedDownload(ltimeFromServer))
            {
                configs = this._preservations.ToDictionary(k => k.ConfigType, v => v.GetFromLocal());
            }
            else
            {
                configs = await this.GetConfigsFromServer(filter);
            }
            //移除要忽略的选项
            this.RemoveIgnores(new Dictionary<ConfigType, IEnumerable<string>>
            {
                { ConfigType.File,this._setting.UpdateStrategy.FileIgnoreList},
                { ConfigType.Item,this._setting.UpdateStrategy.ItemIgnoreList},
            }, configs);
            this.Refresh(configs);
        }
        private bool NeedDownload(DateTime ltimeFromServer)
        {
            if (this._setting.UpdateStrategy.ForcedDownload)
            {
                return true;
            }
            var ltimeFromLocal = this._preservations.Max(p => p.LastWriteTime);
            return ltimeFromLocal > DateTime.Now || ltimeFromLocal < ltimeFromServer;
        }
        private async Task<IDictionary<ConfigType, IDictionary<string, string>>> GetConfigsFromServer(FetchFilter filter)
        {
            var configs = await this._fetcher.GetAllConfigs(filter);
            if (configs != null && configs.Count > 0)
            {
                //本地持久化
                foreach (var kv in configs)
                {
                    this._preservations.FirstOrDefault(p => p.ConfigType == kv.Key)?.WriteAll(kv.Value);
                }
            }
            return configs;
        }
        private void RemoveIgnores(IDictionary<ConfigType, IEnumerable<string>> ignores, IDictionary<ConfigType, IDictionary<string, string>> configs)
        {
            foreach (var ignore in ignores)
            {
                if (ignore.Value != null && configs.ContainsKey(ignore.Key))
                {
                    var dic = configs[ignore.Key];
                    if (dic == null) continue;
                    foreach (var key in ignore.Value)
                    {
                        dic.Remove(key);
                    }
                }
            }
        }
        private void Refresh(IDictionary<ConfigType, IDictionary<string, string>> configs)
        {
        }
        private T GetFilter<T>()
            where T : FetchFilter, new()
        {
            var info = this._setting.ClientInfo;
            return new T
            {
                AppName = info.AppName,
                Version = info.Version,
                Environment = info.Environment,
                ClientIdentity = info.ClientIdentity
            };
        }
    }
}
