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
using NLog;
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
        public const string LOGPREFIX = "NDisconfLog_";
        private NDisconfSetting _setting;
        private Policy _fallBackPolicy;
        private NodeWatcher _watcher;
        private IFetcher _fetcher;
        private IPreservation[] _preservations;
        private static readonly Logger _logger = LogManager.GetLogger(LOGPREFIX + "NDisconfManager");
        private readonly Dictionary<ConfigType, IRuleCollection<Rule>> _ruleDictionary;
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
            _fallBackPolicy = Policy.Handle<Exception>().Fallback((c, ct) => { }, (e, c) =>
            {
                StringBuilder tmp = new StringBuilder();
                if (c.Keys.Count > 0)
                {
                    tmp.AppendLine("Exception params:");
                    foreach (var key in c.Keys)
                    {
                        tmp.Append(key.PadRight(30));
                        tmp.AppendLine(c[key].ToString());
                    }
                }
                _logger.Error(e, tmp.ToString());
            });
            _ruleDictionary = new Dictionary<ConfigType, IRuleCollection<Rule>>
            {
                { ConfigType.File, FileRules },
                { ConfigType.Item, ItemRules },
            };
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
            if (this._setting.EnableRemote)
            {
                await this._fallBackPolicy.ExecuteAsync(async () =>
                {
                    await RecoverAllConfigs().ConfigureAwait(false);
                }, new Dictionary<string, object>
                {
                    { "Action","RecoverAllConfigs"},
                }).ConfigureAwait(false);
            }
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
            var filter = this.GetFilter<FetchFilter>();
            var ltimeFromServer = await this._fetcher.GetLastChangedTime(filter).ConfigureAwait(false);
            IDictionary<ConfigType, IDictionary<string, string>> configs;
            var recoverFromLocal = false;
            if (this.NeedDownload(ltimeFromServer))
            {
                recoverFromLocal = true;
                configs = this._preservations.ToDictionary(k => k.ConfigType, v => v.GetFromLocal());
            }
            else
            {
                configs = await this.GetConfigsFromServer(filter, ltimeFromServer).ConfigureAwait(false);
            }
            //移除要忽略的选项
            this.RemoveIgnores(new Dictionary<ConfigType, IEnumerable<string>>
            {
                { ConfigType.File,this._setting.UpdateStrategy.FileIgnoreList},
                { ConfigType.Item,this._setting.UpdateStrategy.ItemIgnoreList},
            }, configs);
            await this.Refresh(filter, configs, recoverFromLocal);
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
        private async Task<IDictionary<ConfigType, IDictionary<string, string>>> GetConfigsFromServer(FetchFilter filter, DateTime ltimeFromServer)
        {
            var configs = await this._fetcher.GetAllConfigs(filter).ConfigureAwait(false);
            if (configs != null && configs.Count > 0)
            {
                //本地持久化
                foreach (var kv in configs)
                {
                    var pre = this._preservations.FirstOrDefault(p => p.ConfigType == kv.Key);
                    if (pre != null)
                    {
                        pre.WriteAll(kv.Value);
                        pre.LastWriteTime = ltimeFromServer;
                    }
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
        private async Task Refresh(AppInfo info, IDictionary<ConfigType, IDictionary<string, string>> configs, bool recoverFromLocal)
        {
            var builders = configs.Select(c =>
            {
                var builder = new ZkTreeBuilder(info, c.Key, this._setting.ZookeeperBasePath, this._setting.ClientInfo.IgnoreCase);
                IRuleCollection<Rule> rule = null;
                if (this._ruleDictionary.ContainsKey(c.Key))
                {
                    rule = this._ruleDictionary[c.Key];
                }
                foreach (var kv in c.Value)
                {
                    builder.GetOrAddZnodeName(kv.Key);
                    rule?.For(kv.Key);
                    this._fallBackPolicy.Execute(() =>
                    {
                        this.TryNoticeChanged(c.Key, kv.Key, kv.Value);
                    }, new Dictionary<string, object>
                    {
                        { "Action","TryNoticeChangedOnRefresh"},
                        { "ConfigType",c.Key},
                        { kv.Key,kv.Value}
                    });
                }
                return builder;
            }).ToArray();
            await this.AddWatcher(builders).ConfigureAwait(false);
        }
        private async Task AddWatcher(params IZkTreeBuilder[] builders)
        {
            var signatureData = new SignatureData
            {
                HashAlgorithm = this._setting.Security.HashAlgorithm
            };
            signatureData.SetSecretKey(this._setting.Security.SecretKey);
            var zkHost = await this._fetcher.GetZkHosts(signatureData).ConfigureAwait(false);
            this._watcher = new NodeWatcher(zkHost, this._setting.ZookeeperSessionTimeout, this._setting.ClientInfo.ClientIdentity, builders);
            this._watcher.NodeChanged += Watcher_NodeChanged;
            this._watcher.StartConnect();
        }

        private void Watcher_NodeChanged(ConfigType configType, string configName)
        {
            this._fallBackPolicy.Execute(async () =>
            {
                var filter = this.GetFilter<ConfigFetchFilter>();
                filter.ConfigType = configType;
                filter.ConfigName = configName;
                var content = await this._fetcher.GetConfig(filter).ConfigureAwait(false);
                var pre = this._preservations.FirstOrDefault(p => p.ConfigType == configType);
                if (pre != null)
                {
                    pre.Save(configName, content);
                    pre.LastWriteTime = await this._fetcher.GetLastChangedTime(filter).ConfigureAwait(false);
                }
                this.TryNoticeChanged(configType, configName, content);
            }, new Dictionary<string, object>
            {
                { "Action","Watcher_NodeChanged"},
                { "ConfigType",configType},
                { "ConfigName",configName}
            });
        }
        private void TryNoticeChanged(ConfigType configType, string configName, string content)
        {
            if (this._ruleDictionary.ContainsKey(configType))
            {
                this._ruleDictionary[configType].TryNoticeChanged(configName, content);
            }
        }

        private T GetFilter<T>()
            where T : FetchFilter, new()
        {
            var info = this._setting.ClientInfo;
            var t = new T
            {
                AppName = info.AppName,
                Version = info.Version,
                Environment = info.Environment,
                ClientIdentity = info.ClientIdentity,
                HashAlgorithm = this._setting.Security.HashAlgorithm
            };
            t.SetSecretKey(this._setting.Security.SecretKey);
            return t;
        }
    }
}
