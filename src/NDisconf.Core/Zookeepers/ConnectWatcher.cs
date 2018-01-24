using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;

namespace NDisconf.Core.Zookeepers
{
    /// <summary>
    /// zk的连接watcher，负责处理zk连接相关的逻辑
    /// </summary>
    public class ConnectWatcher : Watcher, IDisposable
    {
        #region event
        /// <summary>
        /// zookeeper连接成功时委托
        /// </summary>
        /// <param name="connectedNumbers">为自初始化以来与zk的连接次数，从1开始</param>
        /// <param name="isExpired">表示当次连接是否是因为Expired，true表示是</param>
        public delegate void ConnectedHandler(long connectedNumbers, bool isExpired);
        /// <summary>
        /// zookeeper连接成功时触发的事件
        /// </summary>
        public event ConnectedHandler Connected;
        #endregion
        #region fields
        protected ZooKeeper _zooKeeper;
        private string _connectionString;
        private int _sessionTimeout;
        protected long _connectedTimes = 0;
        protected long _prevSessionId = -1;
        #endregion
        /// <summary>
        /// 初始化watcher
        /// </summary>
        /// <param name="connectionString">zk连接字符串</param>
        /// <param name="sessionTimeout">zk会话超时时间</param>
        public ConnectWatcher(string connectionString, int sessionTimeout)
        {
            this._connectionString = connectionString;
            this._sessionTimeout = sessionTimeout;
        }
        /// <summary>
        /// 开始连接zk服务器，注意调用此方法并不会阻塞等待zk连接成功，而是通过Connected返回成功标志
        /// </summary>
        public void StartConnect()
        {
            if (!IsAlive())
            {
                this.Dispose();
                this._zooKeeper = new ZooKeeper(this._connectionString, this._sessionTimeout, this);
            }
        }
        /// <summary>
        /// 关闭zk连接
        /// </summary>
        public async void Dispose()
        {
            if (this._zooKeeper != null)
            {
                await this._zooKeeper.closeAsync();
            }
        }
        /// <summary>
        /// zk连接成功
        /// </summary>
        protected virtual void OnConnected()
        {
            Interlocked.Increment(ref _connectedTimes);
            var sessionId = this._zooKeeper.getSessionId();
#if DEBUG
            Console.WriteLine("SessionId: " + sessionId);
#endif
            Connected?.Invoke(_connectedTimes, this._prevSessionId != -1 && this._prevSessionId != sessionId);
            this._prevSessionId = sessionId;
        }
        /// <summary>
        /// Processes the specified event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public override Task process(WatchedEvent @event)
        {
#if DEBUG
            Console.WriteLine("KeeperState: " + @event.getState());
#endif
            switch (@event.getState())
            {
                case Event.KeeperState.SyncConnected:
                    if (@event.get_Type() == Event.EventType.None)
                    {
                        this.OnConnected();
                    }
                    break;
                case Event.KeeperState.Expired:
                    this.StartConnect();
                    break;
            }
            return Task.FromResult(1);
        }
        private bool IsAlive()
        {
            if (this._zooKeeper == null)
            {
                return false;
            }
            var state = this._zooKeeper.getState();
            return state != ZooKeeper.States.AUTH_FAILED && state != ZooKeeper.States.CLOSED;
        }
        /// <summary>
        /// 创建Ids.OPEN_ACL_UNSAFE以及CreateMode.PERSISTENT的节点
        /// </summary>
        /// <param name="path">zk路径</param>
        /// <param name="data">节点数据</param>
        /// <param name="watch">节点是否需要监视，默认false</param>
        /// <returns>如果创建了节点，则返回true，代表原先节点不存在，注意只要无异常，最终节点一定是存在的</returns>
        public async Task<bool> CreateZnodeAsync(string path, byte[] data = null, bool watch = false)
        {
            var exists = await this._zooKeeper.existsAsync(path, watch).ConfigureAwait(false) == null;
            if (!exists)
            {
                var tmpPath = Path.GetDirectoryName(path).Replace("\\", "/");
                if (tmpPath != string.Empty && tmpPath != "/")
                {
                    await this.CreateZnodeAsync(tmpPath, null).ConfigureAwait(false);
                }
                await this._zooKeeper.createAsync(path, data, Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT).ConfigureAwait(false);
            }
            return !exists;
        }
    }
}
