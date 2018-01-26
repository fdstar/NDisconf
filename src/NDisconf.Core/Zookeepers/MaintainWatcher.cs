using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NDisconf.Core.Zookeepers
{
    /// <summary>
    /// 服务端执行的Watcher，主要负责节点数据变更
    /// </summary>
    public class MaintainWatcher : ConnectWatcher
    {
        private static Task _jobTask;
        private static readonly ConcurrentQueue<Func<Task>> _queue = new ConcurrentQueue<Func<Task>>();
        private static readonly object _lockObj = new object();
        /// <summary>
        /// znode设置类
        /// </summary>
        /// <param name="connectionString">zk连接字符串</param>
        /// <param name="sessionTimeout">zk会话超时时间</param>
        public MaintainWatcher(string connectionString, int sessionTimeout)
            : base(connectionString, sessionTimeout)
        {
            this.Connected += MaintainWatcher_Connected;
        }
        private void MaintainWatcher_Connected(long connectedNumbers, bool isExpired)
        {
            StartJob();
        }
        /// <summary>
        /// 为节点更新数据，如果节点不存在，则新增节点
        /// </summary>
        /// <param name="zkPath"></param>
        /// <param name="data"></param>
        public void AddOrSetData(string zkPath, byte[] data)
        {
            _queue.Enqueue(async () =>
            {
                if (!await this.CreateZnodeAsync(zkPath, data).ConfigureAwait(false))
                {
                    await this.RemoveChildNode(zkPath).ConfigureAwait(false);//先删除子节点，再更新值保证不会出现客户端已经更新完并新增了节点，而服务端还没删完的情况
                    await this._zooKeeper.setDataAsync(zkPath, data, -1).ConfigureAwait(false);
                }
            });
            StartJob();
        }
        private void StartJob()
        {
            lock (_lockObj)
            {
                if (_jobTask == null || _jobTask.IsCompleted || _jobTask.IsFaulted || _jobTask.IsCanceled)
                {
                    _jobTask?.Dispose();
                    _jobTask = Task.Run(async () =>
                    {
                        while (!_queue.IsEmpty)
                        {
                            if (_queue.TryPeek(out Func<Task> func))
                            {
                                await func().ConfigureAwait(false);
                                _queue.TryDequeue(out func);
                            }
                        }
                    });
                }
            }
        }
        /// <summary>
        /// 移除临时子节点，表明该节点目前已更新，客户端需要重新下载
        /// </summary>
        /// <param name="path"></param>
        private async Task RemoveChildNode(string path)
        {
            var childs = await this._zooKeeper.getChildrenAsync(path, false).ConfigureAwait(false);
            if (childs != null && childs.Children != null && childs.Children.Count > 0)
            {
                foreach (var child in childs.Children)
                {
                    await this._zooKeeper.deleteAsync(string.Format("{0}/{1}", path, child), -1).ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// 删除zk节点
        /// </summary>
        /// <param name="zkPath"></param>
        public void Remove(string zkPath)
        {
            _queue.Enqueue(async () =>
            {
                await this.RemoveChildNode(zkPath).ConfigureAwait(false);//zookeeper在存在子节点时，不允许直接删除父节点，所以需要先删除子节点
                await this._zooKeeper.deleteAsync(zkPath, -1).ConfigureAwait(false);
            });
        }
    }
}
