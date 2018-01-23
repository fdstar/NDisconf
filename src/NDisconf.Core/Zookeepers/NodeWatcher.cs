using NDisconf.Core.Entities;
using org.apache.zookeeper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static org.apache.zookeeper.KeeperException;
using static org.apache.zookeeper.ZooDefs;

namespace NDisconf.Core.Zookeepers
{
    /// <summary>
    /// 客户端执行的wather，负责相关节点的监控及变更通知
    /// </summary>
    public class NodeWatcher : ConnectWatcher
    {
        #region event
        /// <summary>
        /// 节点数据变更时委托
        /// </summary>
        /// <param name="configType">配置类型</param>
        /// <param name="configName">配置节点名称</param>
        public delegate void NodeChangedHandler(ConfigType configType, string configName);
        /// <summary>
        /// 节点数据变更时触发事件
        /// </summary>
        public event NodeChangedHandler NodeChanged;
        #endregion

        #region fields
        private long _lastMtime;
        private byte[] _clientIdentity;
        private IZkTreeBuilder[] _builders;
        private static readonly string clientNodeIdentity = Guid.NewGuid().ToString();
        private ConcurrentDictionary<string, object> _dictionary = new ConcurrentDictionary<string, object>();
        #endregion
        /// <summary>
        /// NodeWatcher
        /// </summary>
        /// <param name="connectstring">zk连接字符串</param>
        /// <param name="sessionTimeout">zk会话超时时间</param>
        /// <param name="clientIdentity">客户端唯一标志，建议传递，默认为null时取默认的机器名</param>
        /// <param name="builders">zk树构建类</param>
        public NodeWatcher(string connectstring, int sessionTimeout, string clientIdentity = null, params IZkTreeBuilder[] builders)
            : base(connectstring, sessionTimeout)
        {
            if (builders == null || builders.Length == 0)
            {
                throw new ArgumentNullException("builders can not be null");
            }
            this._builders = builders;
            this._clientIdentity = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(clientIdentity) ? Environment.MachineName : clientIdentity.Trim());
            this.Connected += NodeWatcher_Connected;
        }

        private async void NodeWatcher_Connected(long connectedNumbers, bool isExpired)
        {
            if (connectedNumbers == 1 || isExpired)
            {//如果是第一次连接成功，或者本次连接是因为Expired，那么需要注册节点监控
                var compareTime = this._lastMtime;
                var notifyList = new List<KeyValuePair<ConfigType, HashSet<string>>>();
                foreach (var builder in this._builders)
                {
                    var configs = new HashSet<string>();
                    foreach (var znode in builder.GetAllZnodes())
                    {
                        var path = builder.GetZkPathByZnodeName(znode);
                        var needNotify = await this.AddWatcherAndAddUpdatedNode(path, compareTime, true).ConfigureAwait(false);
                        if (needNotify)
                        {
                            configs.Add(builder.GetConfigNameByZnodeName(znode));
                        }
                    }
                    notifyList.Add(new KeyValuePair<ConfigType, HashSet<string>>(builder.ConfigType, configs));
                }
                if (isExpired && this.NodeChanged != null)
                {
                    foreach (var kv in notifyList)
                    {
                        foreach (var configName in kv.Value)
                        {
                            this.NodeChanged(kv.Key, configName);
                        }
                    }
                }
            }
            foreach (var path in this._dictionary.Keys)
            {
                //补偿因ConnectionLossException导致的监控节点或添加临时节点失败
                try
                {
                    this._dictionary.TryRemove(path, out object value);
                    await AddWatcherAndAddUpdatedNode(path, this._lastMtime).ConfigureAwait(false);
                }
                catch { };
            }
        }

        /// <summary>
        /// Processes the specified event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public override async Task process(WatchedEvent @event)
        {
            await base.process(@event);
            var path = @event.getPath();
            if (@event.get_Type() == Event.EventType.NodeDataChanged
                && this.NodeChanged != null && !string.IsNullOrWhiteSpace(path))
            {

                var znodeName = Path.GetFileName(path);
                foreach (var builder in this._builders)
                {
                    if (builder.GetZkPathByZnodeName(znodeName) == path)
                    {
                        var configName = builder.GetConfigNameByZnodeName(znodeName);
                        if (configName != null)
                        {
                            this._dictionary.TryRemove(path, out object value);
                            this.NodeChanged?.Invoke(builder.ConfigType, configName);
                            //等待NodeChanged更新完后才继续监控
                            try
                            {
                                await AddWatcherAndAddUpdatedNode(path, this._lastMtime).ConfigureAwait(false);
                            }
                            catch { };
                            break;
                        }
                    }
                }
            }
        }
        private async Task<bool> AddWatcherAndAddUpdatedNode(string path, long compareTime, bool createIfNodeNotExists = false)
        {
            try
            {
                bool needNotify = true;
                if (createIfNodeNotExists)
                {
                    needNotify = !await this.CreateZnodeAsync(path).ConfigureAwait(false);
                }
                var stat = await this._zooKeeper.existsAsync(path, true).ConfigureAwait(false);
                if (stat != null)
                {
                    var mtime = stat.getMtime();
                    await this.AddTmpChildNode(path).ConfigureAwait(false);
                    if (needNotify)
                    {
                        this._lastMtime = Math.Max(this._lastMtime, mtime);
                    }
                    return needNotify && compareTime > 0 && compareTime < mtime;
                }
            }
            catch (ConnectionLossException clEx)
            {
                this._dictionary.TryAdd(path, null);
            }
            return false;
        }
        private async Task AddTmpChildNode(string path)
        {
            //添加监控时同时增加临时节点，表明客户端已经下载过节点数据，删除节点部分工作由服务端进行
            string nodePath = string.Format("{0}/{1}", path, clientNodeIdentity);
            if (await this._zooKeeper.existsAsync(nodePath).ConfigureAwait(false) == null)
            {
                await this._zooKeeper.createAsync(nodePath, this._clientIdentity, Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL).ConfigureAwait(false);//注意使用的是临时节点
            }
        }
    }
}
